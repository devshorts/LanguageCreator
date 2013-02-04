using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.Utils
{
    public static class TokenUtil
    {
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
    }
}
