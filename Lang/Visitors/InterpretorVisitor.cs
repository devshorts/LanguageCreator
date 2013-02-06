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

        public InterpretorVisitor(Ast ast)
        {
            MemorySpaces = new ScopeStack<MemorySpace>();

            var scopeBuilder = new ScopeBuilderVisitor();
            var resolver = new ScopeBuilderVisitor(true);

            ast.Visit(scopeBuilder);

            ast.Visit(resolver);

            ast.Visit(this);
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
            try
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
                    case AstTypes.Return:
                        ReturnDo(ast as ReturnAst);
                        break;
                    case AstTypes.For:
                        ForDo(ast as ForLoop);
                        break;
                }
            }
            catch (ReturnException ex)
            {
                if (ast.AstType == AstTypes.FunctionInvoke)
                {
                    return ex.Value;
                }

                throw;
            }

            return null;
        }

        private void ForDo(ForLoop forLoop)
        {
            MemorySpaces.CreateScope();

            Exec(forLoop.Setup);

            while (Exec(forLoop.Predicate))
            {
                Exec(forLoop.Update);

                Exec(forLoop.Body);
            }

            MemorySpaces.PopScope();
        }

        private void ReturnDo(ReturnAst returnAst)
        {
            if (returnAst.ReturnExpression != null)
            {
                var value = Exec(returnAst.ReturnExpression);

                throw new ReturnException(value);
            }

            throw new ReturnException(null);
        }

        private void WhileDo(WhileLoop whileLoop)
        {
            MemorySpaces.CreateScope();

            while (Exec(whileLoop.Predicate))
            {
                whileLoop.Body.ScopedStatements.ForEach(statement => Exec(statement));
            }

            MemorySpaces.PopScope();
        }

        private void ConditionalDo(Conditional conditional)
        {
            // else has no predicate
            if (conditional.Predicate == null)
            {
                Exec(conditional.Body);
                return;
            }

            MemorySpaces.CreateScope();

            var success = Convert.ToBoolean(Exec(conditional.Predicate));

            if(success)
            {
                Exec(conditional.Body);

                MemorySpaces.PopScope();
            }
            else
            {
                MemorySpaces.PopScope();

                Exec(conditional.Alternate);
            }

        }

        private object InvokeFunction(FuncInvoke funcInvoke)
        {
            var method = Resolve(funcInvoke);

            if (method != null)
            {
                var invoker = method as MethodSymbol;

                if (invoker == null)
                {
                    invoker = MemorySpaces.Current.Get(method.Name) as MethodSymbol;
                }

                return InvokeMethodSymbol(invoker, funcInvoke.Arguments);
            }

            throw new UndefinedElementException("Undefined method");
        }

        private object InvokeMethodSymbol(MethodSymbol method, List<Ast> args)
        {
            MemorySpaces.CreateScope();

            var count = 0;
            foreach (VarDeclrAst arg in method.MethodDeclr.Arguments)
            {
                arg.Visit(this);


                if (arg.VariableValue == null)
                {
                    MemorySpaces.Current.Define(arg.Token.TokenValue, Exec(args[count]));
                }
                else
                {
                    MemorySpaces.Current.Assign(arg.Token.TokenValue, Exec(args[count]));
                }

                count++;
            }

            var val = Exec(method.MethodDeclr.Body);

            MemorySpaces.PopScope();

            return val;
        }

        private void VariableDeclaration(VarDeclrAst varDeclrAst)
        {
            if (varDeclrAst.VariableValue == null)
            {
                return;
            }

            if (varDeclrAst.VariableValue.AstType != AstTypes.MethodDeclr)
            {
                var value = Exec(varDeclrAst.VariableValue);

                if (value != null)
                {
                    var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                    MemorySpaces.Current.Define(symbol.Name, value);
                }
            }
            else
            {
                var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                var resolvedMethod = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableValue.Token.TokenValue);

                MemorySpaces.Current.Define(symbol.Name, resolvedMethod);
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

                case TokenType.Float:
                    return Convert.ToDouble(ast.Token.TokenValue);

                case TokenType.QuotedString:
                    return ast.Token.TokenValue;

                case TokenType.True:
                    return true;

                case TokenType.False:
                    return false;
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
                case TokenType.Or:
                    return left || right;
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

            if (resolved == null)
            {
                var msg = String.Format("Trying to access undefined function {0}", ast.Token.TokenValue);

                Console.WriteLine(msg);

                throw new UndefinedElementException(msg);
            }

            return resolved;
        }
    }
}
