using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class ReturnAst : Ast
    {
        public Ast ReturnExpression { get; private set; }
        public ReturnAst(Ast expression) : base(new Token(TokenType.Return))
        {
            ReturnExpression = expression;
        }

        public ReturnAst()
            : base(new Token(TokenType.Return))
        {
            
        }

        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.Return; }
        }
    }
}
