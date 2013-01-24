using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Parser
{
    public class ParseExpression
    {
        private ParseableTokenStream TokenStream { get; set; }
        public ParseExpression(ParseableTokenStream tokenStream)
        {
            TokenStream = tokenStream;
        }
    }
}
