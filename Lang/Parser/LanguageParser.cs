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

        public LanguageParser(Tokenizer tokenizer)
        {
            TokenStream = new ParseableTokenStream(tokenizer);

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

                        var body = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

                        return new ClassAst(className, body);
                    }

                    return null;
                };

            if (TokenStream.Alt(classTaker))
            {
                return TokenStream.Get(classTaker);
            }

            return null;
        }

        #region Single statement 
        
        /// <summary>
        /// Method declaration or regular statement
        /// </summary>
        /// <returns></returns>
        private Ast Statement()
        {
            var classDeclr = Class();

            if (classDeclr != null)
            {
                return classDeclr;
            }

            if (TokenStream.Alt(MethodDeclaration))
            {
                return TokenStream.Get(MethodDeclaration);
            }

            var statement = Expression();

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
        private Ast Expression()
        {
            // ordering here matters since it resolves to precedence
            var ast = ScopeStart().Or(LambdaStatement)
                                  .Or(VariableDeclWithAssignStatement)
                                  .Or(VariableDeclrStatement)
                                  .Or(GetIf)
                                  .Or(GetWhile)
                                  .Or(GetFor)
                                  .Or(GetReturn)
                                  .Or(PrintStatement)
                                  .Or(New)
                                  .Or(OperationExpression);

            if (ast != null)
            {
                return ast;
            }

            throw new InvalidSyntax(String.Format("Unknown expression type {0} - {1}", TokenStream.Current.TokenType, TokenStream.Current.TokenValue));
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

                    return null;
                };

            return TokenStream.Capture(op);
        }

        private Ast ClassReferenceStatement()
        {
            Func<Ast> reference = () =>
                {
                    var references = new List<Ast>();

                    var classInstance = new Expr(TokenStream.Take(TokenType.Word));

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
                var statements = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

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

                    var expr = Expression();

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

        private Ast OperationExpression()
        {
            if (IsValidOperand())
            {
                return ParseOperationExpression();
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

                    if (TokenStream.Alt(doubleOp))
                    {
                        return TokenStream.Get(doubleOp);
                    }
                    
                    if (TokenStream.Alt(basicOp))
                    {
                        return TokenStream.Get(basicOp);
                    }

                    break;
                default:
                    return null;
            }

            return null;
        }

        private Ast ParseOperationExpression()
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

            Func<Ast> leftOp = () => op(ConsumeFinalExpression, OperationExpression);
            Func<Ast> rightOp = () => op(OperationExpression, ConsumeFinalExpression);
            
            if (TokenStream.Alt(leftOp))
            {
                return TokenStream.Get(leftOp);
            }

            if (TokenStream.Alt(ConsumeFinalExpression))
            {
                return TokenStream.Get(ConsumeFinalExpression);
            }

            if (TokenStream.Alt(rightOp))
            {
                return TokenStream.Get(rightOp);
            }

            return null;
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

            return new ReturnAst(Expression());
        }


        #endregion

        #region Conditionals and Loops

        private Ast GetWhile()
        {
            if (TokenStream.Current.TokenType == TokenType.While && TokenStream.Alt(ParseWhile))
            {
                return TokenStream.Get(ParseWhile);
            }

            return null;
        }

        private Ast GetIf()
        {
            if (TokenStream.Current.TokenType == TokenType.If && TokenStream.Alt(ParseIf))
            {
                return TokenStream.Get(ParseIf);
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

            var body = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

            return new ForLoop(init, condition, modify, body);
        }

        private WhileLoop ParseWhile()
        {
            var predicateAndExpressions = GetPredicateAndExpressions(TokenType.While);

            var predicate = predicateAndExpressions.Item1;
            var statements = predicateAndExpressions.Item2;

            return new WhileLoop(predicate, statements);
        }


        private Conditional ParseIf()
        {
            var predicateAndExpressions = GetPredicateAndExpressions(TokenType.If);

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

            var statements = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

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

            var lines = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

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

            var innerExpressions = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

            return new MethodDeclr(returnType, funcName, argList, innerExpressions);
        }

        private List<Ast> GetArgumentList(bool includeType = false)
        {
            TokenStream.Take(TokenType.OpenParenth);

            var args = new List<Ast>(64);

            while (TokenStream.Current.TokenType != TokenType.CloseParenth)
            {
                var argument = includeType ? VariableDeclaration() : Expression();

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

                return new VarDeclrAst(type, name, Expression());
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

            return new Expr(new Expr(name), equals, Expression());
        }

        #endregion

        #region Single Expressions or Tokens

        private Ast ConsumeFinalExpression()
        {
            return ClassReferenceStatement().Or(FunctionCallStatement).Or(VariableAssignmentStatement)
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

        private Tuple<Ast, ScopeDeclr> GetPredicateAndExpressions(TokenType type)
        {
            TokenStream.Take(type);

            TokenStream.Take(TokenType.OpenParenth);

            var predicate = Expression();

            TokenStream.Take(TokenType.CloseParenth);

            var statements = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

            return new Tuple<Ast, ScopeDeclr>(predicate, statements);
        } 

        private ScopeDeclr GetExpressionsInScope(TokenType open, TokenType close, bool expectSemicolon = true)
        {
            TokenStream.Take(open);
            var lines = new List<Ast>();
            while (TokenStream.Current.TokenType != close)
            {
                var statement = Expression();

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
