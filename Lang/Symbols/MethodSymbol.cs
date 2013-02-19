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

        public MemorySpace Environment { get; set; }

        public MethodSymbol(string name, IType returnType, MethodDeclr declr)
            : base(name, returnType)
        {
            MethodDeclr = declr;
        }
    }
}
