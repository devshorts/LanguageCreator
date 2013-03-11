using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class ArrayIndexAst : Ast
    {
        public Ast Name { get; set; }

        public Ast Index { get; set; }

        public ArrayIndexAst(Ast name, Ast index) : base(name.Token)
        {
            Name = name;
            Index = index;
        }

        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.ArrayIndex; }
        }
    }
}
