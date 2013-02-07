using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Symbols;

namespace Lang.Utils
{
    public static class TokenUtil
    {
        public static Boolean EqualOrPromotable(IType item1, IType item2)
        {
            return item1.ExpressionType == item2.ExpressionType || item1.TypeName == "Inferred" || item2.TypeName == "Inferred";
        }

        public static Boolean IsOperator(Token item)
        {
            switch (item.TokenType)
            {
                case TokenType.Equals:
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Asterix:
                case TokenType.Carat:
                case TokenType.GreaterThan:
                case TokenType.LessThan:
                case TokenType.Ampersand:
                case TokenType.Or:
                case TokenType.Slash:
                    return true;
            }
            return false;
        }

        public static bool EqualOrPromotable(ExpressionTypes item1, ExpressionTypes item2)
        {
            return item1 == item2 || item1 == ExpressionTypes.Inferred || item2 == ExpressionTypes.Inferred;
        }
    }
}
