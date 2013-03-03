using System;
using System.Collections.Generic;
using Lang.AST;
using Lang.Symbols;

namespace Lang.Spaces
{
    [Serializable]
    public class Scope : IScopeable<Scope>
    {
        public Dictionary<string, Symbol> Symbols { get; set; }

        public Scope EnclosingScope { get; private set; }

        public Boolean AllowAllForwardReferences { get; set; }

        public void SetParentScope(Scope scope)
        {
            EnclosingScope = scope;
        }

        public List<IScopeable<Scope>> ChildScopes { get; private set; } 
        
        public Scope()
        {
            Symbols = new Dictionary<string, Symbol>();

            ChildScopes = new List<IScopeable<Scope>>(64);

            AllowAllForwardReferences = false;
        }

        public String ScopeName { get; set; }

        public void Define(Symbol symbol)
        {
            Symbols[symbol.Name] = symbol;
        }

        public Boolean AllowedForwardReferences(Ast ast)
        {
            if (Symbols.ContainsKey(ast.Token.TokenValue))
            {
                return AllowAllForwardReferences;
            }

            if (EnclosingScope == null)
            {
                return false;
            }

            return EnclosingScope.AllowedForwardReferences(ast);
        }

        public Symbol Resolve(Ast ast)
        {
            return Resolve(ast.Token.TokenValue);
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
