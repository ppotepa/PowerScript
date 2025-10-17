# PowerScript

so this is powerscript, basically a wrapper around .NET that lets you write scripts without all the ceremony. its got a nice interactive shell (repl) and can also run script files. think of it like if powershell and python had a baby but simpler.

## whats inside

this thing compiles to .NET 8.0 and has a bunch of cool stuff:
- interactive shell where you can just type stuff and see what happens
- script execution from files
- direct access to .NET framework (super useful)
- functions that compile to actual lambda expressions
- a standard library with common stuff you need
- proper type system with both static and dynamic types

## getting started

build it:
```bash
dotnet build
```

run the shell:
```bash
.\src\PowerScript.CLI\bin\Debug\net8.0\PowerScript.CLI.exe
```

or run a script file:
```bash
.\src\PowerScript.CLI\bin\Debug\net8.0\PowerScript.CLI.exe yourscript.ps
```

## language features

### variables and types

powerscript has a bunch of different types depending on what you need:

**FLEX** - for when you want full dynamic typing, can change types on the fly
```powerscript
FLEX x = 10
PRINT x          // prints: 10

FLEX x = "hello" 
PRINT x          // prints: HELLO
```

**VAR** - figures out the type from the initial value, then sticks with it
```powerscript
VAR count = 5
VAR message = "test"
count = 10       // ok
count = "oops"   // nope, cant do that
```

**INT** - classic integers
```powerscript
INT age = 25
INT score = 100
```

**STRING** - text stuff
```powerscript
STRING name = "Alice"
STRING greeting = "Hello World"
```

**NUMBER** - flexible numeric type (can be int or decimal)
```powerscript
NUMBER value = 42
NUMBER price = 99.99
```

**PREC** - when you specifically need floating point
```powerscript
PREC pi = 3.14159
PREC temperature = 98.6
```

### bit-width types (yeah we got those!)

sometimes you need to be specific about memory size:
```powerscript
INT[8] smallNumber = 255      // 8-bit int
INT[16] mediumNumber = 30000  // 16-bit int
INT[32] bigNumber = 2000000   // 32-bit int
NUMBER[64] hugeValue = 9999   // 64-bit number
```

### arrays

arrays work pretty much like youd expect:
```powerscript
VAR numbers = [1, 2, 3, 4, 5]
PRINT numbers

VAR first = numbers[0]
PRINT first              // prints: 1

VAR names = ["Alice", "Bob", "Charlie"]
PRINT names[1]          // prints: BOB
```

### printing stuff

just use PRINT, it works:
```powerscript
PRINT "Hello, World!"
PRINT 42
PRINT myVariable
```

### arithmetic

all the usual suspects work:
```powerscript
VAR sum = 5 + 3          // 8
VAR diff = 10 - 4        // 6
VAR product = 6 * 7      // 42
VAR quotient = 20 / 4    // 5

// use parantheses for precedence
VAR result = (2 + 3) * 4  // 20
```

### comparison operators

```powerscript
x > 5
y < 10
age >= 18
count <= 100
value == 42
name != "test"
```

### logical operators

```powerscript
IF (x > 5 AND y < 10) {
    PRINT "both conditions true"
}

IF (age < 18 OR hasPermission == 1) {
    PRINT "at least one is true"
}
```

### conditionals

IF statements work as expected:
```powerscript
IF (age >= 18) {
    PRINT "Adult"
} ELSE {
    PRINT "Minor"
}

// nested ifs work too
IF (score >= 90) {
    PRINT "A grade"
} ELSE {
    IF (score >= 80) {
        PRINT "B grade"
    } ELSE {
        PRINT "Keep trying"
    }
}
```

### loops (we call them CYCLE)

powerscript uses CYCLE for loops, and its got a bunch of different flavours:

**basic counting loop:**
```powerscript
CYCLE 5 {
    PRINT "hello"
}
```

