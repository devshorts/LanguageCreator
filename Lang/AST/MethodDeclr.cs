using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class MethodDeclr : Ast
    {
        public Ast MethodName { get; private set; }

        public Ast MethodReturnType { get; private set; }

        public List<Ast> Arguments { get; private set; }

        public List<Ast> BodyStatements { get; private set; } 

        public MethodDeclr(Token token, Token returnType, Token funcName, List<Ast> arguments, List<Ast> body)
            : base(token)
        {
            MethodReturnType = new Expr(returnType);

            MethodReturnType = new Expr(funcName);

            Arguments = arguments;

            BodyStatements = body;
        }
    }
}
