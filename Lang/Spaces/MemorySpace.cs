using System;
using Lang.Data;
using System.Collections.Generic;

namespace Lang.Symbols
{
    public class MemorySpace
    {
        private Dictionary<string, object> Values { get; set; }

        public MemorySpace EnclosingScope { get; private set; }

        public MemorySpace(MemorySpace enclosingScope = null)
        {
            Values = new Dictionary<string, object>();

            EnclosingScope = enclosingScope;
        }

        public void Assign(string name, object value)
        {
            Values[name] = value;
        }

        public object Get(string name)
        {
            return Values[name];
        }
    }
}
