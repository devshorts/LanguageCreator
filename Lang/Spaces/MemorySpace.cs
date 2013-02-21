using System;
using Lang.Data;
using System.Collections.Generic;
using Lang.Spaces;

namespace Lang.Symbols
{
    public class MemorySpace : IScopeable<MemorySpace>
    {
        public Dictionary<string, object> Values { get; set; }

        public Dictionary<string, string> Links { get; set; }
 
        public MemorySpace EnclosingSpace { get; private set; }

        public MemorySpace()
        {
            Values = new Dictionary<string, object>();

            ChildScopes = new List<IScopeable<MemorySpace>>(64);

            Links = new Dictionary<string, string>();
        }

        public void Define(string name, object value)
        {
            Values[name] = value;
        }

        public void Link(string target, string source)
        {
            Links[target] = source;
        }

        public void Assign(string name, object value)
        {
            string link;
            while (Links.TryGetValue(name, out link))
            {
                name = link;
            }

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

        public object Get(string name, bool local = false)
        {
            string link;

            if (Links.TryGetValue(name, out link))
            {
                return Get(link);
            }

            object o;
            if (Values.TryGetValue(name, out o))
            {
                return o;
            }

            if (EnclosingSpace != null && local == false)
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
