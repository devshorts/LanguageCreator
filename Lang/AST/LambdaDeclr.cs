using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class LambdaDeclr : MethodDeclr
    {
        private static Random _rand = new Random();

        public LambdaDeclr(List<Ast> arguments, ScopeDeclr body )
            : base(new Token(TokenType.Infer), new Token(TokenType.Word, AnonymousFunctionName), arguments, body)
        {
        }

         private static string AnonymousFunctionName
         {
             get { return "anonymous" + _rand.Next(); }
         }

    }
}
