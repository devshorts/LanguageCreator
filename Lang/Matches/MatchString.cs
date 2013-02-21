using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.Matches
{
    public class MatchString : MatcherBase
    {
        public static string QUOTE = "\"";

        public static string TIC = "'";

        private String StringDelim { get; set; }

        public MatchString(String delim)
        {
            StringDelim = delim;
        }

        protected override Token IsMatchImpl(Lexer lexer)
        {
            var str = new StringBuilder();

            if (lexer.Current == StringDelim)
            {
                lexer.Consume();

                while (!lexer.End() && lexer.Current != StringDelim)
                {
                    str.Append(lexer.Current);
                    lexer.Consume();
                }

                if (lexer.Current == StringDelim)
                {
                    lexer.Consume();
                }
            }

            if (str.Length > 0)
            {
                return new Token(TokenType.QuotedString, str.ToString());
            }

            return null;
        }
    }
}
