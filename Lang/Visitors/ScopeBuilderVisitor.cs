using System;
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
        public Scope Current { get { return ScopeTree.Current; } }

        public ScopeStack<Scope> ScopeTree { get; private set; }

        public ScopeBuilderVisitor()
        {
            ScopeTree = new ScopeStack<Scope>();
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

            if (ast.Left == null && ast.Right == null)
            {
                ast.AstSymbolType = ResolveOrDefine(ast);
            }
            else
            {
                ast.AstSymbolType = GetExpressionType(ast.Left, ast.Right, ast.Token);
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
                case TokenType.Word:
                    var resolved = Current.Resolve(ast.Token.TokenValue);
                    if (resolved == null)
                    {
                        throw new UndefinedElementException(String.Format("{0} is undefined", ast.Token.TokenValue));
                    }

                    return resolved.Type;
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
                
                case TokenType.Infer:
                    return right.AstSymbolType;
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

            ast.CurrentScope = Current;
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

                if (isVar)
                {
                    ast.AstSymbolType = ast.VariableValue.AstSymbolType;

                    var symbol = DefineUserSymbol(ast.AstSymbolType, ast.VariableName);

                    Current.Define(symbol);
                }
            }

            ast.CurrentScope = Current;
        }

        private Symbol GetName(Ast ast)
        {
            return new Symbol(ast.Token.TokenValue);
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
                    return new BuiltInType(ExpressionTypes.Int);
                case TokenType.Float:
                    return new BuiltInType(ExpressionTypes.Float);
                case TokenType.Void:
                    return new BuiltInType(ExpressionTypes.Void);
                case TokenType.QuotedString:
                case TokenType.String:
                    return new BuiltInType(ExpressionTypes.String);
                case TokenType.Word:
                    return new UserDefinedType(astType.Token.TokenValue);
                case TokenType.True:
                case TokenType.False:
                    return new BuiltInType(ExpressionTypes.Boolean);
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
            var symbol = DefineMethod(ast);

            ValidateReturnStatementType(ast, symbol);

            Current.Define(symbol);

            ScopeTree.CreateScope();

            ast.Arguments.ForEach(arg => arg.Visit(this));

            ast.Body.Visit(this);

            ast.CurrentScope = Current;

            ast.AstSymbolType = symbol.Type;

            ScopeTree.PopScope();
        }

        private void ValidateReturnStatementType(MethodDeclr ast, Symbol symbol)
        {
            var visitor = new ReturnVisitor();

            visitor.Visit(ast);

            IType returnStatementType;

            // no return found
            if (visitor.ReturnType == null)
            {
                returnStatementType = new BuiltInType(ExpressionTypes.Void);
            }
            else
            {
                returnStatementType = CreateSymbolType(visitor.ReturnType);
            }

            var delcaredSymbol = CreateSymbolType(ast.MethodReturnType);

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

            ast.CurrentScope = Current;
        }

        public void Visit(ScopeDeclr ast)
        {
            ScopeTree.CreateScope();

            ast.ScopedStatements.ForEach(statement => statement.Visit(this));

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
            if (ast.ReturnExpression != null)
            {
                ast.ReturnExpression.Visit(this);
            }
        }

        public void Visit(PrintAst ast)
        {
            ast.Expression.Visit(this);
        }
    }
}

