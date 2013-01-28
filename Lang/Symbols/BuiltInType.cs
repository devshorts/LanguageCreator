using System;

namespace Lang.Symbols
{
    [Serializable]
    public class BuiltInType : Symbol, IType
    {
        public BuiltInType(string name)
            : base(name)
        {
        }

        public string TypeName
        {
            get { return Name; }
        }
    }
}
