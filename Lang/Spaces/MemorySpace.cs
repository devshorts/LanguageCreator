using System;
using Lang.Data;
using System.Collections.Generic;
using Lang.Spaces;

namespace Lang.Symbols
{
    public class MemorySpace : IScopeable<MemorySpace>
    {
        public Dictionary<string, object> Values { get; set; }

        public MemorySpace EnclosingSpace { get; private set; }

        public MemorySpace()
        {
            Values = new Dictionary<string, object>();

            ChildScopes = new List<IScopeable<MemorySpace>>(64);
        }

        public void Define(string name, object value)
        {
            Values[name] = value;
        }

        public void Assign(string name, object value)
        {
            if (Values.ContainsKey(name))
            {
                Values[name] = value;

                return;
            }

            if (EnclosingSpace != null)
            {
                EnclosingSpace.Assign(name, value);
            }
            else
            {
                throw new Exception(
                    String.Format(
                        "Attempting to update variable {0} with value {1} but varialbe isn't defined in any memory scope",
                        name, value));
            }
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