**loop with counter variable:**
```powerscript
CYCLE 5 AS i {
    PRINT i      // prints: 0, 1, 2, 3, 4
}
```

**conditional loop:**
```powerscript
VAR x = 0
CYCLE WHILE (x < 5) {
    PRINT x
    x = x + 1
}
```

**loop over arrays:**
```powerscript
VAR numbers = [10, 20, 30]
CYCLE numbers AS num {
    PRINT num
}
```

**explicit collection iteration:**
```powerscript
VAR items = ["apple", "banana", "orange"]
CYCLE ELEMENTS OF items AS item {
    PRINT item
}
```

**range loops:**
```powerscript
// using array literal syntax
CYCLE [1, 10] AS i {
    PRINT i      // 1 to 10
}

// or explicit range syntax
CYCLE RANGE FROM 1 TO 10 AS i {
    PRINT i
}
```

**you can even nest them:**
```powerscript
CYCLE 3 AS i {
    CYCLE 3 AS j {
        PRINT i
        PRINT j
    }
}
```

### functions

define functions with the FUNCTION keyword:
```powerscript
FUNCTION ADD(INT a, INT b)[INT] {
    RETURN a + b
}

VAR result = ADD(5, 3)
PRINT result        // prints: 8
```

**functions can have different return types:**
```powerscript
FUNCTION GET_NAME()[STRING] {
    RETURN "Alice"
}

FUNCTION IS_VALID(INT x)[INT] {
    IF (x > 0) {
        RETURN 1
    }
    RETURN 0
}
```

### .NET interop (this is the cool part)

you can call .NET methods directly using the # prefix and -> operator:

**calling static methods:**
```powerscript
#Console -> WriteLine("Hello from .NET!")
#String -> Concat("Hello", " World")
```

**accessing properties and methods on variables:**
```powerscript
STRING text = "hello world"
VAR length = #text -> Length
PRINT length        // prints: 11

VAR upper = #text -> ToUpper()
PRINT upper        // prints: HELLO WORLD
```

**using .NET types:**
```powerscript
VAR now = #DateTime -> Now
VAR formatted = #now -> ToString()
PRINT formatted
```

**linking .NET namespaces:**
```powerscript
LINK System
LINK System.IO

// now you can use types from those namespaces
```

### importing other powerscript files

use LINK to import other powerscript files:
```powerscript
LINK "Core.ps"
LINK "String.ps"
LINK "Math.ps"

// now you can use functions from those files
VAR sum = ADD(5, 3)
VAR text = STR_UPPER("hello")
```

### standard library

the stdlib has a bunch of useful functions already:

**Math operations:**
```powerscript
VAR sum = ADD(5, 3)
VAR diff = SUB(10, 4)
VAR product = MUL(6, 7)
VAR quotient = DIV(20, 4)
VAR remainder = MOD(10, 3)
VAR power = POW(2, 8)
```

**String operations:**
```powerscript
VAR len = STR_LENGTH("hello")
VAR upper = STR_UPPER("hello")
VAR lower = STR_LOWER("HELLO")
VAR reversed = STR_REVERSE("hello")
VAR trimmed = STR_TRIM("  hello  ")
VAR contains = STR_CONTAINS("hello world", "world")
VAR concat = STR_CONCAT("hello", " ", "world")
```

**I/O functions:**
```powerscript
OUT("text")              // output without newline
NEWLINE()                // print newline
OUT_MULTI(val1, val2)    // output multiple values
```

### comments

just use // for comments:
```powerscript
// this is a comment
VAR x = 10  // comments can go at the end of lines too
```

## examples

### fibonacci sequence
```powerscript
FUNCTION FIB(INT n)[INT] {
    IF (n <= 1) {
        RETURN n
    }
    VAR prev = FIB(SUB(n, 1))
    VAR prevprev = FIB(SUB(n, 2))
    RETURN ADD(prev, prevprev)
}

PRINT FIB(10)  // prints: 55
```

