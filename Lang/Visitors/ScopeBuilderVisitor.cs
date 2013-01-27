using System;
using System.Collections.Generic;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;
using Lang.Symbols;

namespace Lang.Visitors
{
    public class ScopeBuilderVisitor : IAstVisitor
    {
        public Scope Current { get; private set; }

        public Queue<Scope> ScopeTree { get; private set; }

        public ScopeBuilderVisitor()
        {
            ScopeTree = new Queue<Scope>();
        }

        public void Visit(Conditional ast)
        {
            ast.Predicate.Visit(this);

            ast.Body.Visit(this);

            if (ast.Alternate != null)
            {
                ast.Alternate.Visit(this);
            }
        }

        public void Visit(Expr ast)
        {
            if (ast.Left != null)
            {
                ast.Left.Visit(this);
            }

            if (ast.Token.TokenType == TokenType.Word)
            {
                var resolved = Current.Resolve(ast.Token.TokenValue);

                if (resolved != null)
                {
                    Console.WriteLine("Resolving {0} to {1}", ast.Token.TokenValue, resolved.Type.TypeName);
                }
                else
                {
                    var msg = String.Format("Trying to access undefined variable {0}", ast.Token.TokenValue);

                    Console.WriteLine(msg);
                    throw new InvalidSyntax(msg);
                }
            }

            if (ast.Right != null)
            {
                ast.Right.Visit(this);
            }
        }

        public void Visit(FuncInvoke ast)
        {
            var resolved = Current.Resolve(ast.Token.TokenValue);

            if (resolved != null)
            {
                Console.WriteLine("Resolving function {0} to {1}", ast.Token.TokenValue, resolved.Type.TypeName);
            }
            else
            {
                var msg = String.Format("Trying to access undefined function {0}", ast.Token.TokenValue);

                Console.WriteLine(msg);
                throw new InvalidSyntax(msg);
            }

            ast.Arguments.ForEach(arg => arg.Visit(this));
        }

        public void Visit(VarDeclrAst ast)
        {
            if (ast.DeclarationType != null)
            {
                Current.Define(DefineUserSymbol(ast.DeclarationType, ast.VariableName));
            }

            if (ast.VariableValue != null)
            {
                ast.VariableValue.Visit(this);
            }
        }

        private Symbol GetName(Ast ast)
        {
            return new Symbol(ast.Token.TokenValue);
        }

        private Symbol DefineUserSymbol(Ast astType, Ast name)
        {
            IType type = GetSymbolType(astType);

            return new Symbol(name.Token.TokenValue, type);
        }

        private IType GetSymbolType(Ast astType)
        {
            if (astType == null)
            {
                return null;
            }

            switch (astType.Token.TokenType)
            {
                case TokenType.Int:
                case TokenType.Void:
                    return new BuiltInType(astType.Token.TokenType.ToString());
                default:
                    return new UserDefinedType(astType.Token.TokenValue);
            }
        }

        private Symbol DefineMethod(Ast astType, Ast name)
        {
            IType type = GetSymbolType(astType);

            return new MethodSymbol(name.Token.TokenValue, type);
        }

        public void Visit(MethodDeclr ast)
        {
            Current.Define(DefineMethod(ast.MethodReturnType, ast.MethodName));

            ast.Arguments.ForEach(arg => arg.Visit(this));

            ast.BodyStatements.Visit(this);
        }

        public void Visit(WhileLoop ast)
        {
            ast.Predicate.Visit(this);

            ast.Body.Visit(this);
        }

        public void Visit(ScopeDeclr ast)
        {
            CreateScope();

            ast.ScopedStatements.ForEach(statement => statement.Visit(this));

            PopScope();
        }

        public void Visit(ForLoop ast)
        {
            ast.Initial.Visit(this);

            ast.Stop.Visit(this);

            ast.Modify.Visit(this);

            ast.Body.Visit(this);
        }

        public void Visit(ReturnAst ast)
        {
            ast.Visit(this);
        }

        private void CreateScope()
        {
            var parentScope = ScopeTree.Count > 0 ? ScopeTree.Peek() : null;

            Current = new Scope(parentScope);

            if (parentScope != null)
            {
                parentScope.ChildScopes.Add(Current);
            }

            ScopeTree.Enqueue(Current);
        }

        private void PopScope()
        {
            if (ScopeTree.Count > 0)
            {
                Current = ScopeTree.Dequeue();
            }
        }
    }
}
