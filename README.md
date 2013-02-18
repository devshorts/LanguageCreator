LanguageCreator
===============

A place to practice language creation mechanisms. Currently can execute a weird minimal language inspired by c# and f#. It properly contructs an AST, validates scoping rules, infer types, and properly uses encapsulated memory spaces.  

Supported Constructs
===

The language supports variable assignment, if/else conditionals declaration, function declaration with variable arguments, anonymous functions, and quoted strings.  It also supports simple type inference and basic classes. Supported built in types are int, float, string, void, and bool. Booleans are "true" and "false".  Operations are +, -, & (and), || (or), /, and ^.   Anonymous lambdas can take arguments and return values when stored in a type inferred "var" variable.  Regular functions can also be declared as "var" and type inferred from their return types. You can enforce static typing rules by giving a function a proper type, otherwise it'll just use the return type. If no return statement exists it'll default to void.  Partial functions are also supported.

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

Example 5 (classes)
===

```csharp
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
```

```
1
2
10
```                                                                          

Example 6
===
```csharp
class bob{
    var z = 1;
}

class anton{
    var x = new bob();
    int y = 0;
}

anton foo = new anton();

print foo.x.z;
```

```
1
```

Example 7
===
```csharp
class bob{
    var z = 1;
                    
}

class anton{
    var x = new bob();
    int y = 0;
}

anton foo = new anton();

print foo.x.z;

foo.x.z = 2;

print foo.x.z;
```

```
1
2
```

Example 8
===
```
class anton{
    var x = fun() -> { return new anton(); };
    int y = 10;
}

var x = new anton();

var dynamicAnton = x.x();

dynamicAnton.y = 52;

print dynamicAnton.y;
```

```
52
```

Example 9 (forward referencing and object passing)
===
```
class human{
    void init(string id){
        age = 1;
        person = new human();
        name = id;
    }

    int age = 99;
    string name = 'jane doe';
    human person;
}

var person = new human();
person.init('anton');

void printPerson(human person){
    print 'age of  ' + person.name + ' = ';
    print person.age;
    print '----';
}

person.age = 29;

printPerson(person);

printPerson(person.person);
```

```
age of  anton = 
29
----
age of  jane doe = 
99
----
```

Notes
===
Type promotion doesn't exist and neither does inheritance. So you can't print a string and an int on the same line because the expression won't match properly, but thats intentional right now.