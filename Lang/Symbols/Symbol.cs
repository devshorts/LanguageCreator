using System;
using Lang.Spaces;

namespace Lang.Symbols
{
    [Serializable]
    public class Symbol : Scope
    {
        public String Name { get; private set; }
        public IType Type { get; private set; }

        public MemorySpace Memory { get; set; }

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
