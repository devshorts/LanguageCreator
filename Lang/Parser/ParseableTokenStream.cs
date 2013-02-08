using System;
using System.Collections.Generic;
using System.Linq;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;

namespace Lang.Parser
{
    internal class Memo
    {
        public Ast Ast { get; set; }
        public int NextIndex { get; set; }
    }

    public class ParseableTokenStream : LexableStreamBase<Token>
    {
        public ParseableTokenStream(Tokenizer tokenizer) : base (() => tokenizer.Tokenize().ToList())
        {
        }

        private Dictionary<int, Memo> CachedAst = new Dictionary<int, Memo>();

        public Boolean IsMatch(TokenType type)
        {
            if (Current.TokenType == type)
            {
                return true;
            }

            return false;
        }

        public Ast Capture(Func<Ast> ast)
        {
            if (Alt(ast))
            {
                return Get(ast);
            }

            return null;
        }

        public Token Take(TokenType type)
        {
            if (IsMatch(type))
            {
                var current = Current;

                Consume();

                return current;
            }

            throw new InvalidSyntax(String.Format("Invalid Syntax. Expecting {0} but got {1}", type, Current.TokenType));
        }


        /// <summary>
        /// Retrieves a cached version if it was found during any alternate route
        /// otherwise executes it
        /// </summary>
        /// <param name="getter"></param>
        /// <returns></returns>
        public Ast Get(Func<Ast> getter)
        {
            Memo memo;
            if (!CachedAst.TryGetValue(Index, out memo))
            {
                return getter();    
            }

            Index = memo.NextIndex;

            return memo.Ast;
        } 

        public Boolean Alt(Func<Ast> action)
        {
            TakeSnapshot();

            Boolean found = false;

            try
            {
                var currentIndex = Index;

                var ast = action();

                if (ast != null)
                {
                    found = true;

                    CachedAst[currentIndex] = new Memo
                                              {
                                                  Ast = ast,
                                                  NextIndex = Index
                                              };
                }
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

        public override Token Current
        {
            get
            {
                var current = base.Current;
                if (current == null)
                {
                    return new Token(TokenType.EOF);
                }
                return current;
            }
        }
    }
}
