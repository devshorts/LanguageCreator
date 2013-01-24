using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    class ScopeDeclr : Ast
    {
        public ScopeDeclr(ICollection<Ast> statements) : base(new Token(TokenType.ScopeStart))
        {
            if (!CollectionUtil.IsNullOrEmpty(statements))
            {
                statements.ForEach(AddChild);
            }
        }
    }
}
