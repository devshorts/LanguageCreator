using System;
using System.Collections.Generic;
using System.Linq;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;
using Lang.Utils;

namespace Lang.Parser
{
    public class LanguageParser
    {
        private ParseableTokenStream TokenStream { get; set; }

        public LanguageParser(Lexers.Lexer lexer)
        {
            TokenStream = new ParseableTokenStream(lexer);

            // we'll tag all lambdas we find starting from 1000 here
            // later when we iterate over scope and create anonnymous lambdas
            // we need the lambdas to have the SAME name even if we iterate over
            // the syntax tree multiple times. this is hacky, i know.
            // curried functions will be labeled from 0 to 1000
            LambdaDeclr.LambdaCount = 1000;
        }

        public Ast Parse()
        {
            var statements = new List<Ast>(1024);

            while (TokenStream.Current.TokenType != TokenType.EOF)
            {
                statements.Add(ScopeStart().Or(Statement));
            }

            return new ScopeDeclr(statements);
        }


        #region Statement Parsers

        private Ast Class()
        {
            Func<Ast> classTaker = () =>
                {
                    if (TokenStream.Current.TokenType == TokenType.Class)
                    {
                        TokenStream.Take(TokenType.Class);

                        var className = TokenStream.Take(TokenType.Word);

                        var body = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket, Statement, false);

                        return new ClassAst(className, body);
                    }

                    return null;
                };

            return TokenStream.Capture(classTaker);
        }

        #region Single statement 

        /// <summary>
        /// Class, method declaration or inner statements
        /// </summary>
        /// <returns></returns>
        private Ast Statement()
        {
            var ast = TokenStream.Capture(Class)
                                 .Or(() => TokenStream.Capture(MethodDeclaration));

            if (ast != null)
            {
                return ast;
            }

            // must be an inner statement if the other two didn't pass
            // these are statements that can be inside of scopes such as classes
            // methods, or just global scope
            var statement = InnerStatement();

            if (TokenStream.Current.TokenType == TokenType.SemiColon)
            {
                TokenStream.Take(TokenType.SemiColon);
            }

            return statement;
        }


        /// <summary>
        /// A statement inside of a valid scope 
        /// </summary>
        /// <returns></returns>
        private Ast InnerStatement()
        {
            // ordering here matters since it resolves to precedence
            var ast = TryCatch().Or(ScopeStart)
                                .Or(LambdaStatement)
                                .Or(VariableDeclWithAssignStatement)
                                .Or(VariableDeclrStatement)
                                .Or(GetIf)
                                .Or(GetWhile)
                                .Or(GetFor)
                                .Or(GetReturn)
                                .Or(PrintStatement)
                                .Or(Expression)
                                .Or(New);

            if (ast != null)
            {
                return ast;
            }

            throw new InvalidSyntax(String.Format("Unknown expression type {0} - {1}", TokenStream.Current.TokenType, TokenStream.Current.TokenValue));
        }

        private Ast TryCatch()
        {
            if (TokenStream.Current.TokenType == TokenType.Try)
            {
                TokenStream.Take(TokenType.Try);

                var tryBody = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket);

                ScopeDeclr catchBody = null;
                if (TokenStream.Current.TokenType == TokenType.Catch)
                {
                    TokenStream.Take(TokenType.Catch);

                    catchBody = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket);
                }

