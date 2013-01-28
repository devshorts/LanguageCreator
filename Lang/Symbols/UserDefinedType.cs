using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lang.Symbols
{
    [Serializable]
    public class UserDefinedType : Symbol, IType
    {
        public UserDefinedType(string name) : base(name)
        {
        }

        public string TypeName
        {
            get { return Name; }
        }
    }
}
