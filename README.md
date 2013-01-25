LanguageCreator
===============

A place to practice language creation mechanisms. Currently can parse a weird subset of c++, c#, and f#, into an AST

Supported Constructs
===

The AST supports variable assignment, if/else conditionals declaration, function declaration with varialbe arguments, anonymous functions, and quoted strings.  Things to do include AST for adding return types, and adding class constructs.

Though, I may just leave it simple and add the other stuff later. As an example, this properly parsers:

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