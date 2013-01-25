using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class WhileLoop : Ast
    {
        public Ast Predicate { get; private set; }

        public ScopeDeclr Body { get; private set; } 

        public WhileLoop(Token token) : base(token)
        {
        }

        public WhileLoop(Ast predicate, ScopeDeclr body)
            : this(new Token(TokenType.While))
        {
            Predicate = predicate;
            Body = body;
        }


        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
