LanguageCreator
===============

A place to practice language creation mechanisms. Currently can execute a weird minimal language inspired by c# and f#. It properly contructs an AST, validates scoping rules, infer types, and properly uses encapsulated memory spaces.  

Supported Constructs
===

The language supports variable assignment, if/else conditionals declaration, function declaration with variable arguments, anonymous functions, and quoted strings.  It also supports simple type inference. Things to do include and adding class or struct constructs. Supported built in types are int, float, string, void, and bool. Booleans are "true" and "false".  Operations are +, -, & (and), || (or), /, and ^.   Anonymous lambdas can take arguments and return values when stored in a type inferred "var" variable.  Regular functions can also be declared as "var" and type inferred from their return types. You can enforce static typing rules by giving a function a proper type, otherwise it'll just use the return type. If no return statement exists it'll default to void.  Partial functions are also supported.

Example1
====

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


```
1
103
```

Example2
====           
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

Example3
====

```csharp
var foo(string t){
    var x = "test";
    return x + t;
}

print foo("pong");
```



```
testpong
```

Example4 (currying)
===

```csharp
var func(string printer, int x){
    print printer;
    print x;
}
            
var curry = func("anton");

curry(1);

curry(2);

var otherCurry = func("test");

otherCurry(3);
```

```
anton
1
anton
2
test
3
```

Notes
===
So, progress is being made.  Type values are checked, but there is no type promotion yet.  