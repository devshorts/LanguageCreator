LanguageCreator
===============

A place to practice language creation mechanisms. Currently can parse a weird subset of c++, c#, and f#, into an AST with proper scoping rules and memory spaces.

Supported Constructs
===

The language supports variable assignment, if/else conditionals declaration, function declaration with varialbe arguments, anonymous functions, and quoted strings.  Things to do include AST for adding return types, and adding class constructs.

Right now, this properly parsers

```csharp
[Test]
public void FunctionTest()
{
    var test = @""void foo(int x, int y){ 
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
```      

And this interprets

```csharp
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

    var scopeBuilder = new ScopeBuilderVisitor();

    ast.Visit(scopeBuilder);

    var visitor = new InterpretorVisitor();

    ast.Visit(visitor);
}
```

Into 

```
1
103
```

So, progress is being made.  Type values aren't really being checked, everything is being worked as an integer.  Though updating the symbol tracking to be a little more robust and I can get that to work as well. 