                return new TryCatchAst(tryBody, catchBody);
            }

            return null;
        }

        private Ast New()
        {
            Func<Ast> op = () =>
                {
                    if (TokenStream.Current.TokenType == TokenType.New)
                    {
                        TokenStream.Take(TokenType.New);

                        var name = new Expr(TokenStream.Take(TokenType.Word));

                        var args = GetArgumentList();

                        return new NewAst(name, args);
                    }

                    if (TokenStream.Current.TokenType == TokenType.OpenParenth &&
                        TokenStream.Peek(1).TokenType == TokenType.New)
                    {
                        TokenStream.Take(TokenType.OpenParenth);

                        var item = New();

                        TokenStream.Take(TokenType.CloseParenth);

                        return item;
                    }

                    return null;
                };

            return TokenStream.Capture(op);
        }

        private Ast ClassReferenceStatement()
        {
            Func<Ast> reference = () =>
                {
                    var references = new List<Ast>();
                    
                    var classInstance = New().Or(() => new Expr(TokenStream.Take(TokenType.Word)));

                    while (true)
                    {
                        if (TokenStream.Current.TokenType == TokenType.Dot)
                        {
                            TokenStream.Take(TokenType.Dot);
                        }
                        else
                        {
                            if (references.Count == 0)
                            {
                                return null;
                            }

                            if (references.Count > 0)
                            {
                                return new ClassReference(classInstance, references);
                            }
                        }

                        var deref = FunctionCallStatement().Or(() => TokenStream.Current.TokenType == TokenType.Word ? new Expr(TokenStream.Take(TokenType.Word)) : null);

                        references.Add(deref);   
                    }
                };

            return TokenStream.Capture(reference);
        }

        private Ast ScopeStart()
        {
            if (TokenStream.Current.TokenType == TokenType.LBracket)
            {
                var statements = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket);

                return statements;
            }

            return null;
        }

        #endregion

        #region Print

        private Ast PrintStatement()
        {
            Func<Ast> op = () =>
                {
                    TokenStream.Take(TokenType.Print);

                    var expr = InnerStatement();

                    if (expr != null)
                    {
                        return new PrintAst(expr);
                    }

                    return null;
                };

            if (TokenStream.Alt(op))
            {
                return TokenStream.Get(op);
            }

            return null;
        }

        #endregion


        #region Expressions of single items or expr op expr

        private Ast Expression()
        {
            if (IsValidOperand() || TokenStream.Current.TokenType == TokenType.New)
            {
                return ParseExpression();
            }

            switch (TokenStream.Current.TokenType)
            {
                case TokenType.OpenParenth:

                    Func<Ast> basicOp = () =>
                        {
                            TokenStream.Take(TokenType.OpenParenth);

                            var expr = Expression();

                            TokenStream.Take(TokenType.CloseParenth);

                            return expr;
                        };

                    Func<Ast> doubleOp = () =>
                        {
                            var op1 = basicOp();

                            var op = Operator();

                            var expr = Expression();

                            return new Expr(op1, op, expr);
                        };

                    return TokenStream.Capture(doubleOp)
                                      .Or(() => TokenStream.Capture(basicOp));

                default:
                    return null;
            }
        }

        private Ast ParseExpression()
        {
            Func<Func<Ast>, Func<Ast>, Ast> op = (leftFunc, rightFunc) =>
                {
                    var left = leftFunc();

                    if (left == null)
                    {
                        return null;
                    }

                    var opType = Operator();

                    var right = rightFunc();

                    if (right == null)
                    {
                        return null;
                    }

                    return new Expr(left, opType, right);
                };

            Func<Ast> leftOp = () => op(ExpressionTerminal, Expression);

            return TokenStream.Capture(leftOp)
                              .Or(() => TokenStream.Capture(ExpressionTerminal));
        }

        #endregion

        #region Return

        private Ast GetReturn()
        {
            if (TokenStream.Current.TokenType == TokenType.Return && TokenStream.Alt(ParseReturn))
            {
                return TokenStream.Get(ParseReturn);
            }

            return null;
        }

        private ReturnAst ParseReturn()
        {
            TokenStream.Take(TokenType.Return);

            if (TokenStream.Current.TokenType == TokenType.SemiColon)
            {
                return new ReturnAst();
            }

            return new ReturnAst(InnerStatement());
        }


        #endregion

        #region Conditionals and Loops

        private Ast GetWhile()
        {
            if (TokenStream.Current.TokenType == TokenType.While)
            {
                Func<WhileLoop> op = () =>
                    {
                        var predicateAndStatements = GetPredicateAndStatements(TokenType.While);

                        var predicate = predicateAndStatements.Item1;

                        var statements = predicateAndStatements.Item2;

                        return new WhileLoop(predicate, statements);
                    };

                return TokenStream.Capture(op);
            }

            return null;
        }

        private Ast GetIf()
        {
            if (TokenStream.Current.TokenType == TokenType.If)
            {
                return TokenStream.Capture(ParseIf);
            }

            return null;
        }


        private Ast GetFor()
        {
            if (TokenStream.Current.TokenType == TokenType.For && TokenStream.Alt(ParseFor))
            {
                return TokenStream.Get(ParseFor);
            }

            return null;
        }

        private ForLoop ParseFor()
        {
            TokenStream.Take(TokenType.For);

            var args = GetArgumentList();

            var init = args[0];

            var condition = args[1];

            var modify = args[2];

            var body = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket);

            return new ForLoop(init, condition, modify, body);
        }

        private Conditional ParseIf()
        {
            var predicateAndExpressions = GetPredicateAndStatements(TokenType.If);

            var predicate = predicateAndExpressions.Item1;
            var statements = predicateAndExpressions.Item2;

            // no else following, then just basic if statement
            if (TokenStream.Current.TokenType != TokenType.Else)
            {
                return new Conditional(new Token(TokenType.If), predicate, statements);
            }

            // we found an else if scenario
            if (TokenStream.Peek(1).TokenType == TokenType.If)
            {
                TokenStream.Take(TokenType.Else);

                var alternate = ParseIf();

                return new Conditional(new Token(TokenType.If), predicate, statements, alternate);
            }

            // found a trailing else
            return new Conditional(new Token(TokenType.If), predicate, statements, ParseTrailingElse());
        }

        private Conditional ParseTrailingElse()
        {
            TokenStream.Take(TokenType.Else);

            var statements = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket);

            return new Conditional(new Token(TokenType.Else), null, statements);
        }

        #endregion

        #region Function parsing (lambdas, declarations, arguments)

        private Ast FunctionCall()
        {
            var name = TokenStream.Take(TokenType.Word);

            var args = GetArgumentList();

            return new FuncInvoke(name, args);
        }


        private Ast Lambda()
        {
            TokenStream.Take(TokenType.Fun);

            var arguments = GetArgumentList(true);

            TokenStream.Take(TokenType.DeRef);

            var lines = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket);

            var method = new LambdaDeclr(arguments, lines);

            return method;
        }


        private Ast MethodDeclaration()
        {
            if (!IsValidMethodReturnType())
            {
                throw new InvalidSyntax("Invalid syntax");
            }

            // return type
            var returnType = TokenStream.Take(TokenStream.Current.TokenType);

            // func name
            var funcName = TokenStream.Take(TokenType.Word);

            var argList = GetArgumentList(true);

            var innerExpressions = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket);

            return new MethodDeclr(returnType, funcName, argList, innerExpressions);
        }

        private List<Ast> GetArgumentList(bool includeType = false)
        {
            TokenStream.Take(TokenType.OpenParenth);

            var args = new List<Ast>(64);

            while (TokenStream.Current.TokenType != TokenType.CloseParenth)
            {
                var argument = includeType ? VariableDeclaration() : InnerStatement();

                args.Add(argument);

                if (TokenStream.Current.TokenType == TokenType.Comma || TokenStream.Current.TokenType == TokenType.SemiColon)
                {
                    TokenStream.Take(TokenStream.Current.TokenType);
                }
            }

            TokenStream.Take(TokenType.CloseParenth);

            return args;
        }

        #endregion

        #region Variable Declrations and Assignments

        private Ast VariableDeclarationAndAssignment()
        {
            var isVar = TokenStream.Current.TokenType == TokenType.Infer;

            if ((isVar || IsValidMethodReturnType()) && IsValidVariableName(TokenStream.Peek(1)))
            {
                var type = TokenStream.Take(TokenStream.Current.TokenType);

                var name = TokenStream.Take(TokenType.Word);

                TokenStream.Take(TokenType.Equals);

                bool isLink = false;
                if (TokenStream.Current.TokenType == TokenType.Ampersand)
                {
                    isLink = true;
                    TokenStream.Take(TokenType.Ampersand);
                }

                var expr = InnerStatement();

                expr.IsLink = isLink;

                return new VarDeclrAst(type, name, expr);
            }

            return null;
        }

        private Ast VariableDeclaration()
        {
            if (IsValidMethodReturnType() && IsValidVariableName(TokenStream.Peek(1)))
            {
                var type = TokenStream.Take(TokenStream.Current.TokenType);

                var name = TokenStream.Take(TokenType.Word);

                return new VarDeclrAst(type, name);
            }

            return null;
        }

        private Ast VariableAssignment()
        {
            var name = TokenStream.Take(TokenType.Word);

            var equals = TokenStream.Take(TokenType.Equals);

            return new Expr(new Expr(name), equals, InnerStatement());
        }

        #endregion

        #region Single Expressions or Tokens

        private Ast ExpressionTerminal()
        {
            return ClassReferenceStatement().Or(FunctionCallStatement)
                                            .Or(VariableAssignmentStatement)
                                            .Or(New)
                                            .Or(SingleToken);
        }

        private Ast SingleToken()
        {
            if (IsValidOperand())
            {
                var token = new Expr(TokenStream.Take(TokenStream.Current.TokenType));

                return token;
            }

            return null;
        }

        private Token Operator()
        {
            if (TokenUtil.IsOperator(TokenStream.Current))
            {
                return TokenStream.Take(TokenStream.Current.TokenType);
            }

            throw new InvalidSyntax(String.Format("Invalid token found. Expected operator but found {0} - {1}", TokenStream.Current.TokenType, TokenStream.Current.TokenValue));
        }

        #endregion

        #region Helpers

        private Tuple<Ast, ScopeDeclr> GetPredicateAndStatements(TokenType type)
        {
            TokenStream.Take(type);

            TokenStream.Take(TokenType.OpenParenth);

            var predicate = Expression();

            TokenStream.Take(TokenType.CloseParenth);

            var statements = GetStatementsInScope(TokenType.LBracket, TokenType.RBracket);

            return new Tuple<Ast, ScopeDeclr>(predicate, statements);
        }

        private ScopeDeclr GetStatementsInScope(TokenType open, TokenType close, bool expectSemicolon = true)
        {
            return GetStatementsInScope(open, close, InnerStatement, expectSemicolon);
        }

        private ScopeDeclr GetStatementsInScope(TokenType open, TokenType close, Func<Ast> getter, bool expectSemicolon = true)
        {
            TokenStream.Take(open);
            var lines = new List<Ast>();
            while (TokenStream.Current.TokenType != close)
            {
                var statement = getter();

                lines.Add(statement);

                if (expectSemicolon && StatementExpectsSemiColon(statement))
                {
                    TokenStream.Take(TokenType.SemiColon);
                }
            }

            TokenStream.Take(close);

            return new ScopeDeclr(lines);
        }
        

        private bool StatementExpectsSemiColon(Ast statement)
        {
            return !(statement is MethodDeclr || 
                     statement is Conditional || 
                     statement is WhileLoop || 
                     statement is TryCatchAst || 
                     statement is ForLoop);
        }

        #endregion

        #endregion

        #region TokenStream.Alternative Route Testers

        private Ast VariableDeclWithAssignStatement()
        {
            return TokenStream.Capture(VariableDeclarationAndAssignment);
        }

        private Ast VariableAssignmentStatement()
        {
            return TokenStream.Capture(VariableAssignment);
        }

        private Ast VariableDeclrStatement()
        {
            return TokenStream.Capture(VariableDeclaration);
        }

        private Ast FunctionCallStatement()
        {
            return TokenStream.Capture(FunctionCall);
        }

        private Ast LambdaStatement()
        {
            return TokenStream.Capture(Lambda);
        }

        private bool IsValidMethodReturnType()
        {
            switch (TokenStream.Current.TokenType)
            {
                case TokenType.Void:
                case TokenType.Word:
                case TokenType.Int:
                case TokenType.String:
                case TokenType.Infer:
                case TokenType.Method:
                case TokenType.Boolean:
                    return true;
            }
            return false;
        }

        private bool IsValidOperand()
        {
            switch (TokenStream.Current.TokenType)
            {
                case TokenType.Int:
                case TokenType.QuotedString:
                case TokenType.Word:
                case TokenType.True:
                case TokenType.Float:
                case TokenType.Nil:
                case TokenType.False:
                    return true;
            }
            return false;
        }

        private bool IsValidVariableName(Token item)
        {
            switch (item.TokenType)
            {
                case TokenType.Word:
                    return true;
            }
            return false;
        }

        

        #endregion
    }
}
