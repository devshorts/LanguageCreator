using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lang.Data;
using Lang.Matches;

namespace Lang
{
    public class MatchKeyword : MatcherBase
    {
        public string Match { get; set; }

        private TokenType TokenType { get; set; }


        /// <summary>
        /// If true then matching on { in a string like "{test" will match the first cahracter
        /// because it is not space delimited. If false it must be space or special character delimited
        /// </summary>
        public Boolean AllowAsSubString { get; set; }

        public List<MatchKeyword> SpecialCharacters { get; set; } 

        public MatchKeyword(TokenType type, String match)
        {
            Match = match;
            TokenType = type;
            AllowAsSubString = true;
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

            bool found;

            if (!AllowAsSubString)
            {
                var next = lexer.Current;

                found = String.IsNullOrWhiteSpace(next) || SpecialCharacters.Any(character => character.Match == next);
            }
            else
            {
                found = true;
            }

            if (found)
            {
                return new Token(TokenType, Match);
            }

            return null;
        }
    }
}
