LanguageCreator
===============

A place to practice language creation mechanisms. Currently can parse a weird subset of c++, c#, and f#, into an AST

Supported Constructs
===

The AST supports variable assignment, declaration, function declaration with varialbe arguments, anonymous functions, and quoted strings.  Things to do include AST for function calling, adding return types, and adding class constructors.

Though, I may just leave it simple and add the other stuff later. As an example, this properly parsers:

```csharp
[Test]
public void FunctionTest()
{
    var test = @"void foo(int x, int y){ 
                    int x = 1; 
                    fun() -> { 
                        zinger = ""your mom!"" 
                    }
                }

                z = 3;

                int testFunction(){
                    var p = 23;
                }";

    var ast = new Parser(new Tokenizer(test)).Parse();

}
```       