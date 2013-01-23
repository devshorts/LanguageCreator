using System;
using System.Globalization;
using Lang.Data;
using Lang.Matches;

namespace Lang
{
    public class MatchKeyword : MatcherBase
    {
        public string Match { get; set; }

        private TokenType TokenType { get; set; }

        private Boolean IgnoreWhiteSpace { get; set; }

        public MatchKeyword(TokenType type, String match, Boolean ignoreWhiteSpace = true)
        {
            Match = match;
            TokenType = type;
            IgnoreWhiteSpace = ignoreWhiteSpace;
        }

        protected override Token IsMatchImpl(Lexer lexer)
        {
            foreach (var character in Match)
            {
                if (lexer.Current == character.ToString(CultureInfo.InvariantCulture))
                {
                    lexer.Consume();
                }
                else
                {
                    return null;
                }
            }

            return new Token(TokenType, Match);
        }
    }
}
