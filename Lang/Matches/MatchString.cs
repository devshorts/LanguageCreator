using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Lexers;

namespace Lang.Matches
{
    public class MatchString : MatcherBase
    {
        public const string QUOTE = "\"";

        public const string TIC = "'";

        private String StringDelim { get; set; }

        public MatchString(String delim)
        {
            StringDelim = delim;
        }

        protected override Token IsMatchImpl(Tokenizer tokenizer)
        {
            var str = new StringBuilder();

            if (tokenizer.Current == StringDelim)
            {
                tokenizer.Consume();

                while (!tokenizer.End() && tokenizer.Current != StringDelim)
                {
                    str.Append(tokenizer.Current);
                    tokenizer.Consume();
                }

                if (tokenizer.Current == StringDelim)
                {
                    tokenizer.Consume();
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
