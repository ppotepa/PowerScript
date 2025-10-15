# PowerScript

A modern, expressive scripting language with clean syntax and powerful features.

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Tests](https://img.shields.io/badge/tests-47%2F47-success)
![License](https://img.shields.io/badge/license-MIT-blue)

## Overview

PowerScript is a statically-typed scripting language designed for clarity, safety, and ease of use. It combines the simplicity of scripting languages with the robustness of compiled languages.

## Features

### Core Language Features

- **Static Type System** - Type safety with `INT` (integers), `STRING` (text), `NUMBER` (integers or decimals), and dynamic `FLEX` types
- **Functions with Return Types** - Clear function signatures with explicit return type declarations
- **Recursion Support** - Full support for recursive function calls
- **Expression-Based** - Rich expression syntax including arithmetic, comparison, and logical operations
- **Template Strings** - String interpolation with `$variable` syntax
- **Array Support** - First-class arrays with indexing and literal syntax

### Control Flow

- **IF/ELSE Statements** - SQL-style conditional logic
- **CYCLE Loops** - Versatile looping with count-based and collection-based iteration
- **Auto-Generated Loop Variables** - Automatic loop variable naming (A, B, C...)
- **Custom Loop Variables** - Explicit variable naming with `AS` keyword

### Type System

```powerscript
// Static types
INT age = 25                    // Integer only
STRING name = "PowerScript"     // Text
NUMBER pi = 3.14159             // Can be integer or decimal
NUMBER count = 100              // NUMBER accepts integers too

// Strict auto-typed variable
VAR total = 42                  // Type inferred as INT, cannot change

// Dynamic typing
FLEX value = 42                 // Can change type anytime
FLEX data = "can be anything"
FLEX list = [1, 2, 3, 4, 5]
```

### Functions

```powerscript
// Function with return type
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

// Recursive functions
FUNCTION factorial(INT n)[INT] {
    IF n <= 1 {
        RETURN 1
    }
    RETURN n * factorial(n - 1)
}

// Fibonacci sequence
FUNCTION fib(INT n)[INT] {
    IF n <= 1 {
        RETURN n
    }
    RETURN fib(n - 1) + fib(n - 2)
}
```

### Loops

```powerscript
// Count-based loop with auto-generated variable 'A'
CYCLE 5 {
    PRINT A  // 0, 1, 2, 3, 4
}

// Count-based loop with custom variable
CYCLE 10 AS i {
    PRINT i  // 0, 1, 2, 3, ..., 9
}

// Nested loops with auto-generated variables
CYCLE 3 AS outer {
    CYCLE 3 {
        PRINT outer
        PRINT A  // Inner loop gets 'A', outer has explicit name
    }
}

// Expression-based loops
FLEX count = 5
CYCLE count AS i {
    PRINT i
}
```

### Conditionals

```powerscript
// Simple IF statement
IF x > 10 {
    PRINT "Greater than 10"
}

// IF-ELSE statement
IF temperature >= 30 {
    PRINT "Hot"
} ELSE {
    PRINT "Not hot"
}

// Comparison operators: >, <, >=, <=, ==, !=
IF score == 100 {
    PRINT "Perfect!"
}

// Logical operators: AND, OR
IF age >= 18 AND hasLicense == 1 {
    PRINT "Can drive"
}
```

### Arrays

```powerscript
// Array literal
FLEX numbers = [1, 2, 3, 4, 5]

// Array indexing
FLEX first = numbers[0]  // 1
FLEX last = numbers[4]   // 5

// Array assignment
FLEX arr = [0, 0, 0]
FLEX arr[0] = 10
FLEX arr[1] = 20
FLEX arr[2] = 30

// Multi-dimensional arrays
FLEX matrix = [[1, 2], [3, 4]]
FLEX value = matrix[0][1]  // 2

// Dynamic array creation
FLEX size = 10
FLEX arr = CHAIN size
```

### String Features

```powerscript
// String literals
STRING message = "Hello, PowerScript!"

// Template strings with interpolation
FLEX name = "World"
FLEX greeting = $"Hello, $name!"  // "Hello, World!"

// String concatenation
STRING result = "Hello" + " " + "World"
```

### Arithmetic Operations

```powerscript
// Basic arithmetic
FLEX sum = 10 + 5      // 15
FLEX diff = 10 - 5     // 5
FLEX product = 10 * 5  // 50
FLEX quotient = 10 / 5 // 2
FLEX remainder = 10 % 3 // 1

// Operator precedence (standard math rules)
FLEX result = 2 + 3 * 4  // 14 (not 20)
FLEX result = (2 + 3) * 4 // 20

// Expressions in function calls
FLEX value = factorial(5 + 3)  // factorial(8)
```

### Output

```powerscript
// Print values
PRINT "Hello, World!"
PRINT 42
PRINT variable

// Print expressions
PRINT 10 + 20  // 30
PRINT "Sum: " + sum
```

## Example Programs

### Hello World
```powerscript
PRINT "Hello, World!"
```

### Sum of Even Numbers
```powerscript
FUNCTION isEven(INT n)[INT] {
    RETURN (n % 2 == 0)
}

FLEX sum = 0
CYCLE 10 AS i {
    FLEX num = i + 1
    IF isEven(num) == 1 {
        FLEX sum = sum + (num * num)
    }
}

PRINT sum  // 220 (4 + 16 + 36 + 64 + 100)
```

### Factorial Calculator
```powerscript
FUNCTION factorial(INT n)[INT] {
    IF n <= 1 {
        RETURN 1
    }
    RETURN n * factorial(n - 1)
}

PRINT factorial(5)  // 120
```

### Fibonacci Sequence
```powerscript
FUNCTION fib(INT n)[INT] {
    IF n <= 1 {
        RETURN n
    }
    RETURN fib(n - 1) + fib(n - 2)
}

PRINT fib(7)  // 13
```

### Prime Number Detection
```powerscript
FUNCTION isPrime(INT n)[INT] {
    IF n <= 1 {
        RETURN 0
    }
    IF n == 2 {
        RETURN 1
    }
    
    FLEX i = 2
    CYCLE n - 2 AS i {
        FLEX divisor = i + 2
        IF n % divisor == 0 {
            RETURN 0
        }
    }
    RETURN 1
}

PRINT isPrime(17)  // 1 (true)
PRINT isPrime(18)  // 0 (false)
```

## Installation

### Prerequisites
- .NET 8.0 SDK or later

### Building from Source

```bash
# Clone the repository
git clone https://github.com/ppotepa/powerscript.git
cd powerscript

# Build the project
dotnet build

# Run tests
dotnet test

# Run the CLI
dotnet run --project src/PowerScript.CLI -- script.ps
```

## Usage

### Running Scripts

```bash
# Run a PowerScript file
powerscript.exe script.ps

# Or using dotnet
dotnet run --project src/PowerScript.CLI -- script.ps
```

### File Extension
PowerScript files use the `.ps` extension.

## Language Syntax Quick Reference

| Feature | Syntax | Example |
|---------|--------|---------|
| Function Definition | `FUNCTION name(TYPE param)[RETURNTYPE] { }` | `FUNCTION add(INT a, INT b)[INT] { RETURN a + b }` |
| Variable Declaration | `TYPE name = value` | `INT count = 0` |
| Dynamic Variable | `FLEX name = value` | `FLEX data = [1, 2, 3]` |
| If Statement | `IF condition { }` | `IF x > 10 { PRINT "big" }` |
| If-Else Statement | `IF condition { } ELSE { }` | `IF x > 0 { PRINT "pos" } ELSE { PRINT "neg" }` |
| Loop (count) | `CYCLE count { }` | `CYCLE 5 { PRINT A }` |
| Loop (custom var) | `CYCLE count AS var { }` | `CYCLE 10 AS i { PRINT i }` |
| Array Literal | `[element1, element2, ...]` | `FLEX arr = [1, 2, 3]` |
| Array Access | `array[index]` | `FLEX val = arr[0]` |
| Template String | `$"text $var text"` | `FLEX msg = $"Hello $name"` |
| Print | `PRINT expression` | `PRINT "Hello"` |
| Return | `RETURN expression` | `RETURN x + y` |

## Operators

### Arithmetic Operators
- `+` Addition
- `-` Subtraction
- `*` Multiplication
- `/` Division
- `%` Modulo

### Comparison Operators
- `>` Greater than
- `<` Less than
- `>=` Greater than or equal
- `<=` Less than or equal
- `==` Equal
- `!=` Not equal

### Logical Operators
- `AND` Logical AND
- `OR` Logical OR

## Type System

| Type | Description | Example |
|------|-------------|---------|
| `INT` | Integer numbers (whole numbers only) | `INT age = 25` |
| `STRING` | Text strings | `STRING name = "John"` |
| `NUMBER` | Numeric values (integers or decimals) | `NUMBER pi = 3.14159` or `NUMBER count = 100` |
| `FLEX` | Dynamic type (can change type) | `FLEX value = anything` |
| `VAR` | Strict auto-typed (inferred, cannot change) | `VAR count = 42` |

### Type Characteristics

- **INT**: Stores whole numbers only. Best for counters, indices, and integer math.
- **STRING**: Stores text. Supports concatenation with `+` operator.
- **NUMBER**: The most flexible numeric type - accepts both integers and floating-point values. Use when you need decimal precision or mixed numeric operations.
- **FLEX**: Dynamic type that can hold any value and change types during execution.
- **VAR**: Type is inferred from initial value but cannot be changed afterward (strict typing with inference).

## Architecture

PowerScript is built with a clean separation of concerns:

- **PowerScript.Core** - Core AST and token definitions
- **PowerScript.Common** - Shared utilities and logging
- **PowerScript.Parser** - Lexer and parser implementation
- **PowerScript.Compiler** - Compilation pipeline
- **PowerScript.Runtime** - Execution engine
- **PowerScript.Interpreter** - High-level interpreter interface
- **PowerScript.CLI** - Command-line interface

## Testing

PowerScript has comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Simple"
dotnet test --filter "Category=Loops"
dotnet test --filter "Category=Recursion"
```

**Test Results**: 47/47 tests passing (100% ✅)

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

### Development Setup

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests to ensure everything works
5. Submit a pull request

## Roadmap

- [ ] Standard library expansion
- [ ] Module system
- [ ] Error handling (try/catch)
- [ ] More collection types (dictionaries, sets)
- [ ] LINQ-style operations
- [ ] File I/O operations
- [ ] Network capabilities
- [ ] Package manager

## License

MIT License - see LICENSE file for details

## Credits

Created by ppotepa

## Support

- GitHub Issues: [Report bugs or request features](https://github.com/ppotepa/powerscript/issues)
- Documentation: See `/docs` folder for detailed documentation

---

**PowerScript** - Simple, Powerful, Expressive
