using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Matches;

namespace Lang
{
    public class Tokenizer
    {
        private Lexer Lexer { get; set; }

        private List<Token> Tokens { get; set; }

        private List<IMatcher> Matchers { get; set; } 


        public Tokenizer(String source)
        {
            Lexer = new Lexer(source);
            Tokens = new List<Token>(1024);
        }

        public IEnumerable<Token> Tokenize()
        {
            Matchers = InitializeMatchList();

            var current = Next();

            while (current != null && current.TokenType != TokenType.EOF)
            {
                // skip whitespace
                if (current.TokenType != TokenType.WhiteSpace)
                {
                    yield return current;
                }

                current = Next();
            }
        }

        private List<IMatcher> InitializeMatchList()
        {
            // the order here matters because it defines token precedence

            var matchers = new List<IMatcher>(64);

            var keywordmatchers = new List<IMatcher>
                                  {
                                      new MatchKeyword(TokenType.Void, "void", false),
                                      new MatchKeyword(TokenType.Int, "int", false),
                                      new MatchKeyword(TokenType.Fun, "fun", false)
                                  };

            var specialCharacters = new List<IMatcher>
                                    {
                                        new MatchKeyword(TokenType.DeRef, "->"),
                                        new MatchKeyword(TokenType.LBracket, "{"),
                                        new MatchKeyword(TokenType.RBracket, "}"),
                                        new MatchKeyword(TokenType.Plus, "+"),
                                        new MatchKeyword(TokenType.Minus, "-"),
                                        new MatchKeyword(TokenType.Equals, "="),
                                        new MatchKeyword(TokenType.HashTag, "#"),
                                        new MatchKeyword(TokenType.Comma, ","),
                                        new MatchKeyword(TokenType.OpenParenth, "("),
                                        new MatchKeyword(TokenType.CloseParenth, ")"),
                                        new MatchKeyword(TokenType.Asterix, "*"),
                                        new MatchKeyword(TokenType.Slash, "/"),
                                        new MatchKeyword(TokenType.Carat, "^"),
                                        new MatchKeyword(TokenType.Ampersand, "&"),
                                        new MatchKeyword(TokenType.GreaterThan, ">"),
                                        new MatchKeyword(TokenType.LessThan, "<"),
                                        new MatchKeyword(TokenType.SemiColon, ";"),
                                    };

            matchers.Add(new MatchString());
            matchers.AddRange(specialCharacters);
            matchers.AddRange(keywordmatchers);
            matchers.AddRange(new List<IMatcher>
                                                {
                                                    new MatchWhiteSpace(),
                                                    new MatchWord(specialCharacters)
                                                });

            return matchers;
        }

        private Token Next()
        {
            if (Lexer.End())
            {
                return new Token(TokenType.EOF);
            }

            return 
                 (from match in Matchers
                 let token = match.IsMatch(Lexer)
                 where token != null
                 select token).FirstOrDefault();
        }
    }
}
