using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class VarDeclrAst : Ast
    {
        public VarDeclrAst(Token token) : base(token)
        {
        }

        public VarDeclrAst(Token declType, Token name)
            : base(name)
        {
            AddChild(new Expr(declType));
        }

        public VarDeclrAst(Token declType, Token name, Ast value)
            : base(name)
        {
            AddChild(new Expr(declType));
            AddChild(value);
        }
    }
}
