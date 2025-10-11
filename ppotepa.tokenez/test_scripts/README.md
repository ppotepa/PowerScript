# PowerScript Test Scripts

This folder contains various test scripts for the PowerScript language interpreter.

## Test Files

### Core Language Features
- **`test_variables.ps`** - Tests variable declarations and assignments
- **`test_scoped_vars.ps`** - Tests variable scoping and nested scopes
- **`test_duplicate.ps`** - Tests duplicate declaration handling
- **`test_execute.ps`** - Tests the EXECUTE command for running external scripts

### Function Features
- **`test_return_types.ps`** - Tests basic return type functionality with `[TYPE]` syntax
- **`test_comprehensive_return_types.ps`** - Comprehensive test of return types with multiple functions
- **`test_new_types.ps`** - Tests all new types: PREC, CHAR, STRING, and CHAIN collections
- **`program.ps`** - Function definitions and mathematical operations

### Demo Scripts
- **`demo.ps`** - General demonstration of language features
- **`demo_variables.ps`** - Variable demonstration script
- **`helper.ps`** - Helper functions and utilities

### Testing Infrastructure
- **`quick_test.ps`** - Quick test for rapid verification
- **`test_shell.ps1`** - PowerShell script for testing the interactive shell

## Type System

PowerScript supports a rich type system with both primitive and collection types:

### Primitive Types
- **`INT`** - Integer numbers (e.g., `1`, `42`, `-10`)
- **`PREC`** - Precision/floating-point numbers (e.g., `3.14`, `0.5`)
- **`CHAR`** - Single character values (e.g., `'A'`, `'x'`)
- **`STRING`** - Text strings (equivalent to `CHAR CHAIN`)

### Collection Types (CHAIN)
- **`INT CHAIN`** - Arrays/collections of integers
- **`PREC CHAIN`** - Arrays/collections of precision numbers
- **`CHAR CHAIN`** - Arrays/collections of characters (same as STRING)
- **`STRING CHAIN`** - Arrays/collections of strings

### Type Aliases
- **`STRING`** = **`CHAR CHAIN`** (strings are character chains)

## Return Type Syntax

The PowerScript language supports optional return types using square bracket notation:

```powerscript
FUNCTION functionName(TYPE param1, TYPE param2)[RETURN_TYPE]{
    RETURN value
}
```

### Examples:

**Function with primitive return type:**
```powerscript
FUNCTION multiply(INT A, INT B)[INT]{
    RETURN A * B
}

FUNCTION calculatePrice(PREC base, PREC tax)[PREC]{
    RETURN base * tax
}
```

**Function with collection return type:**
```powerscript
FUNCTION processNumbers(INT CHAIN data)[INT CHAIN]{
    RETURN data
}

FUNCTION formatText(CHAR CHAIN input)[STRING]{
    RETURN input
}
```

**Void function (no return type):**
```powerscript
FUNCTION sayHello(){
    PRINT "Hello World"
}
```

## Running Tests

To run any test script:
```bash
dotnet run -- test_scripts/script_name.ps
```

Example:
```bash
dotnet run -- test_scripts/test_comprehensive_return_types.ps
```