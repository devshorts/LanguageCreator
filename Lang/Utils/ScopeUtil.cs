using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;
using Lang.Data;
using Lang.Symbols;

namespace Lang.Utils
{
    class ScopeUtil
    {
        /// <summary>
        /// Determines user type
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IType GetExpressionType(Ast left, Ast right, Token token)
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

        public static IType CreateSymbolType(Ast astType)
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
                case TokenType.Nil:
                    return new BuiltInType(ExpressionTypes.Nil);
                case TokenType.Infer:
                    return new BuiltInType(ExpressionTypes.Inferred);
                case TokenType.QuotedString:
                case TokenType.String:
                    return new BuiltInType(ExpressionTypes.String);
                case TokenType.Word:
                    return new UserDefinedType(astType.Token.TokenValue);
                case TokenType.True:
                case TokenType.False:
                    return new BuiltInType(ExpressionTypes.Boolean);
                case TokenType.Method:
                    return new BuiltInType(ExpressionTypes.Method);
            }

            return null;
        }

        public static Symbol DefineUserSymbol(Ast astType, Ast name)
        {
            IType type = CreateSymbolType(astType);

            return new Symbol(name.Token.TokenValue, type);
        }

        public static Symbol DefineUserSymbol(IType type, Ast name)
        {
            return new Symbol(name.Token.TokenValue, type);
        }

        public static Symbol DefineMethod(MethodDeclr method)
        {
            IType returnType = CreateSymbolType(method.MethodReturnType);

            return new MethodSymbol(method.Token.TokenValue, returnType, method);
        }

    }
}
