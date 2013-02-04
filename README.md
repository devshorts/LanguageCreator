LanguageCreator
===============

A place to practice language creation mechanisms. Currently can parse a weird subset of c++, c#, and f#, into an AST with proper scoping rules and memory spaces.

Supported Constructs
===

The language supports variable assignment, if/else conditionals declaration, function declaration with varialbe arguments, anonymous functions, and quoted strings.  It also supports simple type inference. Things to do include AST for adding return types, and adding class constructs.

Right now, this properly parsers

```csharp
void foo(int x, int y){ 
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
}
```      

And this interprets

```csharp
[Test]
void foo(int x){
    if(x > 2){
        print ((x + 1) + 2);
    }
    else{
        print (x);
    }
}
foo(1);
foo(100);
```

Into 

```
1
103
```

And so does this
                
```
var x = fun() -> {
    int g = 5;
    while(g > 0){
        print g;
        g = g - 1;
    }
    print ""done!"";
}
x();

print "lambda assigments work!";

x();
```

Into 

```
5
4
3
2
1
done!
lambda assigments work!
5
4
3
2
1
done!
```

So, progress is being made.  Type values are checked, but there is no type promotion yet.  