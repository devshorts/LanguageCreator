using System;
using System.Linq;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;
using Lang.Parser;
using Lang.Visitors;
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

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            var expr = (ast.ScopedStatements[0] as Expr);

            Assert.IsTrue(expr.Left.Token.TokenType == TokenType.Word);
            Assert.IsTrue(expr.Right.Token.TokenType == TokenType.Word);
            Assert.IsTrue(ast.Token.TokenType == TokenType.ScopeStart);
        }

        [Test]
        public void AstWithExpression()
        {
            var test = @"x = 1 + 2;";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

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

                            if(foo){
                                var x = 1;
                            }
                            else if(faa){
                                var y = 2;
                                var z = 3;
                            }
                            else{
                                while(1 + 1){
                                    var x = fun () ->{
                                        test = 0;
                                    };
                                }

                                if(foo){
                                    var x = 1;
                                }
                                else if(faa){
                                    var y = 2;
                                    var z = 3;
                                }
                                else{
                                    for(int i = 0; i < 10; i = i + 1){
                                        var x = z;
                                    }
                                }
                            }
                        }";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            Assert.IsTrue(ast.ScopedStatements.Count == 3);
            Assert.IsTrue(ast.ScopedStatements[0] is MethodDeclr);
            Assert.IsTrue(ast.ScopedStatements[1] is Expr);
            Assert.IsTrue(ast.ScopedStatements[2] is MethodDeclr);
        }

        [Test]
        public void FunctionInvokeTest()
        {
            var test = @"test(a, 1 + 2);";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

        }

        [Test]
        public void ConditionalTest()
        {
            var test = @"if(foo){
                            var x = 1;
                        }
                        else if(faa){
                            var y = 2;
                            var z = 3;
                        }
                        else{
                        }

                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            var topScope = (ast as ScopeDeclr).ScopedStatements[0];

            var conditional = topScope as Conditional;
            Assert.IsTrue(conditional != null);
            Assert.IsTrue(conditional.Alternate != null);
            Assert.IsTrue(conditional.Predicate.Token.TokenValue == "foo");
            Assert.IsTrue(conditional.Alternate.Body.ScopedStatements.Count == 2);
            Assert.IsTrue(conditional.Alternate.Alternate != null);
        }

        [Test]
        public void WhileTest()
        {
            var test = @"while(1 + 1){
                            var x = fun () ->{
                                test = 0;
                            };
                        }
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            var topScope = (ast as ScopeDeclr).ScopedStatements[0];

            var conditional = topScope as WhileLoop;
            Assert.IsTrue(conditional != null);
            Assert.IsTrue(conditional.Body != null);
            Assert.IsTrue(conditional.Predicate.Token.TokenType == TokenType.Plus);
            Assert.IsTrue(conditional.Body.ScopedStatements.Count == 1);
            Assert.IsTrue(conditional.Body.ScopedStatements.First() is VarDeclrAst);
            Assert.IsTrue((conditional.Body.ScopedStatements.First() as VarDeclrAst).VariableValue is MethodDeclr);
        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void InvalidConditionalTest()
        {
            var test = @"else(foo){
                            var x = 1;
                        }";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();
        }

        [Test]
        public void TestVisitor()
        {
            var test = @"while(1 + 1){
                            var x = fun () ->{
                                test = 0;
                            };
                        }

                        if(foo){
                            var x = 1;
                        }
                        else if(faa){
                            var y = 2;
                            var z = 3;
                        }
                        else{
                            for(int i = 0; i < 10; i = i + 1){
                                var x = z;
                            }
                        }


                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            var visitor = new PrintAstVisitor();

            ast.Visit(visitor);
        }

        [Test]
        public void ForLoopTest()
        {
            var test = @"for(int i = 0; i < 10; i = i + 1){
                            var x = z;
                        }
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            var topScope = (ast as ScopeDeclr).ScopedStatements[0];

            var forLoop = topScope as ForLoop;
            Assert.IsTrue(forLoop != null);
            Assert.IsTrue(forLoop.Initial is VarDeclrAst);
            Assert.IsTrue(forLoop.Stop.Token.TokenType == TokenType.LessThan);
            Assert.IsTrue(forLoop.Body.ScopedStatements.Count == 1);
        }

        [Test]
        public void TestScope()
        {
            var test = @"while(1){
                            var x = 2;
                            int y;
                        }
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            var visitor = new ScopeBuilderVisitor();

            ast.Visit(visitor);

            var scope = visitor.Current;
        }
    }
}
