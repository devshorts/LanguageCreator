using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class PrintAst : Ast
    {
        public Ast Expression { get; private set; }
        public PrintAst(Ast expression) : base(new Token(TokenType.Print))
        {
            Expression = expression;
        }

        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.Print; }
        }
    }
}
