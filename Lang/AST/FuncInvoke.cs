using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class FuncInvoke : Ast
    {
        public List<Ast> Arguments { get; private set; }

        public Ast FunctionName { get; set; }

        public FuncInvoke(Token token, List<Ast> args) : base(token)
        {
            FunctionName = new Expr(token);

            Arguments = args;
        }


        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
