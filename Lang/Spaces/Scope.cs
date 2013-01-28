using System;
using System.Collections.Generic;
using Lang.Symbols;

namespace Lang.Spaces
{
    [Serializable]
    public class Scope
    {
        private Dictionary<string, Symbol> Symbols { get; set; }

        public Scope EnclosingScope { get; private set; }

        public List<Scope> ChildScopes { get; private set; } 
        
        public Scope(Scope enclosingScope = null)
        {
            Symbols = new Dictionary<string, Symbol>();

            EnclosingScope = enclosingScope;

            ChildScopes = new List<Scope>(64);

        }

        public String ScopeName
        {
            get { return "current"; }
        }

        public void Define(Symbol symbol)
        {
            Symbols[symbol.Name] = symbol;
        }

        public Symbol Resolve(String name)
        {
            Symbol o;
            if (Symbols.TryGetValue(name, out o))
            {
                return o;
            }

            if (EnclosingScope == null)
            {
                return null;
            }

            return EnclosingScope.Resolve(name);
        }
    }
}
