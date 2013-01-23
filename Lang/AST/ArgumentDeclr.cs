using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class ArgumentDeclr : VarDeclrAst
    {
        public ArgumentDeclr(Token token) : base(token)
        {
        }

        public ArgumentDeclr(Token declType, Token name) : base(declType, name)
        {
        }

        public ArgumentDeclr(Token declType, Token name, Ast value) : base(declType, name, value)
        {
        }
    }
}
