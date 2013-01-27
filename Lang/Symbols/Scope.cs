using System;
using Lang.Data;
using System.Collections.Generic;

namespace Lang.Symbols
{
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

            Define(new BuiltInType("int"));
            Define(new BuiltInType("void"));
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
