using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class MethodDeclr : Ast
    {
        public MethodDeclr(Token token, Token returnType, Token funcName, List<ArgumentDeclr> arguments, List<Ast> body)
            : base(token)
        {
            AddChild(new Expr(returnType));
            AddChild(new Expr(funcName));

            if (!CollectionUtil.IsNullOrEmpty(arguments))
            {
                arguments.ForEach(AddChild);
            }

            if (!CollectionUtil.IsNullOrEmpty(body))
            {
                body.ForEach(AddChild);
            }
        }

        public MethodDeclr(Token token, Token returnType, Token funcName, List<ArgumentDeclr> arguments, Ast body)
            : this(token, returnType, funcName, arguments, new List<Ast>{body} )
        {
            
        }
    }
}
