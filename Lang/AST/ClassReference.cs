using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class ClassReference : Ast
    {
        public Ast Next { get; set; }

        public Ast Current { get; set; }

        public ClassReference(Ast current, Ast next = null) : base(current.Token)
        {
            Current = current;
            Next = next;
        }


        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.ClassRef; }
        }

        public override string ToString()
        {
            return Current != null ? Current + (Next != null ? Next.ToString() : "") : "";
        }
    }
}
