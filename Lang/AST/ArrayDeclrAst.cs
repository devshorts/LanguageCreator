using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    class ArrayDeclrAst : VarDeclrAst
    {
        protected ArrayDeclrAst(Token token) : base(token)
        {
            IsArray = true;
        }

        public ArrayDeclrAst(Token declType, Token name) : base(declType, name)
        {
            IsArray = true;
        }

        public ArrayDeclrAst(Token declType, Token name, Ast value) : base(declType, name, value)
        {
            IsArray = true;
        }
    }
}
