using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.AST;
using Lang.Data;

namespace Lang.Visitors
{
    public class InterpretorVisitor : IAstVisitor
    {
        private ScopeBuilderVisitor ScopeBuilder { get; set; }

        public InterpretorVisitor()
        {
            ScopeBuilder = new ScopeBuilderVisitor();
        }


        public void Visit(Conditional ast)
        {
            
        }

        public void Visit(Expr ast)
        {
            
        }

        public void Visit(FuncInvoke ast)
        {
            
        }

        public void Visit(VarDeclrAst ast)
        {
            
        }

        public void Visit(MethodDeclr ast)
        {
            
        }

        public void Visit(WhileLoop ast)
        {
            
        }

        public void Visit(ScopeDeclr ast)
        {
            
        }

        public void Visit(ForLoop ast)
        {
            
        }

        public void Visit(ReturnAst ast)
        {
            
        }

        private Object Exec(Ast ast)
        {
            return null;
        }
    }
}
