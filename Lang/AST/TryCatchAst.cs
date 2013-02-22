using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class TryCatchAst : Ast
    {
        public ScopeDeclr TryBody { get; set; }
        public ScopeDeclr CatchBody { get; set; }

        public TryCatchAst(ScopeDeclr tryBody, ScopeDeclr catchBody) : base(new Token(TokenType.Try))
        {
            TryBody = tryBody;
            CatchBody = catchBody;
        }

        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.TryCatch; }
        }
    }
}
