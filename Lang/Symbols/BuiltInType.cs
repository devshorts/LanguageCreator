using System;
using Lang.AST;

namespace Lang.Symbols
{
    [Serializable]
    public class BuiltInType : Symbol, IType
    {
        public BuiltInType(ExpressionTypes type, Ast src = null)
            : base(type.ToString())
        {
            ExpressionType = type;

            Src = src;
        }

        public string TypeName
        {
            get { return Name; }
        }

        public ExpressionTypes ExpressionType { get; set; }

        public Ast Src { get; set; }
    }
}
