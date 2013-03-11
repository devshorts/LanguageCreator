using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lang.Data;
using Lang.Visitors;

namespace Lang.AST
{
    public class VarDeclrAst : Ast
    {
        public bool IsArray { get; set; }

        public Ast DeclarationType { get; private set; }

        public Ast VariableValue { get; private set; }

        public Ast VariableName { get; private set; }

        protected VarDeclrAst(Token token) : base(token)
        {
        }

        public VarDeclrAst(Token declType, Token name)
            : base(name)
        {
            DeclarationType = new Expr(declType);

            VariableName = new Expr(name);
        }

        public VarDeclrAst(Token declType, Token name, Ast value)
            : base(name)
        {
            DeclarationType = new Expr(declType);

            VariableValue = value;

            VariableName = new Expr(name);
        }


        public override void Visit(IAstVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override AstTypes AstType
        {
            get { return AstTypes.VarDeclr; }
        }

        public override string ToString()
        {
            return String.Format("Declare {0} as {1} with value {2}",
                                 VariableName, DeclarationType, VariableValue);
        }
    }
}
