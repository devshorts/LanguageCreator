using System;
using System.Collections.Generic;
using System.Linq;
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

        public Stack<MemorySpace> Environment { get; set; }

        public MemorySpace Global { get; set; }

        public InterpretorVisitor()
        {
            Environment = new Stack<MemorySpace>();
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

            if (classType.Constructor != null)
            {
                var funcInvoke = new FuncInvoke(classType.Constructor.Token, ast.Args);

                funcInvoke.CurrentScope = classType.Body.CurrentScope;

                Exec(funcInvoke);
            }

            MemorySpaces.Current = oldSpace;

            return space;
        }

        private dynamic ClassRefDo(ClassReference classReference)
        {
            var oldSpace = MemorySpaces.Current;

            var memorySpace = Get(classReference.ClassInstance).Value as MemorySpace;

            MemorySpaces.Current = memorySpace;

            try
            {
                if (classReference.Deferences.Count == 0)
                {
                    return memorySpace;
                }

                foreach (var deref in classReference.Deferences)
                {
                    // make sure that the last dereference knows how to pull
                    // its arguments, which are from the original memory space and not
                    // the relative class space. i.e. A.b.foo(x), x is loaded from the 
                    // space that contains A, not from the space of b.

                    if (deref == classReference.Deferences.Last())
                    {
                        deref.CallingMemory = oldSpace;
                    }

                    var newSpace = GetValue(Exec(deref));

                    if (deref == classReference.Deferences.Last())
                    {
                        return newSpace;
                    }

                    MemorySpaces.Current = newSpace;
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
            // nothing to do here
        }

        private void ForDo(ForLoop forLoop)
        {
            MemorySpaces.CreateScope();

            Exec(forLoop.Setup);

            while (GetValue(Exec(forLoop.Predicate)))
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

            while (GetValue(Exec(whileLoop.Predicate)))
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

                // arguments should always be resolved from the current calling space
                // so make sure the invoking function knows which space it comes from
                if (funcInvoke.CallingMemory == null)
                {
                    funcInvoke.CallingMemory = MemorySpaces.Current;
                }

                if (funcInvoke.CallingMemory != null && !CollectionUtil.IsNullOrEmpty(funcInvoke.Arguments))
                {
                    funcInvoke.Arguments.ForEach(arg => arg.CallingMemory = funcInvoke.CallingMemory);
                }

                var oldMemory = MemorySpaces.Current;

                try
                {
                    // if we're a lambda and we have some sort of closure
                    // set our working space to be that. 
                    if (invoker.Environment != null)
                    {
                        MemorySpaces.Current = invoker.Environment;
                    }

                    var value = InvokeMethodSymbol(invoker, funcInvoke.Arguments);

                    return value;
                }
                finally
                {
                    MemorySpaces.Current = oldMemory;
                }
            }

            throw new UndefinedElementException("Undefined method");
        }

        private dynamic GetValue(dynamic value)
        {
            return value is ValueMemory ? (value as ValueMemory).Value : value;
        }

        private object InvokeMethodSymbol(MethodSymbol method, List<Ast> args)
        {
            // create a new memory scope. this is where arguments will get defined
            // we wont overwrite any memory values since they will all be local here
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

                var oldmemory = MemorySpaces.Current;

                // if the argument is coming from somewhere else, make sure to be able to 
                // load the argument from its preferred space. 
                if (currentArgument.CallingMemory != null)
                {
                    MemorySpaces.Current = currentArgument.CallingMemory;
                }

                var value = GetValue(Exec(currentArgument));

                // since we were just loading values from the argument space
                // switch back to the current space so we can assign the argument value
                // into our local working memory
                MemorySpaces.Current = oldmemory;

                if (expectedArgument.VariableValue == null)
                {
                    MemorySpaces.Current.Define(expectedArgument.Token.TokenValue, value);
                }
                else
                {
                    MemorySpaces.Current.Assign(expectedArgument.Token.TokenValue, value);
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

                space.Define(symbol.Name, TokenType.Nil);

                return;
            }

            var variableValue = varDeclrAst.VariableValue.ConvertedExpression ?? varDeclrAst.VariableValue;

            // if the rhs of a variable is not a method, then execute it, 
            if (variableValue.AstType != AstTypes.MethodDeclr)
            {
                var value = GetValue(Exec(variableValue));

                if (value != null)
                {
                    var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                    var space = MemorySpaces.Current;

                    if (variableValue.IsLink)
                    {
                        space.Link(symbol.Name, variableValue.Token.TokenValue);
                    }
                    else
                    {
                        space.Define(symbol.Name, value);
                    }
                }
            }
            else
            {
                var symbol = varDeclrAst.CurrentScope.Resolve(varDeclrAst.VariableName.Token.TokenValue);

                var resolvedMethod = varDeclrAst.CurrentScope.Resolve(variableValue.Token.TokenValue) as MethodSymbol;

                // make sure to create a NEW method symbol. this way each time we declare this item
                // it will create a local copy and get its own memory space for closures.  
                // if we shared the same method symbol then all instances of the same declaration would share the memory space, 
                // which may not be what we want given class instances having their own spaces

                var localMethodCopy = new MethodSymbol(resolvedMethod.Name, resolvedMethod.Type,
                                                       resolvedMethod.MethodDeclr);

                var space = MemorySpaces.Current;

                if (variableValue is LambdaDeclr)
                {
                    localMethodCopy.Environment = space;
                }

                space.Define(symbol.Name, localMethodCopy);
            }
        }

        private void Print(PrintAst ast)
        {
            var expression = GetValue(Exec(ast.Expression));

            Console.WriteLine(expression);
        }

        private void Assign(Ast ast, dynamic value, MemorySpace space = null)
        {
            if (value is ValueMemory)
            {
                var tup = value as ValueMemory;

                tup.Memory.Assign(ast.Token.TokenValue, tup.Value);

                return;
            }

            if (space != null)
            {
                space.Assign(ast.Token.TokenValue, value);
                return;
            }

            MemorySpaces.Current.Assign(ast.Token.TokenValue, value);
        }

        private ValueMemory Get(Ast ast)
        {
            object item;

            if (Environment.Count > 0)
            {
                foreach (var env in Environment)
                {
                    item = env.Get(ast.Token.TokenValue, true);
                    if (item != null)
                    {
                        //return item;
                        return new ValueMemory(item, env);
                    }
                }
            }

            item = MemorySpaces.Current.Get(ast.Token.TokenValue);

            if (item != null)
            {
                return new ValueMemory(item, MemorySpaces.Current);
            }

            if (ast.CallingMemory != null)
            {
                return new ValueMemory(ast.CallingMemory.Get(ast.Token.TokenValue), ast.CallingMemory);
            }

            


            return null;
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

                        var space = GetValue(Exec(fakeRef));

                        Assign(lastItem, Exec(rhs), space);
                    }

                    else
                    {
                        ValueMemory itemSpace = Get(lhs);

                        Assign(lhs, Exec(rhs), itemSpace != null ? itemSpace.Memory : null);
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

                case TokenType.Nil:
                    return TokenType.Nil;

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
            dynamic leftExec = Exec(ast.Left);
            dynamic rightExec = Exec(ast.Right);

            var left = leftExec is ValueMemory ? (leftExec as ValueMemory).Value : leftExec;
            var right = rightExec is ValueMemory ? (rightExec as ValueMemory).Value : rightExec;

            switch (ast.Token.TokenType)
            {
                case TokenType.Compare:
                    if (left is TokenType || right is TokenType)
                    {
                        return NullTester.NullEqual(left, right);
                    }

                    return left == right;
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
