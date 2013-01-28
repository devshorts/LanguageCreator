using System;
using Lang.Data;
using System.Collections.Generic;
using Lang.Spaces;

namespace Lang.Symbols
{
    public class MemorySpace : IScopeable<MemorySpace>
    {
        private Dictionary<string, object> Values { get; set; }

        public MemorySpace EnclosingSpace { get; private set; }

        public MemorySpace()
        {
            Values = new Dictionary<string, object>();

            ChildScopes = new List<IScopeable<MemorySpace>>(64);
        }

        public void Assign(string name, object value)
        {
            Values[name] = value;
        }

        public object Get(string name)
        {
            object o;
            if (Values.TryGetValue(name, out o))
            {
                return o;
            }

            if (EnclosingSpace != null)
            {
                return EnclosingSpace.Get(name);
            }

            return null;
        }

        public void SetParentScope(MemorySpace scope)
        {
            EnclosingSpace = scope;
        }

        public List<IScopeable<MemorySpace>> ChildScopes { get; private set; }
    }
}
