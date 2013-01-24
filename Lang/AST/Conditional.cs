using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class Conditional : Ast
    {
        public Ast Predicate { get; set; }

        public List<Ast> Body { get; set; }

        public Conditional Alternate { get; set; }

        public Conditional(Token token) : base(token)
        {
        }

        public Conditional(Token conditionalType, Ast predicate, List<Ast> body, Conditional alternate = null) : this(conditionalType)
        {
            Predicate = predicate;
            Body = body;
            Alternate = alternate;
        }
    }
}
