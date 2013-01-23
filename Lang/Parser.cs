using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;

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
            var root = new Expr(new Token(TokenType.ScopeStart));

            GetStatements(root, () => Current == null);

            return root;
        }

        private void GetStatements(Ast parent, Func<Boolean> end)
        {
            while (!end())
            {
                if (Current.TokenType == TokenType.LBracket)
                {
                    Take(TokenType.LBracket);

                    var scopeStart = new Expr(new Token(TokenType.ScopeStart));

                    GetStatements(scopeStart, () => Current.TokenType == TokenType.RBracket);

                    Take(TokenType.RBracket);

                    parent.AddChild(scopeStart);
                }
                else
                {
                    parent.AddChild(OuterExpression());
                }
            }

        }

        private Ast OuterExpression()
        {
            if (IsMethodDeclaration(Current))
            {
                return MethodDeclaration();
            }

            return InnerExpression();
        }

        private Ast InnerExpression()
        {
            if (Current.TokenType == TokenType.SemiColon)
            {
                Take(TokenType.SemiColon);

                return null;
            }

            if (IsFunctionCall())
            {
                return FunctionCall();
            }

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

            if (IsLambda(Current))
            {
                return Lambda();
            }

            switch (Current.TokenType)
            {
                case TokenType.QuotedString:
                    return ParseStringBranches();

                case TokenType.Word: 
                    return ParseWordBranches();

                case TokenType.OpenParenth: 

                    Take(TokenType.OpenParenth);

                    var expression = InnerExpression();

                    Take(TokenType.CloseParenth);

                    return expression;

                default:
                    throw new InvalidSyntax(String.Format("Unknown expression type {0} - {1}", Current.TokenType, Current.TokenValue));
            }
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

        private Ast ParseStringBranches()
        {
            if (EndOfStatement(Peek(1)))
            {
                return ConsumeFinalExpression();
            }

            var expr = new Expr(new Expr(Take(TokenType.QuotedString)), TakeOperator(), InnerExpression());

            return expr;
        }

        private Ast Lambda()
        {
            Take(TokenType.Fun);
            Take(TokenType.OpenParenth);
            Take(TokenType.CloseParenth);
            Take(TokenType.DeRef);
            Take(TokenType.LBracket);

            var lines = new List<Ast>();
            while (Current.TokenType != TokenType.RBracket)
            {
                lines.Add(InnerExpression());
            }

            var method = new MethodDeclr(new Token(TokenType.Fun), new Token(TokenType.Void),
                                         new Token(TokenType.Word, "anonymous"), null, lines);

            Take(TokenType.RBracket);

            return method;
        }

        private bool IsMethodDeclaration(Token current)
        {
            if (IsValidItemType(current))
            {
                return Alt(() => MethodDeclaration());
            }

            return false;
        }

        private bool IsValidItemType(Token current)
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
            if (!IsValidItemType(Current))
            {
                throw new InvalidSyntax("Invalid syntax");
            }

            // return type
            var returnType = Take(Current.TokenType);

            // func name
            var funcName = Take(TokenType.Word);

            var argList = ArgumentList(true);

            Take(TokenType.LBracket);

            var innerExpressions = new List<Ast>();

            while (Current != null && Current.TokenType != TokenType.RBracket)
            {
                innerExpressions.Add(InnerExpression());
            }

            Take(TokenType.RBracket);

            return new MethodDeclr(new Token(TokenType.ScopeStart), returnType, funcName, argList, innerExpressions);
        }

        private List<Ast> ArgumentList(bool includeType = false)
        {
            Take(TokenType.OpenParenth);

            var args = new List<Ast>(64);

            while (Current.TokenType != TokenType.CloseParenth)
            {
                var argument = includeType ? VariableDeclaration() : InnerExpression();

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

            return new VarDeclrAst(type, name, InnerExpression());
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

            return new Expr(new Expr(name), equals, InnerExpression());
        }

        private Ast ParseWordBranches()
        {
            if (EndOfStatement(Peek(1)))
            {
                return ConsumeFinalExpression();
            }

            var expr = new Expr(new Expr(Take(TokenType.Word)), TakeOperator(), InnerExpression());

            return expr;
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
                    return  Alt(() =>
                        {
                            Take(Current.TokenType);
                            Take(TokenType.Word);
                            Take(TokenType.Equals);
                            InnerExpression();
                       });
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
                        Alt(() =>
                            {
                                Take(Current.TokenType);
                                Take(TokenType.Equals);
                                InnerExpression();
                            });
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
