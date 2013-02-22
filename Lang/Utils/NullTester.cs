using System;
using Lang.Data;

namespace Lang.Utils
{
    public static class NullTester
    {
        public static bool NullEqual(dynamic left, dynamic right)
        {
            if (left is TokenType && right is TokenType)
            {
                return left == TokenType.Nil && right == TokenType.Nil;
            }

            return false;
        }
    }
}
