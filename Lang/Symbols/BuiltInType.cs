using System;

namespace Lang.Symbols
{
    [Serializable]
    public class BuiltInType : Symbol, IType
    {
        public BuiltInType(ExpressionTypes type)
            : base(type.ToString())
        {
            ExpressionType = type;
        }

        public string TypeName
        {
            get { return Name; }
        }

        public ExpressionTypes ExpressionType { get; set; }
    }
}
