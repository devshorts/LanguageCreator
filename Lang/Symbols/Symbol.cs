using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    public class Symbol
    {
        public String Name { get; private set; }
        public IType Type { get; private set; }

        public Symbol(String name, IType type)
        {
            Name = name;
            Type = type;
        }

        public Symbol(String name)
        {
            Name = name;
        }
    }
}
