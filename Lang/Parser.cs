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
                    Take(TokenType.LBracket);

                    var statements = GetStatements(() => Current.TokenType == TokenType.RBracket);

                    Take(TokenType.RBracket);

                    aggregate.Add(new ScopeDeclr(statements));
                }

                aggregate.Add(MainStatement());
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

            return Statement();
        }

        /// <summary>
        /// A statement inside of a valid scope 
        /// </summary>
        /// <returns></returns>
        private Ast Statement()
        {
            if (Current.TokenType == TokenType.SemiColon)
            {
                Take(TokenType.SemiColon);

                return null;
            }

            var ast = ScopeStart().Or(FunctionCallOrLambdaAst)
                                  .Or(VariableAssignAndDeclrAst)
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
                    return ParseBranches(Current.TokenType);

                case TokenType.OpenParenth:
                    return GetExpressionsInScope(TokenType.OpenParenth, TokenType.CloseParenth).FirstOrDefault();

                default:
                    return null;
            }
        }

        private Ast VariableAssignAndDeclrAst()
        {
            if (IsVariableDeclarationWithAssignment(Current))
            {
                return VariableDeclarationAndAssignment();
            }

            if (IsVariableAssignment(Current))
            {
                return VariableAssignment();
            }

            if (IsVariableDeclaration(Current))
            {
                var declr = VariableDeclaration();

                Take(TokenType.SemiColon);

                return declr;
            }

            return null;
        }

        private Ast FunctionCallOrLambdaAst()
        {
            if (IsFunctionCall())
            {
                return FunctionCall();
            }

            if (IsLambda(Current))
            {
                return Lambda();
            }

            return null;
        }

        private bool IsFunctionCall()
        {
            if (Current.TokenType == TokenType.Word)
            {
                return Alt(() => FunctionCall());
            }

            return false;
        }

        private Ast FunctionCall()
        {
            var name = Take(TokenType.Word);

            var args = ArgumentList();

            Take(TokenType.SemiColon);

            return new FuncInvoke(name, args);
        }

        private Ast ParseBranches(TokenType tokenType)
        {
            if (EndOfStatement(Peek(1)))
            {
                return ConsumeFinalExpression();
            }

            var expr = new Expr(new Expr(Take(tokenType)), TakeOperator(), Statement());

            return expr;
        }

        private List<Ast> GetExpressionsInScope(TokenType open, TokenType close)
        {
            Take(open);
            var lines = new List<Ast>();
            while (Current.TokenType != close)
            {
                lines.Add(Statement());
            }

            Take(close);

            return lines;
        } 

        private Ast Lambda()
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

            var argList = ArgumentList(true);

            var innerExpressions = GetExpressionsInScope(TokenType.LBracket, TokenType.RBracket);

            return new MethodDeclr(new Token(TokenType.ScopeStart), returnType, funcName, argList, innerExpressions);
        }

        private List<Ast> ArgumentList(bool includeType = false)
        {
            Take(TokenType.OpenParenth);

            var args = new List<Ast>(64);

            while (Current.TokenType != TokenType.CloseParenth)
            {
                var argument = includeType ? VariableDeclaration() : Statement();

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

        private Ast VariableDeclaration()
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
            var token = new Expr(Take(Current.TokenType));

            if (Current.TokenType == TokenType.SemiColon)
            {
                Take(TokenType.SemiColon);
            }

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

        private Boolean EndOfStatement(Token item)
        {
            return item.TokenType == TokenType.SemiColon || 
                   item.TokenType == TokenType.RBracket ||
                   item.TokenType == TokenType.Comma || 
                   item.TokenType == TokenType.CloseParenth;
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
                    return  Alt(() => VariableDeclarationAndAssignment());
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
                    return  Alt(() =>
                        {
                            VariableDeclaration();

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
    }
}
