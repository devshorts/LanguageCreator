using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    public class BasicType : Symbol, IType
    {
        public BasicType(string name) : base(name)
        {
        }

        public string TypeName
        {
            get { return Name; }
        }
    }
}
