using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;
using Lang.Spaces;
using Lang.Symbols;
using Lang.Utils;

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

        private dynamic Exec(Ast ast)
        {
            if (ast == null)
            {
                return null;
            }

            switch (ast.AstType)
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
                case AstTypes.FunctionInvoke:
                    return InvokeFunction(ast as FuncInvoke);
                case AstTypes.Conditional:
                    ConditionalDo(ast as Conditional);
                    break;
                case AstTypes.MethodDeclr:
                    return ast;

                case AstTypes.While:
                    WhileDo(ast as WhileLoop);
                    break;
            }

            return null;
        }

        private void WhileDo(WhileLoop whileLoop)
        {
            while (Exec(whileLoop.Predicate))
            {
                whileLoop.Body.ScopedStatements.ForEach(statement => Exec(statement));
            }
        }

        private void ConditionalDo(Conditional conditional)
        {
            // else has no predicate
            if (conditional.Predicate == null)
            {
                Exec(conditional.Body);
                return;
            }

            if (Convert.ToBoolean(Exec(conditional.Predicate)))
            {
                Exec(conditional.Body);
            }
            else
            {
                Exec(conditional.Alternate);
            }
        }

        private object InvokeFunction(FuncInvoke funcInvoke)
        {
            var method = Resolve(funcInvoke) as MethodSymbol;

            return InvokeMethodSymbol(method, funcInvoke.Arguments);
        }

        private object InvokeMethodSymbol(MethodSymbol method, List<Ast> args)
        {
            MemorySpaces.CreateScope();

            var count = 0;
            foreach (var arg in method.MethodDeclr.Arguments)
            {
                arg.Visit(this);

                MemorySpaces.Current.Assign(arg.Token.TokenValue, Exec(args[count]));

                count++;
            }


            var val = Exec(method.MethodDeclr.BodyStatements);

            MemorySpaces.PopScope();

            return val;
        }

        private void VariableDeclaration(VarDeclrAst varDeclrAst)
        {
            if (varDeclrAst.VariableValue.AstType != AstTypes.MethodDeclr)
            {
                var value = Exec(varDeclrAst.VariableValue);

                if (value != null)
                {
                    var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                    MemorySpaces.Current.Assign(symbol.Name, value);
                }
            }
            else
            {
                var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                var resolvedMethod = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableValue.Token.TokenValue);

                MemorySpaces.Current.Assign(symbol.Name, resolvedMethod);
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
                    MemorySpaces.Current.Assign(lhs.Token.TokenValue, Exec(rhs));
                    return null;

                case TokenType.Word:
                    var symbol = Resolve(ast);

                    return MemorySpaces.Current.Get(symbol.Name);
                  
                case TokenType.Int:
                    return Convert.ToInt32(ast.Token.TokenValue);

                case TokenType.Number:
                    return Convert.ToInt32(ast.Token.TokenValue);

                case TokenType.QuotedString:
                    return ast.Token.TokenValue;
            }

            if (TokenUtil.IsOperator(ast.Token))
            {
                return ApplyOperation(ast);
            }

            return null;
        }

        private object ApplyOperation(Expr ast)
        {
            dynamic left = Exec(ast.Left);
            dynamic right = Exec(ast.Right);

            switch (ast.Token.TokenType)
            {
                case TokenType.GreaterThan:
                    return left > right;
                case TokenType.LessThan:
                    return left < right;
                case TokenType.Plus:
                    return left + right;
                case TokenType.Minus:
                    return left - right;
                case TokenType.Slash:
                    return left/right;
                case TokenType.Carat:
                    return left ^ right;
                case TokenType.Ampersand:
                    return left && right;
            }

            return null;
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
                if (resolved.Type is BuiltInType &&
                    (resolved.Type as BuiltInType).ExpressionType == ExpressionTypes.Method)
                {
                    return MemorySpaces.Current.Get(ast.Token.TokenValue) as MethodSymbol;
                }
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
