using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    public class ClassSymbol : Symbol, IType
    {
        public ClassSymbol(string name)
            : base(name)
        {
        }

        public string TypeName
        {
            get { return Name; }
        }
    }
}
