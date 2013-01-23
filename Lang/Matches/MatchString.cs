using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.Matches
{
    public class MatchString : MatcherBase
    {
        protected override Token IsMatchImpl(Lexer lexer)
        {
            String str = null;

            if (lexer.Current == "\"")
            {
                lexer.Consume();

                while (!lexer.End() && lexer.Current != "\"")
                {
                    str += lexer.Current;
                    lexer.Consume();
                }

                if (lexer.Current == "\"")
                {
                    lexer.Consume();
                }
            }

            if (str != null)
            {
                return new Token(TokenType.QuotedString, str);
            }

            return null;
        }
    }
}
