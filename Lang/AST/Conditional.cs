using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class Conditional : Ast
    {
        public Ast Predicate { get; set; }

        public ScopeDeclr Body { get; set; }

        public Conditional Alternate { get; set; }

        public Conditional(Token token) : base(token)
        {
        }

        public Conditional(Token conditionalType, Ast predicate, ScopeDeclr body, Conditional alternate = null)
            : this(conditionalType)
        {
            Predicate = predicate;
            Body = body;
            Alternate = alternate;
        }

        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes Type
        {
            get { return AstTypes.Conditional; }
        }

        public override string ToString()
        {
            return "(" + Token + "(" + Predicate + ") then " + Body + (Alternate != null ? " else " + Alternate : "");
        }
    }
}
