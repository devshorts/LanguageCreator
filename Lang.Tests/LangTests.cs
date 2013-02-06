using System;
using System.Linq;
using Lang.AST;
using Lang.Data;
using Lang.Exceptions;
using Lang.Parser;
using Lang.Symbols;
using Lang.Visitors;
using NUnit.Framework;

namespace Lang.Tests
{
    [TestFixture]
    public class LangTests
    {
        
        [Test]
        public void TestFloatTokenizer()
        {
            var test = @"1.01";

            var tokens = new Tokenizer(test).Tokenize().ToList();

            Assert.IsTrue(tokens.Count == 1);
            Assert.IsTrue(tokens.First().TokenType == TokenType.Float);
        }


        [Test]
        public void TestTokenizer1()
        {
            var test = @"fun function = 1 print";

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
            Assert.IsTrue(expr.Right.Token.TokenType == TokenType.Int);
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
        public void AstWithNestedExpressions()
        {
            var test = @"(3 + ((1 + 2) + 1));";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            var expr = (ast.ScopedStatements[0] as Expr);

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
                                return 1;
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

            new PrintAstVisitor().Start(ast);

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
            Assert.IsTrue(forLoop.Setup is VarDeclrAst);
            Assert.IsTrue(forLoop.Predicate.Token.TokenType == TokenType.LessThan);
            Assert.IsTrue(forLoop.Body.ScopedStatements.Count == 1);
        }

        [Test]
        public void TestScope()
        {
            var test = @"
                        void foo(int p){
                            p = 1;    
                        }
                        int z = 5;
                        while(z > 0){
                            z = z + 1;
                            foo();
                        }
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            var visitor = new ScopeBuilderVisitor();

            visitor.Start(ast);
        }

        [Test]
        public void TestExpressionInterpreter()
        {
            var test = @"
                        int x = 100 + 1;
                        void foo(){
                            print (1 + 1);
                        }
                        print (x + 2 + (3 + 4));
                        foo();
                        foo();";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestExpressionInterpreterFunctionArguments()
        {
            var test = @"
                        void foo(int x){
                            if(x > 2){
                                print ((x + 1) + 2);
                            }
                            else{
                                print (x);
                            }
                        }

                        foo(1);
                        foo(100);";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        [ExpectedException(typeof(UndefinedElementException))]
        public void TestExpressionInterpreterUndeclaredVar()
        {
            var test = @"
                        int x = 100 + 1;
                        z = 4;
                        print (x + 2 + (3 + 4));";

            var ast = new LanguageParser(new Tokenizer(test)).Parse();

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestScopeTypes()
        {
            var test = @"
                        int x = 100 + 1;
                        T y;
                        var z = 1 > 2;";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            var scopeBuilder = new ScopeBuilderVisitor();
            
            scopeBuilder.Start(ast);

            Assert.IsTrue(ast.ScopedStatements[0].AstSymbolType is BuiltInType);
            Assert.IsTrue(ast.ScopedStatements[1].AstSymbolType is UserDefinedType);
            Assert.IsTrue(ast.ScopedStatements[2].AstSymbolType is BuiltInType);
            Assert.IsTrue((ast.ScopedStatements[2].AstSymbolType as BuiltInType).ExpressionType == ExpressionTypes.Boolean);
        }

        [Test]
        public void TestScopeTypes2()
        {
            var test = @"
                        int x = 5;
                        while(x > 0){
                            print x;
                            x = x - 1;
                        }
                        print ""done!"";";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestScopeTypes4()
        {
            var test = @"
                        var x = (1 + 2) + (2 + 3) + 4;
                        print x;";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestScopeTypes3()
        {
            var test = @"
            
                int arg = 5;

                var x = fun(int arg) -> {
                    int g = arg;
                    while(g > 0){
                        print g;
                        g = g - 1;
                    }
                    print ""done!"";
                }

                var y = x;

                var z = y;

                z(5);

                print ""lambda assignments work!"";

                z(3);

                int a = 1;

                int b = a;
                    
                int c = b;

                print c;

                print arg;";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestBools()
        {
            var test = @"
                        var x = true & false || true;
                        while(x){
                            print x;
                            x = 1 > 2;
                        }
                        print x;";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestReturn()
        {
            var test = @"
                         var x = 10;
                         var z = fun () -> {
                                            while(true){
                                                if(x < 1){
                                                    return 2;
                                                }
                                                x = x - 1;
                                            }
                                        }
                        
                        print z();
                        print x;
                        
                        int foo = z();
                        print foo;";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestReturnTypes()
        {
            var test = @"
                         var foo(string t){
                                var x = ""test"";
                                return x + t;
                         }

                        print foo(""pong"");";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void TestInvalidReturnTypes()
        {
            var test = @"
                         int foo(string t){
                                var x = ""test"";
                                return x + t;
                         }

                        print foo(""pong"");";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestForLoop()
        {
            var test = @"
                         for(int i = 0;i < 10; i = i + 1){
                            i = 15;
                            print i;                            
                         }";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestReferences()
        {
            var test = @"
                         var x = true;

                         var y = x;
        
                         print y;

                         print x;

                         y = false;

                         print y;

                         print x;
";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestForwardReferences()
        {
            var test = @"
                         var func(string printer){
                            return foo(printer);
                         }

                         var foo(string printer){
                            return printer + ""foo"";
                         }
                        
                        print func(""zing"");
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            
            new InterpretorVisitor().Start(ast);

        }

        [Test]
        public void TestForwardReferences2()
        {
            var test = @"
                         string item = func(""test"");

                         var func(string printer){
                            return ""yes"";
                         }

                         print item;
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;


            new InterpretorVisitor().Start(ast);
        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void TestForwardReferences3()
        {
            var test = @"
                         print item;
                         string item = func(""test"");

                         var func(string printer){
                            return ""yes"";
                         }

                         
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;


            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestCurrying()
        {
            var test = @"
                        var func(string printer, int x){
                            print printer;
                            print x;
                        }
            
                        var curry = func(""anton"");

                        curry(1);

                        curry(2);

                        var otherCurry = func(""test"");

                        otherCurry(3);
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);

        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void TestCurryingInvalidArguments()
        {
            var test = @"
                        var func(string printer, int x){
                            print printer;
                            print x;
                        }
            
                        var curry = func(1);

                        curry(1);

                        curry(2);

                        var otherCurry = func(""test"");

                        otherCurry(3);
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);

        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void TestFunctionInvalidParamters()
        {
            var test = @"
                        var func(string printer, int x){
                            print printer;
                            print x;
                        }
            
                        func(""asdf"", ""asdf"",""asdf"");
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);

        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void TestFunctionInvalidTypeParamters()
        {
            var test = @"
                        var func(string printer, int x){
                            print printer;
                            print x;
                        }
            
                        func(""asdf"", ""asdf"");
                        ";

            var ast = new LanguageParser(new Tokenizer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);

        }
    }
}
