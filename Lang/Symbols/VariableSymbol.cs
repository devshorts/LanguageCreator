using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    [Serializable]
    public class VariableSymbol : Symbol
    {
        public VariableSymbol(string name, IType type) : base(name, type)
        {
        }
    }
}
