using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;

namespace Lang.Symbols
{
    public interface IType
    {
        String TypeName { get; }
        ExpressionTypes ExpressionType { get; set; }
        Ast Src { get; set; }
    }
}
