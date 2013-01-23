using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.Matches
{
    public class MatchWord : MatcherBase
    {
        private List<MatchKeyword> KeywordMatchers { get; set; } 
        public MatchWord(IEnumerable<IMatcher> keywordMatchers)
        {
            KeywordMatchers = keywordMatchers.Select(i=>i as MatchKeyword).Where(i=> i != null).ToList();
        }

        protected override Token IsMatchImpl(Lexer lexer)
        {
            String current = null;

            while (!lexer.End() && !String.IsNullOrWhiteSpace(lexer.Current) && KeywordMatchers.All(m => m.Match != lexer.Current))
            {
                current += lexer.Current;
                lexer.Consume();
            }

            if (current == null)
            {
                return null;
            }

            return new Token(TokenType.Word, current);
        }
    }
}
