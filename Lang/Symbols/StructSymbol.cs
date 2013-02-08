using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Symbols.Symbosl2;

namespace Lang.Symbols
{
    public class StructSymbol : ISymbol
    {
        public String Name { get; set; }

        public Dictionary<String, ISymbol> Symbols { get; set; }
    }
}
