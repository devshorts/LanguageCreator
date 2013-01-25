using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;
using Lang.Data;

namespace Lang.Visitors
{
    public class AstVisitor : IAstVisitor
    {
        public void Visit(Conditional ast)
        {
            Console.WriteLine(ast.Token);

            if (ast.Predicate != null)
            {
                PrintWrap("Predicate", () => ast.Predicate.Visit(this));
            }

            ast.Body.Visit(this);

            if (ast.Alternate != null)
            {
                if (ast.Alternate.Token.TokenType == TokenType.If)
                {
                    Console.WriteLine("Else");
                }

                ast.Alternate.Visit(this);
            }
        }

        public void Visit(Expr ast)
        {
            if (ast.Left != null)
            {
                ast.Left.Visit(this);
            }

            Console.WriteLine(ast.Token);

            if (ast.Right != null)
            {
                ast.Right.Visit(this);
            }
        }

        public void Visit(FuncInvoke ast)
        {
            ast.FunctionName.Visit(this);

            ast.Arguments.ForEach(arg => arg.Visit(this));
        }

        public void Visit(VarDeclrAst ast)
        {
            if (ast.DeclarationType != null)
            {
                ast.DeclarationType.Visit(this);
            }

            ast.VariableName.Visit(this);

            if (ast.VariableValue != null)
            {
                Console.WriteLine("Equals");

                ast.VariableValue.Visit(this);
            }
        }

        public void Visit(MethodDeclr ast)
        {
            PrintWrap("MethodDeclaration", () =>
                {
                    ast.MethodReturnType.Visit(this);

                    ast.MethodName.Visit(this);

                    PrintWrap("Arguments", () => ast.Arguments.ForEach(arg => arg.Visit(this)));

                    PrintWrap("Body", () => ast.BodyStatements.Visit(this));

                });
        }

        public void Visit(WhileLoop ast)
        {
            Console.WriteLine(ast.Token);

            PrintWrap("Predicate", () => ast.Predicate.Visit(this));

            ast.Body.Visit(this);
        }

        public void Visit(ScopeDeclr ast)
        {
            PrintWrap("Scope", () => ast.ScopedStatements.ForEach(statement => statement.Visit(this)));
        }

        private void PrintWrap(string name, Action action)
        {
            Console.WriteLine(name + "(");

            action();

            Console.WriteLine(")");
        }
    }
}
