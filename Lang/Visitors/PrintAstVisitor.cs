using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;
using Lang.Data;

namespace Lang.Visitors
{
    public class PrintAstVisitor : IAstVisitor
    {
        public void Visit(Conditional ast)
        {
            Console.Write(ast.Token);

            if (ast.Predicate != null)
            {
                PrintWrap("Predicate", () => ast.Predicate.Visit(this));
            }

            ast.Body.Visit(this);

            if (ast.Alternate != null)
            {
                if (ast.Alternate.Token.TokenType == TokenType.If)
                {
                    Console.Write("Else");
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

            Console.Write(" " + ast.Token);

            if (ast.Right != null)
            {
                Console.Write(" ");

                ast.Right.Visit(this);

                Console.WriteLine();
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
                    PrintWrap("Return type", () => ast.MethodReturnType.Visit(this));

                    PrintWrap("FunctionName", () => ast.MethodName.Visit(this));

                    PrintWrap("Arguments", () => ast.Arguments.ForEach(arg => arg.Visit(this)));

                    PrintWrap("Body", () => ast.BodyStatements.Visit(this));
                });
        }

        public void Visit(WhileLoop ast)
        {
            Console.Write(ast.Token);

            PrintWrap("Predicate", () => ast.Predicate.Visit(this));

            ast.Body.Visit(this);
        }

        public void Visit(ScopeDeclr ast)
        {
            PrintWrap("Scope", () => ast.ScopedStatements.ForEach(statement => statement.Visit(this)), true);
        }

        public void Visit(ForLoop ast)
        {
            PrintWrap("ForLoop", () =>
                {
                    PrintWrap("For", () => ast.Initial.Visit(this));
                    PrintWrap("Until", () => ast.Stop.Visit(this));
                    PrintWrap("Modify", () => ast.Modify.Visit(this));
                    
                    ast.Body.Visit(this);
                });
        }

        public void Visit(ReturnAst ast)
        {
            PrintWrap("Return", () => ast.ReturnExpression.Visit(this));
        }

        public void Visit(PrintAst ast)
        {
            PrintWrap("Print", () => ast.Expression.Visit(this));
        }

        private void PrintWrap(string name, Action action, bool newLine = false)
        {
            if (newLine)
            {
                Console.WriteLine(name + " (");
            }
            else
            {
                Console.Write(name + " (");
            }


            action();

            Console.WriteLine(" )");
        }
    }
}
