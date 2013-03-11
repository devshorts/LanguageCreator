using Lang.AST;

namespace Lang.Visitors
{
    public interface IAstVisitor
    {
        void Visit(Conditional ast);
        void Visit(Expr ast);
        void Visit(FuncInvoke ast);
        void Visit(VarDeclrAst ast);
        void Visit(MethodDeclr ast);
        void Visit(WhileLoop ast);
        void Visit(ScopeDeclr ast);
        void Visit(ForLoop ast);
        void Visit(ReturnAst ast);
        void Visit(PrintAst ast);

        void Start(Ast ast);
        void Visit(ClassAst ast);
        void Visit(ClassReference ast);
        void Visit(NewAst ast);
        void Visit(TryCatchAst ast);
        void Visit(ArrayIndexAst ast);
    }
}
