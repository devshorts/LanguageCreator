using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class ClassAst : Ast
    {
        public ScopeDeclr Body { get; set; }

        public ClassAst(Token token, ScopeDeclr body) : base(token)
        {
            Body = body;
        }

        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.Class; }
        }
    }
}
