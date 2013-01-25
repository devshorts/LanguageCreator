using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class WhileLoop : Ast
    {
        public Ast Predicate { get; private set; }

        public List<Ast> Body { get; private set; } 

        public WhileLoop(Token token) : base(token)
        {
        }

        public WhileLoop(Ast predicate, List<Ast> body) : this(new Token(TokenType.While))
        {
            Predicate = predicate;
            Body = body;
        }
    }
}
