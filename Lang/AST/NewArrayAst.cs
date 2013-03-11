using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.AST
{
    class NewArrayAst : NewAst
    {
        public NewArrayAst(Ast name, Ast size) : base(name, new List<Ast>{size})
        {
            IsArray = true;
        }
    }
}
