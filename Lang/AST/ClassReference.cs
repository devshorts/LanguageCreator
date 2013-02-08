using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class ClassReference : Ast
    {
        public Ast ClassInstance { get; set; }

        public List<Ast> Deferences  { get; set; }

        public ClassReference(Ast classInstance, List<Ast> deferences)
            : base(classInstance.Token)
        {
            ClassInstance = classInstance;
            Deferences = deferences;
        }


        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.ClassRef; }
        }

        public override string ToString()
        {
            return ClassInstance + Deferences.Aggregate("", (acc, item) => acc + "." + item);
        }
    }
}
