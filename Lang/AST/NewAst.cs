using System.Collections.Generic;
using System.Linq;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class NewAst : Ast
    {
        public NewAst(Ast name, List<Ast> args) : base(name.Token)
        {
            Args = args;
            Name = name;
        }

        public List<Ast> Args { get; set; }

        public Ast Name { get; set; }

        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.New; }
        }

        public override string ToString()
        {
            return string.Format("new {0} with args {1}", Name, 
                CollectionUtil.IsNullOrEmpty(Args) ? "n/a" : Args.Aggregate(")", (item, acc) => item + acc + "\n"));
        }
    }
}
