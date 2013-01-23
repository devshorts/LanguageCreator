using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.Matches
{
    public abstract class MatcherBase : IMatcher
    {
        public Token IsMatch(Lexer lexer)
        {
            if (lexer.End())
            {
                return new Token(TokenType.EOF);
            }

            lexer.TakeSnapshot();

            var match = IsMatchImpl(lexer);

            if (match == null)
            {
                lexer.RollbackSnapshot();
            }
            else
            {
                lexer.CommitSnapshot();
            }

            return match;
        }

        protected abstract Token IsMatchImpl(Lexer lexer);
    }
}
