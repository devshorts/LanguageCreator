using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public class LambdaDeclr : MethodDeclr
    {
        public static int LambdaCount { get; set; }

        private static Random _rand = new Random();

        public LambdaDeclr(List<Ast> arguments, ScopeDeclr body )
            : base(new Token(TokenType.Infer), new Token(TokenType.Word, AnonymousFunctionName), arguments, body, true)
        {
        }

         private static string AnonymousFunctionName
         {
             get
             {
                 LambdaCount++;
                 return "anonymous" + LambdaCount;
             }
         }

    }
}
