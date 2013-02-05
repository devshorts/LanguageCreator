LanguageCreator
===============

A place to practice language creation mechanisms. Currently can parse a weird minimal language inspired by c# and f#. It can properly contruct an AST, validate scoping rules, infer types, and properly uses encapsulated memory spaces.  

Supported Constructs
===

The language supports variable assignment, if/else conditionals declaration, function declaration with variable arguments, anonymous functions, and quoted strings.  It also supports simple type inference. Things to do include and adding class or struct constructs. Supported built in types are int, float, string, void, and bool. Booleans are "true" and "false".  Operations are +, -, & (and), || (or), /, and ^.   Anonymous lambdas can take arguments and return values when stored in a type inferred "var" variable.  Regular functions can also be declared as "var" and type inferred from their return types. You can enforce static typing rules by giving a function a proper type, otherwise it'll just use the return type. If no return statement exists it'll default to void.

Here are a couple simple examples:

```csharp
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

This:
                
```csharp
var x = fun(int arg) -> {
    int g = arg;
    while(g > 0){
        print g;
        g = g - 1;
    }
    print "done!";
}

var y = x;

var z = y;

z(5);

print "lambda assigments work!";

z(3);

int a = 1;

int b = a;
                    
int c = b;

print c;
```

Executes to

```
5
4
3
2
1
done!
lambda assigments work!
3
2
1
done!
1
```

This

```csharp
var foo(string t){
    var x = "test";
    return x + t;
}

print foo("pong");
```

Executes to


```
testpong
```

So, progress is being made.  Type values are checked, but there is no type promotion yet.  