using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;

namespace Lang.Symbols
{
    [Serializable]
    public class MethodSymbol : Symbol
    {
        public MethodDeclr MethodDeclr { get; private set; }

        public MethodSymbol(string name, IType type, MethodDeclr declr) : base(name, type)
        {
            MethodDeclr = declr;
        }

        public MethodSymbol(string name) : base(name)
        {
        }
    }
}
