using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class ScopeDeclr : Ast
    {
        public List<Ast> ScopedStatements { get; private set; } 

        public ScopeDeclr(List<Ast> statements) : base(new Token(TokenType.ScopeStart))
        {
            ScopedStatements = statements;
        }
    }
}
