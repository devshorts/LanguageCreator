using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;
using Lang.Spaces;
using Lang.Symbols;

namespace Lang.Visitors
{
    public class InterpretorVisitor : IAstVisitor
    {
        private ScopeStack<MemorySpace> MemorySpaces { get; set; }

        public InterpretorVisitor()
        {
            MemorySpaces = new ScopeStack<MemorySpace>();
        }


        public void Visit(Conditional ast)
        {
            Exec(ast);
        }

        public void Visit(Expr ast)
        {
            Exec(ast);
        }

        public void Visit(FuncInvoke ast)
        {
            Exec(ast);
        }

        public void Visit(VarDeclrAst ast)
        {
            Exec(ast);
        }

        public void Visit(MethodDeclr ast)
        {
            Exec(ast);
        }

        public void Visit(WhileLoop ast)
        {
            Exec(ast);
        }

        public void Visit(ScopeDeclr ast)
        {
            Exec(ast);
        }

        public void Visit(ForLoop ast)
        {
            Exec(ast);
        }

        public void Visit(ReturnAst ast)
        {
            Exec(ast);
        }

        public void Visit(PrintAst ast)
        {
            Exec(ast);
        }

        private Object Exec(Ast ast)
        {
            switch (ast.Type)
            {
                case AstTypes.ScopeDeclr:
                    ScopeDelcaration(ast as ScopeDeclr);
                    break;
                case AstTypes.VarDeclr:
                    VariableDeclaration(ast as VarDeclrAst);
                    break;

                case AstTypes.Expression:
                    var ret = Expression(ast as Expr);
                    if (ret != null)
                    {
                        return ret;
                    }
                    break;
                case AstTypes.Print:
                    Print(ast as PrintAst);
                    break;
            }

            return null;
        }

        private void VariableDeclaration(VarDeclrAst varDeclrAst)
        {
            var value = Exec(varDeclrAst.VariableValue);

            if (value != null)
            {
                var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                MemorySpaces.Current.Assign(symbol.Name, value);
            }
        }

        private void Print(PrintAst ast)
        {
            var expression = Exec(ast.Expression);

            Console.WriteLine(expression);
        }

        private object Expression(Expr ast)
        {
            var lhs = ast.Left;
            var rhs = ast.Right;

            switch (ast.Token.TokenType)
            {
                case TokenType.Equals:
                    var symbolEquals = Resolve(lhs);

                    MemorySpaces.Current.Assign(lhs.Token.TokenValue, Exec(rhs));
                    return null;

                case TokenType.Plus:
                    return Convert.ToInt32(Exec(lhs)) + Convert.ToInt32(Exec(rhs));

                case TokenType.Word:
                    var symbol = Resolve(ast);

                    return MemorySpaces.Current.Get(ast.Token.TokenValue);
                    
                case TokenType.Number:
                    return Convert.ToInt32(ast.Token.TokenValue);

                case TokenType.QuotedString:
                    return ast.Token.TokenValue;

                default:
                    return null;
            }
        }

               private void ScopeDelcaration(ScopeDeclr ast)
        {
            MemorySpaces.CreateScope();

            ast.ScopedStatements.ForEach(statement => statement.Visit(this));

            MemorySpaces.PopScope();
        }

        private Symbol Resolve(Ast ast)
        {
            var resolved = ast.CurrentScope.Resolve(ast.Token.TokenValue);

            if (resolved != null)
            {
                Console.WriteLine("Resolving {0} to {1}", ast.Token.TokenValue, resolved.Type.TypeName);
            }
            else
            {
                var msg = String.Format("Trying to access undefined function {0}", ast.Token.TokenValue);

                Console.WriteLine(msg);
                throw new UndefinedElementException(msg);
            }

            return resolved;
        }
    }
}
