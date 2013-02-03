using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    public interface IType
    {
        String TypeName { get; }
        ExpressionTypes ExpressionType { get; set; }
    }
}
