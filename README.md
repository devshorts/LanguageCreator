LanguageCreator
===============

A place to practice language creation mechanisms. Currently can execute a weird minimal language inspired by c# and f#. It properly contructs an AST, validates scoping rules, infer types, and properly uses encapsulated memory spaces.  

Supported Constructs
===

The language supports:

- variable assignment
- if/else conditionals declaration
- function declaration with variable arguments
- anonymous functions
- quoted strings
- simple type inference 
- basic classes (no inheritance)
- exception handling
- nil support
- partial function creation and application

Supported built in types are:

- int
- float
- string
- void
- bool (Booleans are `true` and `false`)

Operations are:

- +
- -
- & (and)
- || (or)
- /
- ^

Regarding type inference:
- Anonymous lambdas can take arguments and return values when stored in a type inferred "var" variable
- Regular functions can also be declared as "var" and type inferred from their return types
- You can enforce static typing rules by giving a function a proper type, otherwise it'll just use the return type
- If no return statement exists it'll default to void.  

Closures can reference their parent's memory space but not their callers memory space.  

If a class has a function called `init` then that is treated as the constructor and is called when a class is created. 

Class instantiation is scala style, it is just top down.  If a statement isn't enclosed in a function it will be executed before calling the `init` function.

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
```csharp
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
```csharp
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
```

```
age of  anton = 
29
----
age of  jane doe = 
99
----
```      

Example 10 basic closures
===

```csharp
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
```

```
101
test in second lambda
test in class bob pr1 from class human pr
102
test in second lambda
test in class bob pr1 from main
102
```

Example 11 (basic reference linking)
===

```csharp
int x = 1;

int y = &x;

print y;

y = 2;

print x;   

y = 3;

print x;             

x = 4;

print y;
```

```
1
2
3
4
```

Example 12 (complex reference linking passing via different memory scopes)
===

```csharp
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

print y;
```

```
101
test in second lambda
test in class bob pr1 from class human pr
102
test in second lambda
test in class bob pr1 from main
102
```

(note, this is supposed to have the same result as example 10 which has no links)

Example 13 - nil comparison
===

```csharp
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
```          

```
is nil
is not nil
is not nil
```
     
Example 14 - simple exception handling
===

```csharp
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
```

```
exception!
```

Notes
===
Type promotion doesn't exist and neither does inheritance. So you can't print a string and an int on the same line because the expression won't match properly, but thats intentional right now.
