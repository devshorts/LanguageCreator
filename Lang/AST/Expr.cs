using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class Expr : Ast
    {
        public Ast Left { get; private set; }

        public Ast Right { get; private set; }

        public Expr(Token token) : base(token)
        {
        }

        public Expr(Ast left, Token token, Ast right)
            : base(token)
        {
            Left = left;
            Right = right;
        }


        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            if (Left == null && Right == null)
            {
                return Token.ToString();
            }

            return "(" + Left + " " + Token + " " + Right + ")";
        }
    }
}
