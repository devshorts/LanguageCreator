using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;

namespace Lang.AST
{
    public abstract class Ast
    {
        public Token Token { get; private set; }

        public List<Ast> Children { get; private set; } 

        public Ast(Token token)
        {
            Token = token;
            Children = new List<Ast>();
        }

        public void AddChild(Ast child)
        {
            if (child != null)
            {
                Children.Add(child);
            }
        }

        public override string ToString()
        {
            return Token.TokenType + " " + Children.Aggregate("", (acc, ast) => acc + " " + ast);
        }
    }
}
