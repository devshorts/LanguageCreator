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

            var ast = new Parser(new Tokenizer(test)).Parse();

            Assert.IsTrue(ast.Children[0].Children[0].Token.TokenType == TokenType.Word);
            Assert.IsTrue(ast.Children[0].Children[1].Token.TokenType == TokenType.Word);
            Assert.IsTrue(ast.Token.TokenType == TokenType.ScopeStart);
        }

        [Test]
        public void AstWithExpression()
        {
            var test = @"x = 1 + 2;";

            var ast = new Parser(new Tokenizer(test)).Parse();

            Assert.IsTrue(ast.Children[0].Children[0].Token.TokenType == TokenType.Word);
            Assert.IsTrue(ast.Children[0].Children[1].Children[0].Token.TokenType == TokenType.Word);
            Assert.IsTrue(ast.Children[0].Children[1].Children[1].Token.TokenType == TokenType.Word);
            Assert.IsTrue(ast.Children[0].Children[1].Token.TokenType == TokenType.Plus);
        }

        [Test]
        public void AstWithExpression2()
        {
            var test = @"int z = 1;
                        {
                            int y = 5 + 4;
                        }
                        x = 1 + 2 ^ (5-7);";

            var ast = new Parser(new Tokenizer(test)).Parse();

            Assert.IsTrue(ast.Children.Count == 3);
            Assert.IsTrue(ast.Children[0] is VarDeclrAst);
            Assert.IsTrue(ast.Children[1].Token.TokenType == TokenType.ScopeStart);
            Assert.IsTrue(ast.Children[2] is Expr);
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
                            fun() -> { 
                                zinger = ""your mom!"";
                                someThing();
                            }
                        }

                        z = 3;

                        int testFunction(){
                            var p = 23;
                        }";

            var ast = new Parser(new Tokenizer(test)).Parse();

        }

        [Test]
        public void FunctionInvokeTest()
        {
            var test = @"test(a, 1 + 2);";

            var ast = new Parser(new Tokenizer(test)).Parse();

        }
    }
}
