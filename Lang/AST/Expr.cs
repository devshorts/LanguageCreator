using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class Expr : Ast
    {
        public Expr(Token token) : base(token)
        {
        }

        public Expr(Ast left, Token token, Ast right)
            : base(token)
        {
            AddChild(left);
            AddChild(right);
        }
    }
}
