using System;
using System.Collections.Generic;
using Lang.AST;
using Lang.Data;
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

            if (ast.Right != null)
            {
                ast.Right.Visit(this);
            }
        }

        public void Visit(FuncInvoke ast)
        {
            Current.Define(GetName(ast.FunctionName));

            ast.Arguments.ForEach(arg => arg.Visit(this));
        }

        public void Visit(VarDeclrAst ast)
        {
            if (ast.DeclarationType != null)
            {
                Current.Define(GetNameAndType(ast.DeclarationType, ast.VariableName));
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

        private Symbol GetNameAndType(Ast astType, Ast name)
        {
            BasicType type = null;

            if (astType != null)
            {
                type = new BasicType(astType.Token.TokenValue);
            }

            return new Symbol(name.Token.TokenValue, type);
        }

        public void Visit(MethodDeclr ast)
        {
            Current.Define(GetNameAndType(ast.MethodReturnType, ast.MethodName));

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
                Current = ScopeTree.Dequeue().EnclosingScope;
            }
        }
    }
}
