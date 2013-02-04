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

        public IType ReturnType { get; private set; }

        public MethodSymbol(string name, IType returnType, MethodDeclr declr)
            : base(name, new BuiltInType(ExpressionTypes.Method))
        {
            MethodDeclr = declr;

            ReturnType = returnType;
        }

        public MethodSymbol(string name) : base(name)
        {
        }
    }
}
