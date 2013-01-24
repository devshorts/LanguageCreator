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
        }

        public Ast Parse()
        {
            var statements = GetStatements(() => TokenStream.Current == null);

            return new ScopeDeclr(statements);
        }

        #region Entry Point

        /// <summary>
        /// List of statements in the main program body
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="end"></param>
        private List<Ast> GetStatements(Func<Boolean> end)
        {
            var aggregate = new List<Ast>(1024);

            while (!end())
            {
                if (TokenStream.Current.TokenType == TokenType.LBracket)
                {
                    var statements = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

                    aggregate.Add(new ScopeDeclr(statements));
                }
                else
                {
                    var statement = MainStatement();

                    aggregate.Add(statement);
                }
            }

            return aggregate;
        }

        /// <summary>
        /// Method declaration or regular statement
        /// </summary>
        /// <returns></returns>
        private Ast MainStatement()
        {
            if (IsMethodDeclaration())
            {
                return MethodDeclaration();
            }

            var statement = Statement();

            TokenStream.Take(TokenType.SemiColon);

            return statement;
        }

        #endregion

        #region Statement Parsers

        /// <summary>
        /// A statement inside of a valid scope 
        /// </summary>
        /// <returns></returns>
        private Ast Statement()
        {
            var ast = ScopeStart().Or(LambdaStatement)
                                  .Or(VariableDeclrStatement)
                                  .Or(VariableDeclWithAssignStatement)
                                  .Or(OperationExpression);

            if (ast != null)
            {
                return ast;
            }

            throw new InvalidSyntax(String.Format("Unknown expression type {0} - {1}", TokenStream.Current.TokenType, TokenStream.Current.TokenValue));
        }

        private Ast ScopeStart()
        {
            if (TokenStream.Current.TokenType == TokenType.LBracket)
            {
                var statements = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

                return new ScopeDeclr(statements);
            }

            return null;
        }

        private Ast OperationExpression()
        {
            switch (TokenStream.Current.TokenType)
            {
                case TokenType.QuotedString:
                case TokenType.Word:
                    return ParseOperationExpression();

                case TokenType.OpenParenth:
                    return GetExpressionsInScope(TokenType.OpenParenth, TokenType.CloseParenth, false).FirstOrDefault();

                default:
                    return null;
            }
        }

        private Ast GetFunctionCall()
        {
            var name = TokenStream.Take(TokenType.Word);

            var args = GetArgumentList();

            return new FuncInvoke(name, args);
        }

        private Ast ParseOperationExpression()
        {
            Func<Ast> op = () =>
                {
                    var left = FunctionCallStatement().Or(SingleToken);

                    return new Expr(left, TakeOperator(), Statement());
                };

            if(TokenStream.Alt(op))
            {
                return TokenStream.Get(op);
            }
            
            if (TokenStream.Alt(ConsumeFinalExpression))
            {
                return TokenStream.Get(ConsumeFinalExpression);
            }

            return null;
        }

        private List<Ast> GetExpressionsInScope(TokenType open, TokenType close, bool expectSemicolon = true)
        {
            TokenStream.Take(open);
            var lines = new List<Ast>();
            while (TokenStream.Current.TokenType != close)
            {
                lines.Add(Statement());

                if (expectSemicolon)
                {
                    TokenStream.Take(TokenType.SemiColon);
                }
            }

            TokenStream.Take(close);

            return lines;
        } 

        private Ast GetLambda()
        {
            TokenStream.Take(TokenType.Fun);
            TokenStream.Take(TokenType.OpenParenth);
            TokenStream.Take(TokenType.CloseParenth);
            TokenStream.Take(TokenType.DeRef);

            var lines = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

            var method = new MethodDeclr(new Token(TokenType.Fun), new Token(TokenType.Void),
                                         new Token(TokenType.Word, "anonymous"), null, lines);


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

            return new MethodDeclr(new Token(TokenType.ScopeStart), returnType, funcName, argList, innerExpressions);
        }

        private List<Ast> GetArgumentList(bool includeType = false)
        {
            TokenStream.Take(TokenType.OpenParenth);

            var args = new List<Ast>(64);

            while (TokenStream.Current.TokenType != TokenType.CloseParenth)
            {
                var argument = includeType ? GetVariableDeclaration() : Statement();

                args.Add(argument);

                if (TokenStream.Current.TokenType == TokenType.Comma)
                {
                    TokenStream.Take(TokenType.Comma);
                }
            }

            TokenStream.Take(TokenType.CloseParenth);

            return args;
        }

        private Ast VariableDeclarationAndAssignment()
        {
            var type = TokenStream.Take(TokenStream.Current.TokenType);

            var name = TokenStream.Take(TokenType.Word);

            TokenStream.Take(TokenType.Equals);

            return new VarDeclrAst(type, name, Statement());
        }

        private Ast GetVariableDeclaration()
        {
            var type = TokenStream.Take(TokenStream.Current.TokenType);

            var name = TokenStream.Take(TokenType.Word);

            return new VarDeclrAst(type, name);
        }

        private Ast VariableAssignment()
        {
            var name = TokenStream.Take(TokenType.Word);

            var equals = TokenStream.Take(TokenType.Equals);

            return new Expr(new Expr(name), equals, Statement());
        }

        private Ast ConsumeFinalExpression()
        {
            return FunctionCallStatement().Or(VariableAssignmentStatement)
                                          .Or(SingleToken);
        }

        private Ast SingleToken()
        {
            var token = new Expr(TokenStream.Take(TokenStream.Current.TokenType));

            return token;
        }

        private Token TakeOperator()
        {
            if (IsOperator(TokenStream.Current))
            {
                return TokenStream.Take(TokenStream.Current.TokenType);
            }

            throw new InvalidSyntax(String.Format("Invalid token found. Expected operator but found {0} - {1}", TokenStream.Current.TokenType, TokenStream.Current.TokenValue));

        }

        #endregion

        #region TokenStream.Alternative Route Testers


        private Ast VariableDeclWithAssignStatement()
        {
            if (IsVariableDeclarationWithAssignment())
            {
                return TokenStream.Get(VariableDeclarationAndAssignment);
            }

            return null;
        }

        private Ast VariableAssignmentStatement()
        {
            if (IsVariableAssignment())
            {
                return TokenStream.Get(VariableAssignment);
            }

            return null;
        }

        private Ast VariableDeclrStatement()
        {
            if (IsVariableDeclaration())
            {
                var declr = TokenStream.Get(GetVariableDeclaration);

                return declr;
            }

            return null;
        }

        private Ast FunctionCallStatement()
        {
            if (IsFunctionCall())
            {
                return TokenStream.Get(GetFunctionCall);
            }

            return null;
        }

        private Ast LambdaStatement()
        {
            if (IsLambda())
            {
                return TokenStream.Get(GetLambda);
            }

            return null;
        }

        private bool IsFunctionCall()
        {
            if (TokenStream.Current.TokenType == TokenType.Word)
            {
                return TokenStream.Alt(GetFunctionCall);
            }

            return false;
        }

        private bool IsMethodDeclaration()
        {
            if (IsValidMethodReturnType())
            {
                return TokenStream.Alt(MethodDeclaration);
            }

            return false;
        }

        private bool IsValidMethodReturnType()
        {
            switch (TokenStream.Current.TokenType)
            {
                case TokenType.Void:
                case TokenType.Word:
                case TokenType.Int:
                    return true;
            }
            return false;
        }


        private Boolean IsLambda()
        {
            if (TokenStream.Current.TokenType == TokenType.Fun)
            {
                return TokenStream.Peek(1).TokenType == TokenType.OpenParenth &&
                       TokenStream.Peek(2).TokenType == TokenType.CloseParenth &&
                       TokenStream.Peek(3).TokenType == TokenType.DeRef;
            }

            return false;
        }

        private Boolean IsVariableDeclarationWithAssignment()
        {
            switch (TokenStream.Current.TokenType)
            {
                case TokenType.Void:
                case TokenType.Word:
                case TokenType.Int:
                    return TokenStream.Alt(VariableDeclarationAndAssignment);
            }

            return false;
        }

        private Boolean IsVariableDeclaration()
        {
            switch (TokenStream.Current.TokenType)
            {
                case TokenType.Void:
                case TokenType.Word:
                case TokenType.Int:
                    return TokenStream.Alt(() =>
                    {
                        var variableDeclr = GetVariableDeclaration();

                        TokenStream.Take(TokenType.SemiColon);

                        return variableDeclr;
                    });
            }

            return false;
        }

        private Boolean IsVariableAssignment()
        {
            switch (TokenStream.Current.TokenType)
            {
                case TokenType.Word:
                    return
                        TokenStream.Alt(VariableAssignment);
            }

            return false;
        }

        private Boolean IsOperator(Token item)
        {
            switch (item.TokenType)
            {
                case TokenType.Equals:
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Asterix:
                case TokenType.Carat:
                case TokenType.Slash:
                    return true;
            }
            return false;
        }

        #endregion
    }
}