### bubble sort
```powerscript
FUNCTION BUBBLE_SORT(VAR arr)[VAR] {
    VAR len = #arr -> Length
    CYCLE RANGE FROM 0 TO SUB(len, 1) AS i {
        CYCLE RANGE FROM 0 TO SUB(SUB(len, i), 2) AS j {
            VAR current = arr[j]
            VAR next = arr[ADD(j, 1)]
            IF (current > next) {
                arr[j] = next
                arr[ADD(j, 1)] = current
            }
        }
    }
    RETURN arr
}

VAR numbers = [64, 34, 25, 12, 22, 11, 90]
numbers = BUBBLE_SORT(numbers)
```

### prime number checker
```powerscript
FUNCTION IS_PRIME(INT n)[INT] {
    IF (n <= 1) {
        RETURN 0
    }
    IF (n == 2) {
        RETURN 1
    }
    
    VAR i = 2
    CYCLE WHILE (i * i <= n) {
        IF (MOD(n, i) == 0) {
            RETURN 0
        }
        i = ADD(i, 1)
    }
    RETURN 1
}

PRINT IS_PRIME(17)  // prints: 1 (true)
PRINT IS_PRIME(20)  // prints: 0 (false)
```

## test results

as of now, heres where we stand:
- **211 out of 213 tests passing (99.1%)**
- StandardLibrary: 61/61 (100%)
- TuringCompleteness: 47/47 (100%)
- Language: 103/105 (98.1%)

the 2 failing tests are just feature gaps (FOR loops not implemented, and decimal parsing needs work).

## project structure

```
tokenez/
├── src/
│   ├── PowerScript.CLI/          # command line interface
│   ├── PowerScript.Common/       # shared stuff
│   ├── PowerScript.Compiler/     # compiles tokens to AST
│   ├── PowerScript.Core/         # core types and tokens
│   ├── PowerScript.Interpreter/  # runs the code
│   ├── PowerScript.Parser/       # parses tokens
│   └── PowerScript.Runtime/      # executes the AST
├── stdlib/                       # standard library functions
├── test-scripts/                 # example scripts
│   ├── simple/                   # basic examples
│   ├── moderate/                 # medium complexity
│   ├── complex/                  # advanced algorithms
│   └── language/                 # language feature tests
└── tests/                        # unit tests
```

## shell commands

when you run the interactive shell, you get some built-in commands:

```
HELP      - shows help message
EXIT      - quits the shell (QUIT works too)
CLEAR     - clears the screen (CLS works too)
HISTORY   - shows command history
VERSION   - shows version info
ABOUT     - about powerscript
```

## architecture notes

the interpreter works like this:
1. lexical analysis turns source code into tokens
2. tokens get organized into a tree structure
3. processors walk the token tree and build an AST
4. the runtime executes the AST
5. functions get compiled to actual .NET lambda expressions for speed

its all built on reflection so you can call pretty much any .NET method you want.

## quirks and gotchas

- all string output gets converted to uppercase (its a feature not a bug)
- variable names are case-insensitive 
- you need the # prefix before identifiers when calling .NET methods
- CYCLE is our loop keyword, not FOR or WHILE (though CYCLE WHILE exists)
- arrays are 0-indexed like civilized people expect

## contributing

if you want to add stuff:
1. add your feature to the appropriate processor
2. write tests in the tests/ folder
3. update this readme with examples
4. make sure all tests still pass

## license

do whatever you want with it, im not your boss.

## known issues

- FOR loops arent implemented (use CYCLE instead)
- decimal number parsing needs some work (3.14 gets split weirdly)
- some nullable warnings in the code (they dont affect functionality)

## final thoughts

this started as a learning project and turned into something actually useable. its not meant to replace C# or anything, but for quick scripts and .NET automation its pretty handy. the REPL is nice for experimenting and the .NET interop means you get the entire framework at your fingertips.

enjoy!
