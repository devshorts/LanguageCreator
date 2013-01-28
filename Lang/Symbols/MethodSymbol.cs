using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    [Serializable]
    public class MethodSymbol : Symbol
    {
        public MethodSymbol(string name, IType type) : base(name, type)
        {
        }

        public MethodSymbol(string name) : base(name)
        {
        }
    }
}
