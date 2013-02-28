using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class ScopeDeclr : Ast
    {
        public List<Ast> ScopedStatements { get; private set; } 

        public ScopeDeclr(List<Ast> statements) : base(new Token(TokenType.ScopeStart))
        {
            ScopedStatements = statements;
        }


        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.ScopeDeclr; }
        }

        public override string ToString()
        {
            return ScopedStatements.Aggregate("", (item, acc) => acc + item);
        }
    }
}
