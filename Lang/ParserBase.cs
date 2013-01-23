using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Exceptions;

namespace Lang
{
    public abstract class ParserBase : LexableStreamBase<Token>
    {
        public ParserBase(Tokenizer tokenizer) : base (() => tokenizer.Tokenize().ToList())
        {
        }

        protected Boolean IsMatch(TokenType type)
        {
            if (Current.TokenType == type)
            {
                return true;
            }

            return false;
        }

        protected Token Take(TokenType type)
        {
            if (IsMatch(type))
            {
                var current = Current;

                Consume();

                return current;
            }

            throw new InvalidSyntax(String.Format("Invalid Syntax. Expecting {0} but got {1}", type, Current.TokenType));
        }

        protected Boolean Alt(Action action)
        {
            TakeSnapshot();

            Boolean found = false;

            try
            {
                action();

                found = true;

            }
            catch
            {
                
            }

            RollbackSnapshot();

            return found;
        }

        public override Token Peek(int lookahead)
        {
            var peeker = base.Peek(lookahead);

            if (peeker == null)
            {
                return new Token(TokenType.EOF);
            }

            return peeker;
        }
    }
}
