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

        public MemorySpace Global { get; set; }

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

        public void Start(Ast ast)
        {
            var scopeBuilder = new ScopeBuilderVisitor();

            var resolver = new ScopeBuilderVisitor(true);

            scopeBuilder.Start(ast);

            resolver.Start(ast);

            Global = MemorySpaces.Current;
            
            ast.Visit(this);
        }

        public void Visit(ClassAst ast)
        {
            Exec(ast);
        }

        public void Visit(ClassReference ast)
        {
            Exec(ast);
        }

        public void Visit(NewAst ast)
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

                if (ast.ConvertedExpression != null)
                {
                    return Exec(ast.ConvertedExpression);
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
                        var methodDeclr = ast as MethodDeclr;
                        return new MethodSymbol(methodDeclr.Token.TokenValue, ScopeBuilderVisitor.CreateSymbolType(methodDeclr.ReturnAst), methodDeclr);

                    case AstTypes.While:
                        WhileDo(ast as WhileLoop);
                        break;
                    case AstTypes.Return:
                        ReturnDo(ast as ReturnAst);
                        break;
                    case AstTypes.For:
                        ForDo(ast as ForLoop);
                        break;
                    case AstTypes.Class:
                        ClassDo(ast as ClassAst);
                        break;
                    case AstTypes.ClassRef:
                        return ClassRefDo(ast as ClassReference);
                        break;
                    case AstTypes.New:
                        return NewDo(ast as NewAst);
                        break;
                }
            }
            catch (ReturnException ex)
            {
                // let the return value bubble up through all the exectuions until we
                // get to the source syntax tree that invoked it. at that point safely return the value
                if (ast.AstType == AstTypes.FunctionInvoke)
                {
                    return ex.Value;
                }

                throw;
            }

            return null;
        }

        private object NewDo(NewAst ast)
        {
            var className = Resolve(ast);

            var classType = (className as ClassSymbol).Src as ClassAst;

            var space = new MemorySpace();

            var oldSpace = MemorySpaces.Current;

            MemorySpaces.Current = space;
            foreach (var symbol in classType.Body.ScopedStatements)
            {
                Exec(symbol);
            }

            MemorySpaces.Current = oldSpace;

            return space;
        }

        private dynamic ClassRefDo(ClassReference classReference)
        {
            var oldSpace = MemorySpaces.Current;

            var memorySpace = Get(classReference.ClassInstance) as MemorySpace;

            MemorySpaces.Current = memorySpace;

            try
            {
                if (classReference.Deferences.Count == 0)
                {
                    return memorySpace;
                }

                var count = 0;
                foreach (var deref in classReference.Deferences)
                {
                    if (deref == classReference.Deferences.Last())
                    {
                        deref.CallingMemory = oldSpace;
                    }

                    var newSpace = Exec(deref);

                    if (count == classReference.Deferences.Count - 1)
                    {
                        return newSpace;
                    }

                    MemorySpaces.Current = newSpace;

                    count++;
                }
            }
            finally
            {
                MemorySpaces.Current = oldSpace;
            }

            return null;
        }

        private void ClassDo(ClassAst classAst)
        {
            
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

                if (funcInvoke.CallingMemory != null && !CollectionUtil.IsNullOrEmpty(funcInvoke.Arguments))
                {
                    funcInvoke.Arguments.ForEach(arg => arg.CallingMemory = funcInvoke.CallingMemory);
                }

                return InvokeMethodSymbol(invoker, funcInvoke.Arguments);
            }

            throw new UndefinedElementException("Undefined method");
        }

        private object InvokeMethodSymbol(MethodSymbol method, List<Ast> args)
        {
            MemorySpaces.CreateScope();

            var count = 0;

            if (method.MethodDeclr.Arguments.Count != args.Count)
            {
                throw new InvalidSyntax(String.Format("Wrong number of arguments passed to method {0}. Got {1}, expected {2}", 
                    method.MethodDeclr.MethodName.Token.TokenValue,
                    args.Count, method.MethodDeclr.Arguments.Count));
            }

            foreach (VarDeclrAst expectedArgument in method.MethodDeclr.Arguments)
            {
                var currentArgument = args[count]; 

                if (expectedArgument.VariableValue == null)
                {
                    MemorySpaces.Current.Define(expectedArgument.Token.TokenValue, Exec(currentArgument));
                }
                else
                {
                    MemorySpaces.Current.Assign(expectedArgument.Token.TokenValue, Exec(currentArgument));
                }

                // if the passed in argument is a word and not a literal (like string or bool) then 
                // pull its value from memory so we can match type to the target type 
                var resolvedSymbol = (currentArgument.Token.TokenType == TokenType.Word ? 
                                                MemorySpaces.Current.Get(currentArgument.Token.TokenValue)
                                            :   args[count]) as Symbol;

                var resolvedType = resolvedSymbol != null ? resolvedSymbol.Type : currentArgument.AstSymbolType;

                if (currentArgument is MethodDeclr)
                {
                    resolvedType = new BuiltInType(ExpressionTypes.Method);
                }

                if (!TokenUtil.EqualOrPromotable(expectedArgument.AstSymbolType, resolvedType))
                {
                    throw new InvalidSyntax(String.Format("Cannot pass argument {0} of type {1} to function {2} as argument {3} of type {4}",
                        currentArgument.Token.TokenValue,
                        currentArgument.AstSymbolType.TypeName,
                        method.MethodDeclr.MethodName.Token.TokenValue,
                        expectedArgument.VariableName.Token.TokenValue,
                        expectedArgument.AstSymbolType.TypeName));
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
                var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                var space = MemorySpaces.Current;

                space.Define(symbol.Name, null);

                return;
            }

            var variableValue = varDeclrAst.VariableValue.ConvertedExpression ?? varDeclrAst.VariableValue;

            // if the rhs of a variable is not a method, then execute it, 
            if (variableValue.AstType != AstTypes.MethodDeclr)
            {
                var value = Exec(variableValue);

                if (value != null)
                {
                    var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                    var space = MemorySpaces.Current;
                    
                    space.Define(symbol.Name, value);
                }
            }
            else
            {
                var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                var resolvedMethod = varDeclrAst.CurrentScope.Resolve(variableValue.Token.TokenValue);

                var space = MemorySpaces.Current;
                    
                space.Define(symbol.Name, resolvedMethod);
            }
        }

        private void Print(PrintAst ast)
        {
            var expression = Exec(ast.Expression);

            Console.WriteLine(expression);
        }

        private void Assign(Ast ast, dynamic value, MemorySpace space = null)
        {
            if (space != null)
            {
                space.Assign(ast.Token.TokenValue, value);
            }

            else
            {
                MemorySpaces.Current.Assign(ast.Token.TokenValue, value);
            }
        }

        private dynamic Get(Ast ast)
        {
            object item;
            
            item = MemorySpaces.Current.Get(ast.Token.TokenValue);

            if (ast.CallingMemory != null && item == null)
            {
                item = ast.CallingMemory.Get(ast.Token.TokenValue);
            }

            return item;
        }

        private dynamic Expression(Expr ast)
        {
            var lhs = ast.Left;
            var rhs = ast.Right;

            switch (ast.Token.TokenType)
            {
                case TokenType.Equals:
                    if (lhs.AstType == AstTypes.ClassRef)
                    {
                        // a litle trickery here. create a copy of the class reference
                        // with everytihng up to the second to last item. this gives you
                        // the workign memory space that the very last item should sit in
                        // then lets execute this as if we are asking for the memory space
                        // and finally assign the very last symbol to the calculated memory
                        // space we got

                        var classRef = (lhs as ClassReference);

                        var lastItem = classRef.Deferences.Last();

                        var fakeRef = new ClassReference(classRef.ClassInstance,
                                                         classRef.Deferences.Take(classRef.Deferences.Count - 1)
                                                                 .ToList());

                        var space = Exec(fakeRef);

                        Assign(lastItem, Exec(rhs), space);
                    }

                    else
                    {
                        Assign(lhs, Exec(rhs));
                    }
                    return null;

                case TokenType.Word:
                    return Get(ast);
                  
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
                resolved = ast.Global.Resolve(ast.Token.TokenValue);

                if (resolved == null)
                {
                    var msg = String.Format("Trying to access undefined function {0}", ast.Token.TokenValue);

                    Console.WriteLine(msg);

                    throw new UndefinedElementException(msg);
                }
            }

            return resolved;
        }
    }
}
