using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;
using Lang.Utils;

namespace Lang
{
    public class Parser : ParserBase
    {
        public Parser(Tokenizer tokenizer)
            : base(tokenizer)
        {
        }

        public Ast Parse()
        {
            var statements = GetStatements(() => Current == null);

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
                if (Current.TokenType == TokenType.LBracket)
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
            if (IsMethodDeclaration(Current))
            {
                return MethodDeclaration();
            }

            var statement = Statement();

            Take(TokenType.SemiColon);

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

            throw new InvalidSyntax(String.Format("Unknown expression type {0} - {1}", Current.TokenType, Current.TokenValue));
        }

        private Ast ScopeStart()
        {
            if (Current.TokenType == TokenType.LBracket)
            {
                var statements = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

                return new ScopeDeclr(statements);
            }

            return null;
        }

        private Ast OperationExpression()
        {
            switch (Current.TokenType)
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
            var name = Take(TokenType.Word);

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

            if(Alt(()=>op()))
            {
                return op();
            }
            
            if (Alt(() => ConsumeFinalExpression()))
            {
                return ConsumeFinalExpression();
            }

            return null;
        }

        private List<Ast> GetExpressionsInScope(TokenType open, TokenType close, bool expectSemicolon = true)
        {
            Take(open);
            var lines = new List<Ast>();
            while (Current.TokenType != close)
            {
                lines.Add(Statement());

                if (expectSemicolon)
                {
                    Take(TokenType.SemiColon);
                }
            }

            Take(close);

            return lines;
        } 

        private Ast GetLambda()
        {
            Take(TokenType.Fun);
            Take(TokenType.OpenParenth);
            Take(TokenType.CloseParenth);
            Take(TokenType.DeRef);

            var lines = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

            var method = new MethodDeclr(new Token(TokenType.Fun), new Token(TokenType.Void),
                                         new Token(TokenType.Word, "anonymous"), null, lines);


            return method;
        }

        private Ast MethodDeclaration()
        {
            if (!IsValidMethodReturnType(Current))
            {
                throw new InvalidSyntax("Invalid syntax");
            }

            // return type
            var returnType = Take(Current.TokenType);

            // func name
            var funcName = Take(TokenType.Word);

            var argList = GetArgumentList(true);

            var innerExpressions = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

            return new MethodDeclr(new Token(TokenType.ScopeStart), returnType, funcName, argList, innerExpressions);
        }

        private List<Ast> GetArgumentList(bool includeType = false)
        {
            Take(TokenType.OpenParenth);

            var args = new List<Ast>(64);

            while (Current.TokenType != TokenType.CloseParenth)
            {
                var argument = includeType ? GetVariableDeclaration() : Statement();

                args.Add(argument);

                if (Current.TokenType == TokenType.Comma)
                {
                    Take(TokenType.Comma);
                }
            }

            Take(TokenType.CloseParenth);

            return args;
        }

        private Ast VariableDeclarationAndAssignment()
        {
            var type = Take(Current.TokenType);

            var name = Take(TokenType.Word);

            Take(TokenType.Equals);

            return new VarDeclrAst(type, name, Statement());
        }

        private Ast GetVariableDeclaration()
        {
            var type = Take(Current.TokenType);

            var name = Take(TokenType.Word);

            return new VarDeclrAst(type, name);
        }

        private Ast VariableAssignment()
        {
            var name = Take(TokenType.Word);

            var equals = Take(TokenType.Equals);

            return new Expr(new Expr(name), equals, Statement());
        }

        private Ast ConsumeFinalExpression()
        {
            return FunctionCallStatement().Or(VariableAssignmentStatement)
                                          .Or(SingleToken);
        }

        private Ast SingleToken()
        {
            var token = new Expr(Take(Current.TokenType));

            return token;
        }

        private Token TakeOperator()
        {
            if (IsOperator(Current))
            {
                return Take(Current.TokenType);
            }

            throw new InvalidSyntax(String.Format("Invalid token found. Expected operator but found {0} - {1}", Current.TokenType, Current.TokenValue));

        }

        #endregion

        #region Alternative Route Testers


        private Ast VariableDeclWithAssignStatement()
        {
            if (IsVariableDeclarationWithAssignment(Current))
            {
                return VariableDeclarationAndAssignment();
            }

            return null;
        }

        private Ast VariableAssignmentStatement()
        {
            if (IsVariableAssignment(Current))
            {
                return VariableAssignment();
            }

            return null;
        }

        private Ast VariableDeclrStatement()
        {
            if (IsVariableDeclaration(Current))
            {
                var declr = GetVariableDeclaration();

                return declr;
            }

            return null;
        }

        private Ast FunctionCallStatement()
        {
            if (IsFunctionCall())
            {
                return GetFunctionCall();
            }

            return null;
        }

        private Ast LambdaStatement()
        {
            if (IsLambda(Current))
            {
                return GetLambda();
            }

            return null;
        }

        private bool IsFunctionCall()
        {
            if (Current.TokenType == TokenType.Word)
            {
                return Alt(() => GetFunctionCall());
            }

            return false;
        }

        private bool IsMethodDeclaration(Token current)
        {
            if (IsValidMethodReturnType(current))
            {
                return Alt(() => MethodDeclaration());
            }

            return false;
        }

        private bool IsValidMethodReturnType(Token current)
        {
            switch (current.TokenType)
            {
                case TokenType.Void:
                case TokenType.Word:
                case TokenType.Int:
                    return true;
            }
            return false;
        }


        private Boolean IsLambda(Token item)
        {
            if (item.TokenType == TokenType.Fun)
            {
                return Peek(1).TokenType == TokenType.OpenParenth &&
                       Peek(2).TokenType == TokenType.CloseParenth &&
                       Peek(3).TokenType == TokenType.DeRef;
            }

            return false;
        }

        private Boolean IsVariableDeclarationWithAssignment(Token item)
        {
            switch (item.TokenType)
            {
                case TokenType.Void:
                case TokenType.Word:
                case TokenType.Int:
                    return Alt(() => VariableDeclarationAndAssignment());
            }

            return false;
        }

        private Boolean IsVariableDeclaration(Token item)
        {
            switch (item.TokenType)
            {
                case TokenType.Void:
                case TokenType.Word:
                case TokenType.Int:
                    return Alt(() =>
                    {
                        GetVariableDeclaration();

                        Take(TokenType.SemiColon);
                    });
            }

            return false;
        }

        private Boolean IsVariableAssignment(Token item)
        {
            switch (item.TokenType)
            {
                case TokenType.Word:
                    return
                        Alt(() => VariableAssignment());
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
