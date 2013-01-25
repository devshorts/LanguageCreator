using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    public class Scope
    {
        private List<Symbol> Symbols { get; set; }

        public Scope EnclosingScope { get; private set; }

        public List<Scope> ChildScopes { get; private set; } 
        
        public Scope(Scope enclosingScope = null)
        {
            Symbols = new List<Symbol>(64);

            EnclosingScope = enclosingScope;

            ChildScopes = new List<Scope>(64);
        }

        public String ScopeName
        {
            get { return "current"; }
        }

        public void Define(Symbol symbol)
        {
            Symbols.Add(symbol);
        }

        public Symbol Resolve(String name)
        {
            return Symbols.FirstOrDefault(s => s.Name == name);
        }
    }
}
