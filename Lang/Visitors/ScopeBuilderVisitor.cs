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
    public class ScopeBuilderVisitor : IAstVisitor
    {
        private void SetScopeType(ScopeType scopeType)
        {
            ScopeContainer.CurrentScopeType = scopeType;
        }

        private Scope _current = null;
        public Scope Current
        {
            get { return ScopeTree.Current; }
            set { ScopeContainer.CurrentScopeStack.Current = value; }
        }

        public ScopeStack<Scope> ScopeTree { get { return ScopeContainer.CurrentScopeStack; } }

        private MethodDeclr CurrentMethod { get; set; }

        private Boolean ResolvingTypes { get; set; }

        private ScopeContainer ScopeContainer;

        public ScopeBuilderVisitor(bool resolvingTypes = false)
        {
            ResolvingTypes = resolvingTypes;
            ScopeContainer = new ScopeContainer();
        }

        public void Visit(Conditional ast)
        {
            if (ast.Predicate != null)
            {
                ast.Predicate.Visit(this);
            }

            ast.Body.Visit(this);

            if (ast.Alternate != null)
            {
                ast.Alternate.Visit(this);
            }

            SetScope(ast);
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

            SetScope(ast);

            if (ast.Left == null && ast.Right == null)
            {
                ast.AstSymbolType = ResolveOrDefine(ast);
            }
            else
            {
                if (ResolvingTypes)
                {
                    ast.AstSymbolType = GetExpressionType(ast.Left, ast.Right, ast.Token);
                }
            }
        }

        /// <summary>
        /// Creates a type for built in types or resolves user defined types
        /// </summary>
        /// <param name="ast"></param>
        /// <returns></returns>
        private IType ResolveOrDefine(Expr ast)
        {
            if (ast == null)
            {
                return null;
            }

            switch (ast.Token.TokenType)
            {
                case TokenType.Word: return ResolveType(ast);
            }

            return CreateSymbolType(ast);
        }

        /// <summary>
        /// Determines user type
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private IType GetExpressionType(Ast left, Ast right, Token token)
        {
            switch (token.TokenType)
            {
                case TokenType.Ampersand:
                case TokenType.Or:
                case TokenType.GreaterThan:
                case TokenType.LessThan:
                    return new BuiltInType(ExpressionTypes.Boolean);
                
                case TokenType.Method:
                case TokenType.Infer:
                    if (right is MethodDeclr)
                    {
                        return new BuiltInType(ExpressionTypes.Method, right);
                    }

                    return right.AstSymbolType;
            }

            if (!ResolvingTypes && (left.AstSymbolType == null || right.AstSymbolType == null))
            {
                return null;
            }

            if (left.AstSymbolType.ExpressionType != right.AstSymbolType.ExpressionType)
            {
                throw new Exception("Mismatched types");
            }

            return left.AstSymbolType;
        }

        public void Visit(FuncInvoke ast)
        {
            ast.Arguments.ForEach(arg => arg.Visit(this));

            SetScope(ast);

            var functionType = Resolve(ast.FunctionName) as MethodSymbol;

            if (functionType != null && ast.Arguments.Count < functionType.MethodDeclr.Arguments.Count)
            {
                var curriedMethod = CreateCurriedMethod(ast, functionType);

                curriedMethod.Visit(this);

                var methodSymbol = DefineMethod(curriedMethod);

                Current.Define(methodSymbol);

                ast.ConvertedExpression = curriedMethod;
            }
            else
            {
                ast.AstSymbolType = ResolveType(ast.FunctionName, ast.CurrentScope);
            }
        }

        private LambdaDeclr CreateCurriedMethod(FuncInvoke ast, MethodSymbol functionType)
        {
            var srcMethod = functionType.MethodDeclr;

            var fixedAssignments = new List<VarDeclrAst>();

            var count = 0;
            foreach (var argValue in ast.Arguments)
            {
                var srcArg = srcMethod.Arguments[count] as VarDeclrAst;

                var token = new Token(srcArg.DeclarationType.Token.TokenType, argValue.Token.TokenValue);

                var declr = new VarDeclrAst(token, srcArg.Token, new Expr(argValue.Token));

                // if we're creating a curry using a variable then we need to resolve the variable type
                // otherwise we can make a symbol for the literal
                var newArgType = argValue.Token.TokenType == TokenType.Word ? 
                                        ast.CurrentScope.Resolve(argValue).Type
                                    :   ScopeUtil.CreateSymbolType(argValue);

                // create a symbol type for the target we're invoking on so we can do type checking
                var targetArgType = ScopeUtil.CreateSymbolType(srcArg.DeclarationType);

                if (!TokenUtil.EqualOrPromotable(newArgType, targetArgType))
                {
                    throw new InvalidSyntax(String.Format("Cannot pass argument {0} of type {1} to partial function {2} as argument {3} of type {4}",
                        argValue.Token.TokenValue, 
                        newArgType.TypeName, 
                        srcMethod.MethodName.Token.TokenValue, 
                        srcArg.VariableName.Token.TokenValue,
                        targetArgType.TypeName)); 
                }

                fixedAssignments.Add(declr);

                count++;
            }

            var newBody = fixedAssignments.Concat(srcMethod.Body.ScopedStatements).ToList();

            var curriedMethod = new LambdaDeclr(srcMethod.Arguments.Skip(ast.Arguments.Count).ToList(), new ScopeDeclr(newBody));

            SetScope(curriedMethod);

            return curriedMethod;
        }

        /// <summary>
        /// Resolve the target ast type from the current scope, OR give it a scope to use.  
        /// Since things can be resolved in two passes (initial scope and forward reference scope)
        /// we want to be able to pass in a scope override.  The second value is usually only ever used
        /// on the second pass when determining forward references
        /// </summary>
        /// <param name="ast"></param>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private IType ResolveType(Ast ast, Scope currentScope = null)
        {
            try
            {
                return Current.Resolve(ast).Type;
            }
            catch (Exception ex)
            {
                if (currentScope != null || ast.CurrentScope != null)
                {
                    if (currentScope == null && ast.CurrentScope != null)
                    {
                        currentScope = ast.CurrentScope;
                    }

                    if (currentScope == null)
                    {
                        if (ResolvingTypes)
                        {
                            throw;
                        }
                        return null;
                    }

                    try
                    {
                        var resolvedType = currentScope.Resolve(ast);

                        if (currentScope.AllowAllForwardReferences || 
                            resolvedType is ClassSymbol ||
                            resolvedType is MethodSymbol)
                        {
                            return resolvedType.Type;
                        }
                        throw new UndefinedElementException(String.Format("Undefined element {0}",
                                                                          ast.Token.TokenValue));
                    }
                    catch (Exception ex1)
                    {
                        if (ResolvingTypes)
                        {
                            throw new UndefinedElementException(String.Format("Undefined element {0}",
                                                                              ast.Token.TokenValue));
                        }

                        return null;
                    }
                }

                throw;
            }
        }


        private Symbol Resolve(String name)
        {
            try
            {
                return Current.Resolve(name);
            }
            catch (Exception ex)
            {
                if (ResolvingTypes)
                {
                    //
                    return null;
                }

                throw;
            }
        }

        private Symbol Resolve(Ast ast)
        {
            try
            {
                return Current.Resolve(ast);
            }
            catch (Exception ex)
            {
                if (ResolvingTypes)
                {
                    //
                    return null;
                }

                throw;
            }
        }

        public void Visit(VarDeclrAst ast)
        {
            var isVar = ast.DeclarationType.Token.TokenType == TokenType.Infer;

            if (ast.DeclarationType != null && !isVar)
            {
                var symbol = DefineUserSymbol(ast.DeclarationType, ast.VariableName);

                Current.Define(symbol);

                ast.AstSymbolType = symbol.Type;
            }

            if (ast.VariableValue != null)
            {
                ast.VariableValue.Visit(this);

                // if its type inferred, determine the declaration by the value's type
                if (isVar)
                {
                    // if the right hand side is a method declaration, make sure to track the source value
                    // this way we can reference it later to determine not only that this is a method type, but what
                    // is the expected return value for static type checking later

                    var val = ast.VariableValue.ConvertedExpression ?? ast.VariableValue;

                    ast.AstSymbolType = val is MethodDeclr
                                            ? new BuiltInType(ExpressionTypes.Method, val)
                                            : val.AstSymbolType;

                    var symbol = DefineUserSymbol(ast.AstSymbolType, ast.VariableName);

                    Current.Define(symbol);
                }
                else if (ResolvingTypes)
                {
                    var declaredType = CreateSymbolType(ast.DeclarationType);

                    var value = ast.VariableValue.ConvertedExpression ?? ast.VariableValue;

                    ReturnAst returnType = null;

                    // when we're resolving types check if the rhs is a function invoke. if it is, see 
                    // what the return value of the src expression is so we can make sure that the 
                    // lhs and the rhs match.
                    try
                    {
                        returnType =
                            value is FuncInvoke
                                ? ((value as FuncInvoke).AstSymbolType) != null
                                      ? ((value as FuncInvoke).AstSymbolType.Src as MethodDeclr).ReturnAst
                                      : null
                                : null;

                    }
                    catch
                    {
                    }

                    value = returnType != null ? returnType.ReturnExpression : value;

                    if (!TokenUtil.EqualOrPromotable(value.AstSymbolType.ExpressionType, declaredType.ExpressionType))
                    {
                        throw new InvalidSyntax(String.Format("Cannot assign {0} of type {1} to {2}", ast.VariableValue, 
                            value.AstSymbolType.ExpressionType, 
                            declaredType.ExpressionType));
                    } 

                }
            }

            SetScope(ast);
        }
        
        private Symbol DefineUserSymbol(Ast astType, Ast name)
        {
            IType type = CreateSymbolType(astType);

            return new Symbol(name.Token.TokenValue, type);
        }

        private Symbol DefineUserSymbol(IType type, Ast name)
        {
            return new Symbol(name.Token.TokenValue, type);
        }


        private IType CreateSymbolType(Ast astType)
        {
            if (astType == null)
            {
                return null;
            }

            switch (astType.Token.TokenType)
            {
                case TokenType.Int:
                    return new BuiltInType(ExpressionTypes.Int) { Src = astType };
                case TokenType.Float:
                    return new BuiltInType(ExpressionTypes.Float) { Src = astType };
                case TokenType.Void:
                    return new BuiltInType(ExpressionTypes.Void) { Src = astType };
                case TokenType.Infer:
                    return new BuiltInType(ExpressionTypes.Inferred) { Src = astType };
                case TokenType.QuotedString:
                case TokenType.String:
                    return new BuiltInType(ExpressionTypes.String) { Src = astType };
                case TokenType.Word:
                    return new UserDefinedType(astType.Token.TokenValue) { Src = astType };
                case TokenType.Class:
                    var c = new ClassSymbol(astType.Token.TokenValue) {Src = astType};
                    return c;
                case TokenType.True:
                case TokenType.False:
                    return new BuiltInType(ExpressionTypes.Boolean) { Src = astType };
                case TokenType.Method:
                    return new BuiltInType(ExpressionTypes.Method) { Src = astType };
            }

            return null;
        }

        private Symbol DefineMethod(MethodDeclr method)
        {
            IType returnType = CreateSymbolType(method.MethodReturnType);

            return new MethodSymbol(method.Token.TokenValue, returnType, method);
        }

        public void Visit(MethodDeclr ast)
        {
            CurrentMethod = ast;

            var symbol = DefineMethod(ast);

            Current.Define(symbol);

            ScopeTree.CreateScope();

            ast.Arguments.ForEach(arg => arg.Visit(this));

            ast.Body.Visit(this);

            SetScope(ast);

            if (symbol.Type.ExpressionType == ExpressionTypes.Inferred)
            {
                if (ast.ReturnAst == null)
                {
                    ast.AstSymbolType = new BuiltInType(ExpressionTypes.Void);
                }
                else
                {
                    ast.AstSymbolType = ast.ReturnAst.AstSymbolType;
                }
            }
            else
            {
                ast.AstSymbolType = symbol.Type;
            }

            ValidateReturnStatementType(ast, symbol);


            ScopeTree.PopScope();
        }

        private void ValidateReturnStatementType(MethodDeclr ast, Symbol symbol)
        {
            if (!ResolvingTypes)
            {
                return;
            }

            IType returnStatementType;

            // no return found
            if (ast.ReturnAst == null)
            {
                returnStatementType = new BuiltInType(ExpressionTypes.Void);
            }
            else
            {
                returnStatementType = ast.ReturnAst.AstSymbolType;
            }

            var delcaredSymbol = CreateSymbolType(ast.MethodReturnType);

            // if its inferred, just use whatever the return statement i
            if (delcaredSymbol.ExpressionType == ExpressionTypes.Inferred)
            {
                return;
            }

            if (returnStatementType.ExpressionType != delcaredSymbol.ExpressionType)
            {
                throw new InvalidSyntax(String.Format("Return type {0} for function {1} is not of the same type of declared method (type {2})",
                    returnStatementType.ExpressionType, symbol.Name, delcaredSymbol.ExpressionType));
            }
        }

        public void Visit(WhileLoop ast)
        {
            ast.Predicate.Visit(this);

            ast.Body.Visit(this);

            SetScope(ast);
        }

        public void Visit(ScopeDeclr ast)
        {
            ScopeTree.CreateScope();

            ast.ScopedStatements.ForEach(statement => statement.Visit(this));

            SetScope(ast);

            ScopeTree.PopScope();
        }

        private void SetScope(Ast ast)
        {
            if (ast.CurrentScope == null)
            {
                ast.CurrentScope = Current;
            }
        }

        public void Visit(ForLoop ast)
        {
            ast.Setup.Visit(this);

            ast.Predicate.Visit(this);

            if (ResolvingTypes && ast.Predicate.AstSymbolType.ExpressionType != ExpressionTypes.Boolean)
            {
                throw new InvalidSyntax("For loop predicate has to evaluate to a boolean");
            }

            ast.Update.Visit(this);

            ast.Body.Visit(this);

            SetScope(ast);
        }

        public void Visit(ReturnAst ast)
        {
            if (ast.ReturnExpression != null)
            {
                ast.ReturnExpression.Visit(this);

                ast.AstSymbolType = ast.ReturnExpression.AstSymbolType;

                CurrentMethod.ReturnAst = ast;
            }
        }

        public void Visit(PrintAst ast)
        {
            ast.Expression.Visit(this);

            if (ResolvingTypes)
            {
                if (ast.Expression.AstSymbolType == null)
                {
                    throw new InvalidSyntax("Undefined expression in print statement");
                }

                if (ast.Expression.AstSymbolType.ExpressionType == ExpressionTypes.Void)
                {
                    throw new InvalidSyntax("Cannot print a void expression");
                }
            }
        }

        public void Start(Ast ast)
        {
            LambdaDeclr.LambdaCount = 0;

            ast.Visit(this);
        }

        public void Visit(ClassAst ast)
        {
            var classSymbol = DefineClassSymbol(ast);

            Current.Define(classSymbol);

            SetScopeType(ScopeType.Class);

            SetScopeSource(classSymbol);

            ScopeTree.CreateScope();

            ast.Body.Visit(this);

            classSymbol.Symbols = ast.Body.CurrentScope.Symbols;

            ScopeTree.PopScope();

            SetScopeType(ScopeType.Global);
        }

        private void SetScopeSource(Symbol classSymbol)
        {
            Current = classSymbol;
        }

        private Symbol DefineClassSymbol(ClassAst ast)
        {
            return new ClassSymbol(ast.Token.TokenValue) { Src = ast, ScopeName = ast.Token.TokenValue };
        }

        public void Visit(ClassReference ast)
        {
            if (!ResolvingTypes)
            {
                return;
            }

            var declaredSymbol = Resolve(ast.ClassInstance);

            var classScope = Resolve(declaredSymbol.Type.TypeName) as ClassSymbol;

            var oldScope = Current;
            
            Current = classScope;

            foreach (var reference in ast.Deferences)
            {
                reference.Visit(this);

                var field = Resolve(reference);

                if (field == null)
                {
                    throw new InvalidSyntax(String.Format("Class {0} has no field named {1}", declaredSymbol.Type.TypeName, reference.Token.TokenValue));
                }
            }

            Current = oldScope;

            ast.AstSymbolType = ast.Deferences.Last().AstSymbolType;

            SetScope(ast);

            SetScope(ast.ClassInstance);
        }

        public void Visit(NewAst ast)
        {
            ast.Args.ForEach(arg => arg.Visit(this));

            var className = Resolve(ast.Name);

            if (className == null)
            {
                throw new InvalidSyntax(String.Format("Class {0} is undefined", ast.Name.Token.TokenValue));
            }

            if (ResolvingTypes)
            {
                ast.AstSymbolType = className.Type;
            }

            SetScope(ast);
        }
    }
}

