using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Exceptions;

namespace Lang.Matches
{
    public class MatchWord : MatcherBase
    {
        private List<MatchKeyword> SpecialCharacters { get; set; } 
        public MatchWord(IEnumerable<IMatcher> keywordMatchers)
        {
            SpecialCharacters = keywordMatchers.Select(i=>i as MatchKeyword).Where(i=> i != null).ToList();
        }

        protected override Token IsMatchImpl(Lexer lexer)
        {
            String current = null;

            while (!lexer.End() && !String.IsNullOrWhiteSpace(lexer.Current) && SpecialCharacters.All(m => m.Match != lexer.Current))
            {
                current += lexer.Current;
                lexer.Consume();
            }

            if (current == null)
            {
                return null;
            }

            // can't start a word with a special character
            if (SpecialCharacters.Any(c => current.StartsWith(c.Match)))
            {
                throw new InvalidSyntax(String.Format("Cannot start a word with a special character {0}", current));
            }

            return new Token(TokenType.Word, current);
        }
    }
}
