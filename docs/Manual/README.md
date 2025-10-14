# PowerScript Language Reference Manual

## Complete Documentation for PowerScript Programming Language

This manual provides comprehensive documentation for the PowerScript programming language, a statically-typed, compiled scripting language with .NET interoperability.

---

## Table of Contents

### [Part 1: Introduction](1_Introduction.md)
- About PowerScript
- Key Features
- Design Philosophy
- File Extension
- Getting Started
- Compilation and Execution
- Case Sensitivity
- Comments
- Program Structure

### [Part 2: Data Types and Variables](2_DataTypes_Variables.md)
- Data Types Overview (INT, STRING, BOOL, ARRAY)
- Variable Declaration with FLEX
- Type System
- Array Operations
- Type Coercion
- Variable Scope
- Best Practices

### [Part 3: Operators and Expressions](3_Operators_Expressions.md)
- Arithmetic Operators (+, -, *, /, %)
- Comparison Operators (==, !=, <, >, <=, >=)
- Logical Operators (AND, OR, NOT)
- Operator Precedence
- Parentheses and Grouping
- String Concatenation
- Complex Expressions

### [Part 4: Control Flow](4_Control_Flow.md)
- IF Statement
- CYCLE Loop
- Nested Loops
- Combining Loops and Conditionals
- Array Iteration
- Common Patterns (Counter, Accumulator)
- Algorithmic Patterns (Sort, Search, Min/Max)

### [Part 5: Functions](5_Functions.md)
- Function Declaration
- Void Functions
- Functions with Return Values
- Parameters (Typed)
- Return Statement
- Recursion
- Local and Global Variables
- Function Composition

### [Part 6: .NET Interoperability](6_NET_Interoperability.md)
- LINK Statement for Namespaces
- .NET Method Calls (NET:: and # syntax)
- Console Operations
- String Operations
- Math Operations
- File I/O
- Common .NET Patterns

### [Part 7: File Linking and Modules](7_File_Linking_Modules.md)
- File Linking with LINK "file.ps"
- How File Linking Works
- Standard Library Usage
- Creating Library Files
- Organizing Libraries
- Libs Folder Structure
- Circular Reference Prevention
- Module Patterns

### [Part 8: Standard Library Reference](8_Standard_Library.md)
- Standard Library Overview
- Output Functions (out, outln, outNum, outStr, outMulti, outThree)
- Utility Functions (newline, space)
- Complete Function Reference
- Usage Patterns
- Extending the Standard Library

### [Part 9: Best Practices and Style Guide](9_Best_Practices.md)
- Code Organization
- Naming Conventions
- Code Formatting
- Comments and Documentation
- Error Handling
- Function Design
- Code Clarity
- Performance Considerations
- Testing and Debugging
- Common Patterns
- Anti-Patterns to Avoid

### [Part 10: Language Grammar and Technical Reference](10_Grammar_Reference.md)
- Lexical Structure
- Keywords and Operators
- Literals and Identifiers
- Grammar Rules (EBNF)
- Type System
- Scoping Rules
- Execution Model
- Memory Model
- Interoperability
- Limitations
- Error Messages

---

## Quick Start Example

```powerscript
// Link the standard library
LINK "Libs/StdLib.ps"

// Declare variables
FLEX x = 10
FLEX y = 20

// Define a function
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

// Use the function
FLEX sum = add(x, y)

// Output the result
outStr("Sum: ")
outNum(sum)
```

---

## Language Features at a Glance

✅ **Static Typing**: INT, STRING, BOOL, ARRAY  
✅ **Functions**: Typed parameters and return values  
✅ **Control Flow**: IF statements and CYCLE loops  
✅ **Arrays**: Dynamic arrays with zero-based indexing  
✅ **.NET Integration**: Direct access to .NET Framework  
✅ **File Linking**: Modular code organization  
✅ **Standard Library**: Built-in utility functions  
✅ **Compiled**: Compiles to .NET Lambda expressions  
✅ **Turing Complete**: Full computational capability  

---

## Document Structure

Each part of this manual follows a consistent structure:

1. **Overview**: Introduction to the topic
2. **Syntax**: Formal syntax definitions
3. **Examples**: Practical code examples
4. **Best Practices**: Recommended usage patterns
5. **Common Pitfalls**: What to avoid
6. **Reference**: Quick lookup tables

---

## Reading Guide

### For Beginners
Start with:
1. Part 1: Introduction
2. Part 2: Data Types and Variables
3. Part 4: Control Flow
4. Part 8: Standard Library Reference

### For Experienced Programmers
Focus on:
1. Part 1: Introduction (language overview)
2. Part 6: .NET Interoperability
3. Part 7: File Linking and Modules
4. Part 10: Grammar Reference

### For Library Authors
Essential reading:
1. Part 7: File Linking and Modules
2. Part 5: Functions
3. Part 9: Best Practices

---

## Code Examples

All code examples in this manual are tested and verified. They can be run directly with the PowerScript compiler:

```bash
ppotepa.tokenez.exe example.ps
```

---

## Version

**Manual Version**: 1.0.0  
**PowerScript Version**: 1.0.0  
**Last Updated**: October 2025  

---

## Feedback and Contributions

This manual is a living document. If you find errors, have suggestions, or want to contribute examples, please provide feedback.

---

## License

This documentation is part of the PowerScript project.

---

## Additional Resources

- **Standard Library**: Located in `Libs/StdLib.ps`
- **Example Scripts**: See `TuringCompletenessTests/scripts/`
- **Project Structure**: See `Libs/README.md`

---

## Conventions Used

Throughout this manual:

- **Code blocks**: PowerScript code examples
- `keyword`: Language keywords and identifiers
- **Bold**: Important concepts
- *Italic*: Emphasis
- ✅ Feature available
- ⚠️ Warning or limitation
- 📌 Important note

---

## Navigation Tips

- Each part is a self-contained document
- Use the table of contents to jump to specific topics
- Cross-references link to related sections
- Code examples are syntax-highlighted
- All examples include expected output where applicable

---

**Happy Coding with PowerScript!** 🚀
