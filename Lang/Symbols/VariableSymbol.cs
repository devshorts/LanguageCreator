using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    public class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, IType type) : base(name, type)
        {
        }
    }
}
