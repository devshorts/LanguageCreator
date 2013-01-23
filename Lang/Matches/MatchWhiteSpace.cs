using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.Matches
{
    class MatchWhiteSpace : MatcherBase
    {
        protected override Token IsMatchImpl(Lexer lexer)
        {
            bool foundWhiteSpace = false;

            while (!lexer.End() && String.IsNullOrWhiteSpace(lexer.Current))
            {
                foundWhiteSpace = true;

                lexer.Consume();
            }

            if (foundWhiteSpace)
            {
                return new Token(TokenType.WhiteSpace);
            }

            return null;
        }
    }
}
