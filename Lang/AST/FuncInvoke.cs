using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    class FuncInvoke : Ast
    {
        public FuncInvoke(Token token, List<Ast> args) : base(token)
        {
            args.ForEach(AddChild);
        }
    }
}
