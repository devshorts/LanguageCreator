using System;
using System.Linq;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;
using NUnit.Framework;

namespace Lang.Tests
{
    [TestFixture]
    public class LangTests
    {
        [Test]
        public void TestTokenizer1()
        {
            var test = @"function = 1";

            var tokens = new Tokenizer(test).Tokenize().ToList();
        }

        [Test]
        public void TestTokenizer()
        {
            var test = @"function void int ""void int"" {} ->*/test^void,5,6,7";

            var tokens = new Tokenizer(test).Tokenize().ToList();
        }

        [Test]
        public void TestSimpleAst()
        {
            var test = @"x = 1;";

            var ast = new Parser(new Tokenizer(test)).Parse() as ScopeDeclr;

            var expr = (ast.ScopedStatements[0] as Expr);

            Assert.IsTrue(expr.Left.Token.TokenType == TokenType.Word);
            Assert.IsTrue(expr.Right.Token.TokenType == TokenType.Word);
            Assert.IsTrue(ast.Token.TokenType == TokenType.ScopeStart);
        }

        [Test]
        public void AstWithExpression()
        {
            var test = @"x = 1 + 2;";

            var ast = new Parser(new Tokenizer(test)).Parse() as ScopeDeclr;

            var expr = (ast.ScopedStatements[0] as Expr);

            Assert.IsTrue(expr.Left.Token.TokenType == TokenType.Word);
            Assert.IsTrue((expr.Right as Expr).Left.Token.TokenValue == "1");
            Assert.IsTrue((expr.Right as Expr).Right.Token.TokenValue == "2");
            Assert.IsTrue((expr.Right as Expr).Token.TokenValue == "+");
            Assert.IsTrue((expr.Right as Expr).Token.TokenType == TokenType.Plus);
        }

        [Test]
        public void AstWithExpression2()
        {
            var test = @"int z = 1;
                        {
                            int y = 5 + 4;
                        }
                        x = 1 + 2 ^ (5-7);";

            var ast = new Parser(new Tokenizer(test)).Parse() as ScopeDeclr;

            Assert.IsTrue(ast.ScopedStatements.Count == 3);
            Assert.IsTrue(ast.ScopedStatements[0] is VarDeclrAst);
            Assert.IsTrue(ast.ScopedStatements[1].Token.TokenType == TokenType.ScopeStart);
            Assert.IsTrue(ast.ScopedStatements[2] is Expr);
        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void AstWithExpressionFailure()
        {
            var test = @"int z = 1;
                        {
                            int y = 5 + 4{;
                        }
                        x = 1 + 2 ^ (5-7);"; 

            var ast = new Parser(new Tokenizer(test)).Parse();

        }

        [Test]
        public void FunctionTest()
        {
            var test = @"void foo(int x, int y){ 
                            int x = 1; 
                            var z = fun() -> { 
                                zinger = ""your mom!"";
                                someThing(a + b) + 25 - (""test"" + 5);
                            };
                        }

                        z = 3;

                        int testFunction(){
                            var p = 23;
                        }";

            var ast = new Parser(new Tokenizer(test)).Parse() as ScopeDeclr;

            Assert.IsTrue(ast.ScopedStatements.Count == 3);
            Assert.IsTrue(ast.ScopedStatements[0] is MethodDeclr);
            Assert.IsTrue(ast.ScopedStatements[1] is Expr);
            Assert.IsTrue(ast.ScopedStatements[2] is MethodDeclr);
        }

        [Test]
        public void FunctionInvokeTest()
        {
            var test = @"test(a, 1 + 2);";

            var ast = new Parser(new Tokenizer(test)).Parse();

        }
    }
}
