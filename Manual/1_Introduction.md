# PowerScript Language Reference Manual
## Part 1: Introduction

### 1.1 About PowerScript

PowerScript is a statically-typed, compiled scripting language that combines simple syntax with powerful features. It compiles to .NET Lambda expressions for high-performance execution while maintaining an easy-to-learn syntax suitable for beginners and experienced programmers alike.

### 1.2 Key Features

- **Simple Syntax**: Clean, readable code with minimal boilerplate
- **Static Typing**: Type-safe with INT, STRING, BOOL, and ARRAY types
- **.NET Interoperability**: Direct access to .NET Framework methods
- **File Linking**: Modular code organization with LINK statements
- **Standard Library**: Built-in functions for common operations
- **Compiled Execution**: Compiles to optimized .NET Lambda expressions
- **Turing Complete**: Supports variables, conditionals, loops, and functions

### 1.3 Design Philosophy

PowerScript is designed with the following principles:

1. **Simplicity First**: Keywords are intuitive (FLEX for variables, CYCLE for loops)
2. **Explicit Types**: All types must be declared for clarity and safety
3. **Minimal Ceremony**: No semicolons, minimal punctuation
4. **Practical Interop**: Easy access to .NET ecosystem
5. **Developer Friendly**: Clear error messages and diagnostic warnings

### 1.4 File Extension

PowerScript files use the `.ps` extension:
- Source files: `program.ps`
- Library files: `StdLib.ps`

### 1.5 Getting Started

A minimal PowerScript program:

```powerscript
PRINT "Hello, World!"
```

A simple calculation:

```powerscript
FLEX x = 5
FLEX y = 10
FLEX sum = x + y
PRINT sum
```

### 1.6 Compilation and Execution

PowerScript uses a multi-stage process:

1. **Preprocessing**: File linking and expansion
2. **Tokenization**: Source code → Token stream
3. **Parsing**: Tokens → Abstract Syntax Tree (AST)
4. **Compilation**: AST → .NET Lambda expressions
5. **Execution**: Compiled code runs on .NET runtime

### 1.7 Case Sensitivity

- **Keywords**: Case-insensitive (FLEX, flex, Flex all valid)
- **Variable names**: Case-insensitive but converted to UPPERCASE internally
- **Function names**: Case-insensitive
- **String literals**: Case-sensitive

### 1.8 Comments

PowerScript supports C-style comments:

```powerscript
// Single-line comment

/* Multi-line comment
   can span multiple lines */
```

### 1.9 Program Structure

A PowerScript program consists of:

1. **LINK statements** (optional): Import namespaces or files
2. **Function declarations** (optional): Define reusable functions
3. **Root statements**: Top-level code executed sequentially

Example:

```powerscript
// 1. Link external files and namespaces
LINK System
LINK "Libs/StdLib.ps"

// 2. Define functions
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

// 3. Root-level execution
FLEX result = add(5, 3)
PRINT result
```

### 1.10 Next Steps

Continue to the following sections to learn PowerScript:

- **Part 2**: Data Types and Variables
- **Part 3**: Operators and Expressions
- **Part 4**: Control Flow
- **Part 5**: Functions
- **Part 6**: .NET Interoperability
- **Part 7**: File Linking and Modules
- **Part 8**: Standard Library Reference
- **Part 9**: Best Practices
- **Part 10**: Language Grammar Reference
