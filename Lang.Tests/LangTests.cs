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

            var tokens = new Lexers.Lexer(test).Lex().ToList();

            Assert.IsTrue(tokens.Count == 1);
            Assert.IsTrue(tokens.First().TokenType == TokenType.Float);
        }


        [Test]
        public void TestTokenizer1()
        {
            var test = @"fun function = 1 print";

            var tokens = new Lexers.Lexer(test).Lex().ToList();
        }

        [Test]
        public void TestTokenizer()
        {
            var test = @"function void int ""void int"" {} ->*/test^void,5,6,7 8.0";

            var tokens = new Lexers.Lexer(test).Lex().ToList();

            foreach (var token in tokens)
            {
                Console.WriteLine(token.TokenType + " - " + token.TokenValue);
            }
        }

        [Test]
        public void TestSimpleAst()
        {
            var test = @"x = 1;";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            var expr = (ast.ScopedStatements[0] as Expr);

            Assert.IsTrue(expr.Left.Token.TokenType == TokenType.Word);
            Assert.IsTrue(expr.Right.Token.TokenType == TokenType.Int);
            Assert.IsTrue(ast.Token.TokenType == TokenType.ScopeStart);
        }

        [Test]
        public void AstWithExpression()
        {
            var test = @"x = 1 + 2;";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            Assert.IsTrue(ast.ScopedStatements.Count == 3);
            Assert.IsTrue(ast.ScopedStatements[0] is MethodDeclr);
            Assert.IsTrue(ast.ScopedStatements[1] is Expr);
            Assert.IsTrue(ast.ScopedStatements[2] is MethodDeclr);
        }

        [Test]
        public void FunctionInvokeTest()
        {
            var test = @"test(a, 1 + 2);";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();
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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

            new PrintAstVisitor().Start(ast);

        }

        [Test]
        public void ForLoopTest()
        {
            var test = @"for(int i = 0; i < 10; i = i + 1){
                            var x = z;
                        }
                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse();

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestTypeResolutionUserDefined()
        {
            var test = @"
                        int x = 100 + 1;
                        T y;
                        var z = 1 > 2;";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            var scopeBuilder = new ScopeBuilderVisitor();

            scopeBuilder.Start(ast);

            var typeResolver = new ScopeBuilderVisitor(true);

            typeResolver.Start(ast);

            Assert.IsTrue(ast.ScopedStatements[0].AstSymbolType is BuiltInType);
            Assert.IsTrue(ast.ScopedStatements[1].AstSymbolType is UserDefinedType);
            Assert.IsTrue(ast.ScopedStatements[2].AstSymbolType is BuiltInType);
            Assert.IsTrue((ast.ScopedStatements[2].AstSymbolType as BuiltInType).ExpressionType == ExpressionTypes.Boolean);
        }

        [Test]
        public void TestWhileLoop()
        {
            var test = @"
                        int x = 5;
                        while(x > 0){
                            print x;
                            x = x - 1;
                        }
                        print ""done!"";";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestNestedExpressions()
        {
            var test = @"
                        var x = (1 + 2) + (2 + 3) + 4;
                        print x;";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestLambdaAssignments()
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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestAssingingVariableToVariable()
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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestFunctionInternalsBeingForwardReferences()
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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;


            new InterpretorVisitor().Start(ast);

        }

        [Test]
        [ExpectedException(typeof(UndefinedElementException))]
        public void InvalidForwardReferences()
        {
            var test = @"
                         int x = y;
                         int y = 0;
                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestCallingFunctionAsForwardReferences()
        {
            var test = @"
                         string item = func(""test"");

                         var func(string printer){
                            return ""yes"";
                         }

                         print item;
                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;


            new InterpretorVisitor().Start(ast);
        }

        [Test]
        [ExpectedException(typeof(UndefinedElementException))]
        public void TestForwardReferences3()
        {
            var test = @"
                         print item;
                         string item = func(""test"");

                         var func(string printer){
                            return ""yes"";
                         }                         
                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestCurrying()
        {
            var test = @"
                        void func(string printer, int y){
                            print printer;
                            print y;
                        }
            
                        var curry = func('anton');

                        int x = 1;
                        curry(x);

                        curry(2);

                        var otherCurry = func('test');

                        otherCurry(3);
                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

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

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);

        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void TestInvalidTypeAssignment()
        {
            var test = @"
                        int x = 1.0;
                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);

        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void TestInvalidAssignment2()
        {
            var test = @"
                        int x = func();

                        string func(){
                            return ""test"";
                        }
                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestArgumentInferWithLambdasAsMethodPassing()
        {
            var test = @"
                        var func(method printer, method printer2){
                            print printer();
                            printer2();
                        }
           
                        var x = fun() -> { print 'test'; return 1; };
                        func(x,x);

                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestArgumentInferAndTypeCheckWithLambdasAsMethods()
        {
            var test = @"
                        var func(method printer, method printer2){
                            print printer();
                            printer2();
                        }
           
                        var x = fun() -> { print 'x'; return 1; };
                        var two = fun() -> { print 'two function'; };
                        var three = fun() -> { print 'three function'; };
                        var z = func(x);
                        z(two);
                        z(three);

                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        [ExpectedException(typeof(InvalidSyntax))]
        public void TestArgumentInferInvalid()
        {
            var test = @"
                        var func(method printer, method printer1){
                            printer(1);
                            printer1();
                        }
           
                        var x = fun() -> { print 'test'; };

                        var curry = func(x);

                        curry(x);

                        ";

            var ast = new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr;

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestClassParsing()
        {
            var test = @"
                        class anton{
                            int x;
                            int y;
                        }

                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr).ScopedStatements.FirstOrDefault() as ClassAst;

            Assert.IsTrue(ast != null);

            Assert.IsTrue(ast.Body.ScopedStatements.Count == 2);
        }

        [Test]
        public void TestClassParsing2()
        {
            var test = @"
                        class anton{
                            int x = 1;
                            int y = 2;

                            void foo(){}
                        }

                        var ant = new anton();
                        var foo = new anton();
    
                        foo.x = 2;

                        print ant.x;

                        print foo.x;

                        ant.x = foo.x;

                        print ant.x;                
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestClassParsing4()
        {
            var test = @"
                        class anton{
                            int x = 1;
                            int y = 2;

                            void foo(){
                                print x;
                            }
             
                        }

                        var ant = new anton();
                        var foo = new anton();
    
                        foo.x = 2;

                        ant.foo();                

                        foo.foo();

                        foo.x = 10;

                        foo.foo();
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestClassParsing5()
        {
            var test = @"
                        class anton{
                            int x = 1;
                            int y = 2;

                            int foo(){
                                return x;
                            }
             
                        }

                        var ant = new anton();
                        var foo = new anton();
    
                        foo.x = 2;

                        print ant.foo();                

                        print foo.foo();

                        foo.x = 10;

                        print foo.foo();
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        [ExpectedException(typeof(UndefinedElementException))]
        public void TestClassParsing3()
        {
            var test = @"
                        class anton{
                            int x = 1;
                            int y = 2;
                        }

                        anton foo = new anton();

                        print foo.p + 2;
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestClassParsing6()
        {
            var test = @"
                class bob{
                    var z = fun() -> { return 'in bob';} ;
                    
                }

                class anton{
                    var x = new bob();
                    int y = 0;
                }

                anton foo = new anton();

                print foo.x.z();
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestClassParsing7()
        {
            var test = @"
                class bob{
                    var z = 1;
                    var printer = fun() -> { return z; };
                }

                class anton{
                    var x = new bob();
                    int y = 0;
                }

                anton foo = new anton();

                print foo.x.z;

                foo.x.z = 2;

                print foo.x.z;

                var foo2 = new anton();

                print foo2.x.printer();
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestClassParsing8()
        {
            var test = @"
               
                class human{
                    void init(string id){
                        age = 99;
                        name = id;
                    }

                    void create(){
                        person = new human('test');
                    }

                    int age;
                    string name;

                    human person;
                }

                var person = new human('anton');

                void printPerson(human person){
                    print 'age of  ' + person.name + ' = ';
                    print person.age;
                    print '----';
                }

                person.age = 29;
                person.create();            

                printPerson(person);

                printPerson(person.person); 
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestMethodPassing()
        {
            var test = @"
               
class human{
    void init(string id){
        age = 1;
                        
        name = id;
    }

    void create(){
        person = new human('test');
    }

    void methodProxy(method nameAcceptor){
        nameAcceptor(name);
    }

    int age = 99;
    string name = 'jane doe';
    human person;
}

var person = new human('anton');

var proxyCopy = fun(string i) -> { print i; };

person.methodProxy(proxyCopy);

person.methodProxy(fun(string i) -> { print i + 'proxy'; });
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestMethodSpaceInvocationClasses()
        {
            var test = @"
               
                class bob{
                    int y = 1;

                    void pr1(method x){

                        var a = new human();
                        a.y = 100;

                        print a.y;
                        print x();   
                    }
                }

                class human{
                    int y = 0;
                    
                    var b = new bob();

                    void pr(method z){ 
                                                                    
                        b.pr1(z);
                    }
                }

                var a = new human();
                var b = new bob();

                b.y = 69;

                var lambda = fun() ->{
                                 return b.y;
                             };

                a.pr(lambda);

                b.y = 20;

                b.pr1(lambda);
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestMethodSpaceInvocationClasses2()
        {
            var test = @"
               
                class bob{
                    int x = 0;
                    string pr1(method x){
                        return x('test') + ' in class bob pr1';   
                    }
                }

                class human{
                    int x = 1;
                    
                    var b = new bob();

                    void pr(method z){                                                                     
                        print b.pr1(z) + ' from class human pr';
                    }
                }

                var a = new human();
                var b = new bob();

                int x = 100;
                var lambda = fun(string v) ->{
                                 var p = fun() -> { 
                                                x = x + 1;
                                                print x;
                                                print v + ' in second lambda'; 
                                            };
                                 p();
                                 return v;      
                             };

                a.pr(lambda);

                print b.pr1(lambda) + ' from main';

                print x;
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestAssigningClasses()
        {
            var test = @"
               
                class bob{
                    int x = 44;
                }

                class human{
                    bob x;
                }

                var a = new human();

                a.x = new bob();

                print a.x.x;
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestBasicLinks()
        {
            var test = @"
               
                int x = 1;

                int y = &x;

                print y;

                y = 2;

                print x;   

                y = 3;

                print x;             

                x = 4;

                print y;
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestMethodSpaceInvocationClasses_WithLinks()
        {
            var test = @"
               
                class bob{
                    int x = 0;
                    int y = 0;
                    string pr1(method x){
                        y = y + 1;
                        print y;
                        return x('test') + ' in class bob pr1';   
                    }
                }

                class human{
                    int x = 1;
                    
                    var b = new bob();

                    void pr(method z){      
                        x = x + 1;
                        print x;                                                                                       
                        print b.pr1(z) + ' from class human pr';
                    }
                }

                var a = new human();
                var b = new bob();
                var c = new bob();

                int y = 100;
                int f = &y;
                int x = &f;
                

                var lambda = fun(string v) ->{
                                 var p = fun() -> { 
                                                x = x + 1;
                                                print x;
                                                print v + ' in second lambda'; 
                                            };
                                 p();
                                 return v;      
                             };

                a.pr(lambda);

                print b.pr1(lambda) + ' from main';
                print c.pr1(lambda) + ' from main2';
                print c.pr1(lambda) + ' from main3';
                print b.y;

                print y;
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }



        [Test]
        public void TestNilClasses()
        {
            var test = @"
               
                void printNull(var d){
                    try{
                        if((d == nil) || (d.z == nil)){
                            print 'is nil';
                        }   
                    }
                    catch{
                        print 'an exception occurred, something was probably nil';
                    }
                }

                class test{
                    int z;
                }

                test item = new test();
                
                printNull(item);                
           ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestNil()
        {
            var test = @"
               
                void printNull(int item){
                    if(item == nil){
                        print 'is nil';
                    }
                    else {
                        print 'is not nil';
                    }
                }

                int x;
                
                int y = 1;
                
                printNull(x);
                printNull(y);
    
                x = 2;

                printNull(x);
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestNotNil()
        {
            var test = @"
               
                void printNull(int item){
                    if(item != nil){
                        print 'is not nil';
                    }
                    else {
                        print 'is nil';
                    }
                }

                int x;
                
                int y = 1;
                
                printNull(x);
                printNull(y);
    
                x = 2;

                printNull(x);
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestTryCatch()
        {
            var test = @"
               
                class test{
                    int x;
                }

                test item;

                try{
                    print item.x;
                }
                catch{
                    print 'exception!';
                }
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }

        [Test]
        public void TestRightRecursion()
        {
            var test = @"
               
                var x = 1 + 2;
                var y = 1 + 2 + 3;
                var z = (1 + 2) + 3;
                var a = (1 + 2 ) + (3 + 4);
                var b = 1 + (2 + 3);
                var c = 1 + (2 + 3) + 4;
                var d = (1 + 2 + 3 + 4);
                        ";

            var ast = (new LanguageParser(new Lexers.Lexer(test)).Parse() as ScopeDeclr);

            new InterpretorVisitor().Start(ast);
        }
    }
}
