using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class ForLoop : Ast
    {
        public Ast Initial { get; private set; }

        public Ast Stop { get; private set; }

        public Ast Modify { get; private set; }

        public ScopeDeclr Body { get; private set; }

        public ForLoop(Ast init, Ast stop, Ast modify, ScopeDeclr body)
            : base(new Token(TokenType.For))
        {
            Initial = init;

            Stop = stop;

            Modify = modify;

            Body = body;
        }

        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.For; }
        }

        public override string ToString()
        {
            return "(" + Token + "(" + Initial + ") (" + Stop + ")" + "(" + Modify +"){" + Body + "}";
        }
    }
}
