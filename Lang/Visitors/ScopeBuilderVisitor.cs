using System;
using Lang.AST;
using Lang.Data;
using Lang.Spaces;
using Lang.Symbols;

namespace Lang.Visitors
{
    public class ScopeBuilderVisitor : IAstVisitor
    {
        public Scope Current { get { return ScopeTree.Current; } }

        public ScopeStack<Scope> ScopeTree { get; private set; }

        public ScopeBuilderVisitor()
        {
            ScopeTree = new ScopeStack<Scope>();
        }

        public void Visit(Conditional ast)
        {
            ast.Predicate.Visit(this);

            ast.Body.Visit(this);

            if (ast.Alternate != null)
            {
                ast.Alternate.Visit(this);
            }

            ast.CurrentScope = Current;
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

            ast.CurrentScope = Current;
        }

        public void Visit(FuncInvoke ast)
        {
            ast.Arguments.ForEach(arg => arg.Visit(this));

            ast.CurrentScope = Current;
        }

        public void Visit(VarDeclrAst ast)
        {
            if (ast.DeclarationType != null)
            {
                Console.WriteLine("Defining {0}", ast.VariableName.Token.TokenValue);
                Current.Define(DefineUserSymbol(ast.DeclarationType, ast.VariableName));
            }

            if (ast.VariableValue != null)
            {
                ast.VariableValue.Visit(this);
            }

            ast.CurrentScope = Current;
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

            ScopeTree.CreateScope();

            ast.Arguments.ForEach(arg => arg.Visit(this));

            ast.BodyStatements.Visit(this);

            ast.CurrentScope = Current;

            ScopeTree.PopScope();
        }

        public void Visit(WhileLoop ast)
        {
            ast.Predicate.Visit(this);

            ast.Body.Visit(this);

            ast.CurrentScope = Current;
        }

        public void Visit(ScopeDeclr ast)
        {
            ScopeTree.CreateScope();

            ast.ScopedStatements.ForEach(statement => 
                statement.Visit(this));

            ast.CurrentScope = Current;

            ScopeTree.PopScope();
        }

        public void Visit(ForLoop ast)
        {
            ast.Initial.Visit(this);

            ast.Stop.Visit(this);

            ast.Modify.Visit(this);

            ast.Body.Visit(this);

            ast.CurrentScope = Current;
        }

        public void Visit(ReturnAst ast)
        {
            ast.ReturnExpression.Visit(this);
        }

        public void Visit(PrintAst ast)
        {
            ast.Expression.Visit(this);
        }
    }
}
