# PowerScript Language Reference Manual
## Part 10: Language Grammar and Technical Reference

### 10.1 Lexical Structure

#### 10.1.1 Keywords

Reserved keywords in PowerScript (case-insensitive):

**Control Flow:**
- `IF` - Conditional statement
- `CYCLE` - Loop statement
- `AS` - Loop iterator keyword

**Functions:**
- `FUNCTION` - Function declaration
- `RETURN` - Return statement

**Variables:**
- `FLEX` - Variable declaration
- `VAR` - Alternative variable declaration

**Types:**
- `INT` - Integer type
- `STRING` - String type
- `BOOL` - Boolean type
- `ARRAY` - Array type

**Values:**
- `TRUE` - Boolean true
- `FALSE` - Boolean false

**Operators:**
- `AND` - Logical AND
- `OR` - Logical OR
- `NOT` - Logical NOT

**I/O:**
- `PRINT` - Output statement

**Interop:**
- `LINK` - Import namespace or file
- `NET::` - .NET method call prefix
- `EXECUTE` - Execute external script

#### 10.1.2 Operators

**Arithmetic:**
- `+` Addition
- `-` Subtraction
- `*` Multiplication
- `/` Division
- `%` Modulo

**Comparison:**
- `==` Equal to
- `!=` Not equal to
- `<` Less than
- `>` Greater than
- `<=` Less than or equal
- `>=` Greater than or equal

**Logical:**
- `AND` Logical conjunction
- `OR` Logical disjunction
- `NOT` Logical negation

**Assignment:**
- `=` Assignment

**Delimiters:**
- `{` `}` Code blocks
- `(` `)` Parameters, grouping
- `[` `]` Arrays, return types
- `,` Parameter separator
- `"` String delimiter
- `#` .NET shorthand prefix

#### 10.1.3 Literals

**Integer Literals:**
```powerscript
42
-10
0
1000000
```

**String Literals:**
```powerscript
"Hello, World!"
"PowerScript"
""  // Empty string
"Line 1\nLine 2"  // With escape sequences
```

**Boolean Literals:**
```powerscript
TRUE
FALSE
```

**Array Literals:**
```powerscript
[1, 2, 3, 4, 5]
["a", "b", "c"]
[10, 20, 30]
[]  // Empty array
```

#### 10.1.4 Identifiers

**Rules:**
- Start with letter or underscore
- Contain letters, numbers, underscores
- Case-insensitive (converted to UPPERCASE internally)
- Cannot be reserved keywords

**Valid:**
```powerscript
x
myVariable
user_name
count123
_private
```

**Invalid:**
```powerscript
123count    // Starts with number
my-variable // Contains hyphen
if          // Reserved keyword
```

#### 10.1.5 Comments

**Single-line:**
```powerscript
// This is a single-line comment
```

**Multi-line:**
```powerscript
/* This is a
   multi-line comment */
```

### 10.2 Grammar Rules

#### 10.2.1 Program Structure

```
Program := (LinkStatement | FunctionDeclaration | Statement)*

LinkStatement := "LINK" (Namespace | StringLiteral)

FunctionDeclaration := "FUNCTION" Identifier "(" ParameterList ")" "[" ReturnType "]" Block

Statement := VariableDeclaration
           | Assignment
           | IfStatement
           | CycleStatement
           | PrintStatement
           | ReturnStatement
           | FunctionCall
           | NetMethodCall
           | ExecuteStatement
```

#### 10.2.2 Variable Declaration

```
VariableDeclaration := "FLEX" Identifier "=" Expression

Assignment := "FLEX" Identifier "=" Expression
            | "FLEX" ArrayAccess "=" Expression

ArrayAccess := Identifier "[" Expression "]"
```

#### 10.2.3 Function Declaration

```
FunctionDeclaration := "FUNCTION" Identifier "(" ParameterList ")" "[" ReturnType "]" Block

ParameterList := Parameter ("," Parameter)*
               | ε  // Empty

Parameter := Type Identifier

ReturnType := Type
            | ε  // Empty for void

Type := "INT"
      | "STRING"
      | "BOOL"
      | "ARRAY"

Block := "{" Statement* "}"
```

#### 10.2.4 Control Flow

```
IfStatement := "IF" Expression Block

CycleStatement := "CYCLE" Expression "AS" Identifier Block
```

#### 10.2.5 Expressions

```
Expression := LogicalOrExpression

LogicalOrExpression := LogicalAndExpression ("OR" LogicalAndExpression)*

LogicalAndExpression := EqualityExpression ("AND" EqualityExpression)*

EqualityExpression := RelationalExpression (("==" | "!=") RelationalExpression)*

RelationalExpression := AdditiveExpression (("<" | ">" | "<=" | ">=") AdditiveExpression)*

AdditiveExpression := MultiplicativeExpression (("+" | "-") MultiplicativeExpression)*

MultiplicativeExpression := UnaryExpression (("*" | "/" | "%") UnaryExpression)*

UnaryExpression := "NOT" UnaryExpression
                 | PrimaryExpression

PrimaryExpression := Literal
                   | Identifier
                   | FunctionCall
                   | ArrayAccess
                   | "(" Expression ")"
```

#### 10.2.6 Literals and Calls

```
Literal := IntegerLiteral
         | StringLiteral
         | BooleanLiteral
         | ArrayLiteral

ArrayLiteral := "[" (Expression ("," Expression)*)? "]"

FunctionCall := Identifier "(" ArgumentList ")"

ArgumentList := Expression ("," Expression)*
              | ε  // Empty

NetMethodCall := ("NET::" | "#") DottedIdentifier "(" ArgumentList ")"

DottedIdentifier := Identifier ("." Identifier)*
```

### 10.3 Type System

#### 10.3.1 Primitive Types

**INT:**
- 32-bit signed integer
- Range: -2,147,483,648 to 2,147,483,647
- Default value: 0

**STRING:**
- Unicode text strings
- Immutable
- Default value: ""

**BOOL:**
- Boolean values
- Values: TRUE, FALSE
- Default value: FALSE

#### 10.3.2 Composite Types

**ARRAY:**
- Homogeneous or heterogeneous collections
- Zero-based indexing
- Dynamic sizing
- Stored as List<object> internally

#### 10.3.3 Type Inference

PowerScript uses explicit typing:

```powerscript
// Type determined by declaration
FLEX x = 10              // INT inferred from literal
FLEX name = "Alice"      // STRING inferred from literal
FLEX flag = TRUE         // BOOL inferred from literal
FLEX arr = [1, 2, 3]     // ARRAY inferred from literal
```

#### 10.3.4 Type Conversion

Automatic conversions:

```powerscript
// Number to String (in concatenation)
FLEX message = "Age: " + 30  // "Age: 30"

// String to Number (in arithmetic)
FLEX result = "100" + 50     // 150
```

### 10.4 Scoping Rules

#### 10.4.1 Global Scope

Variables declared at root level:

```powerscript
FLEX globalVar = 100

FUNCTION test()[INT] {
    RETURN globalVar  // Accessible
}
```

#### 10.4.2 Function Scope

Variables declared in functions:

```powerscript
FUNCTION test()[INT] {
    FLEX localVar = 50  // Function scope
    RETURN localVar
}
// localVar not accessible here
```

#### 10.4.3 Block Scope

Variables in loops and conditionals:

```powerscript
CYCLE 5 AS i {
    FLEX temp = i * 2  // Block scope
}
// temp not accessible here
```

### 10.5 Execution Model

#### 10.5.1 Compilation Phases

1. **Preprocessing**: File linking, expansion
2. **Tokenization**: Source → Tokens
3. **Parsing**: Tokens → AST
4. **Semantic Analysis**: Type checking, validation
5. **Compilation**: AST → Lambda expressions
6. **Execution**: Runtime evaluation

#### 10.5.2 Evaluation Order

1. Expressions evaluated left-to-right
2. Operator precedence respected
3. Short-circuit evaluation for AND/OR
4. Function arguments evaluated before call

### 10.6 Memory Model

#### 10.6.1 Variable Storage

- Variables stored in Dictionary<string, object>
- Keys are UPPERCASE identifiers
- Values are boxed .NET objects

#### 10.6.2 Array Storage

- Arrays stored as List<object>
- Dynamic resizing supported
- Elements accessed by integer index

### 10.7 Interoperability

#### 10.7.1 .NET Integration

PowerScript can call .NET methods:

```powerscript
// Full syntax
NET::System.Console.WriteLine("Text")

// Short syntax (requires LINK)
LINK System
#Console.WriteLine("Text")
```

#### 10.7.2 Namespace Resolution

```powerscript
LINK System          // Links System namespace
LINK System.IO       // Links System.IO namespace
LINK "file.ps"       // Links PowerScript file
```

### 10.8 Limitations

#### 10.8.1 Current Limitations

1. **No Classes**: Cannot define custom types
2. **No Interfaces**: No interface support
3. **No Generics**: No generic type parameters
4. **Static Methods Only**: Limited object instantiation
5. **No Exception Handling**: No try-catch-finally
6. **No Async/Await**: Synchronous execution only
7. **No Operator Overloading**: Fixed operator semantics

#### 10.8.2 Type Constraints

1. Function parameters must be explicitly typed
2. Return types must be declared
3. No type inference for function signatures
4. Arrays are type-unsafe (can hold mixed types)

### 10.9 Error Messages

#### 10.9.1 Common Compile Errors

**Syntax Errors:**
```
Unexpected token 'X' at line Y
Missing closing brace '}' for block
```

**Type Errors:**
```
Type mismatch: Expected INT, found STRING
Function 'add' requires 2 parameters, found 1
```

**Semantic Errors:**
```
Variable 'x' not declared
Function 'test' not found
Return type mismatch
```

#### 10.9.2 Runtime Errors

```
Index out of range: Array size 5, index 10
Division by zero
Null reference exception
```

### 10.10 Reserved for Future Use

Keywords reserved for potential future features:

- `CLASS` - Class definitions
- `INTERFACE` - Interface definitions
- `TRY`, `CATCH`, `FINALLY` - Exception handling
- `ASYNC`, `AWAIT` - Asynchronous operations
- `IMPORT`, `EXPORT` - Module system
- `CONST` - Constant declarations
- `LET` - Immutable bindings
- `WHILE` - While loops
- `FOR` - For loops
- `SWITCH`, `CASE` - Switch statements
- `BREAK`, `CONTINUE` - Loop control
- `THROW` - Exception throwing

### 10.11 Compatibility

**Target Platform:**
- .NET 8.0 or higher
- Windows, Linux, macOS (cross-platform)

**Dependencies:**
- System.Linq.Expressions
- System.Reflection
- Standard .NET libraries

### 10.12 Version Information

**Current Version:** 1.0.0  
**Language Specification:** PowerScript 2025  
**Last Updated:** October 2025

### 10.13 Grammar Summary (EBNF)

```ebnf
Program          = { LinkStmt | FuncDecl | Statement } ;
LinkStmt         = "LINK" ( Identifier | StringLit ) ;
FuncDecl         = "FUNCTION" Identifier "(" [ ParamList ] ")" 
                   "[" [ Type ] "]" Block ;
ParamList        = Param { "," Param } ;
Param            = Type Identifier ;
Type             = "INT" | "STRING" | "BOOL" | "ARRAY" ;
Block            = "{" { Statement } "}" ;
Statement        = VarDecl | Assignment | IfStmt | CycleStmt 
                   | PrintStmt | ReturnStmt | FuncCall | NetCall ;
VarDecl          = "FLEX" Identifier "=" Expression ;
Assignment       = "FLEX" ( Identifier | ArrayAccess ) "=" Expression ;
ArrayAccess      = Identifier "[" Expression "]" ;
IfStmt           = "IF" Expression Block ;
CycleStmt        = "CYCLE" Expression "AS" Identifier Block ;
PrintStmt        = "PRINT" Expression ;
ReturnStmt       = "RETURN" Expression ;
Expression       = LogOrExpr ;
LogOrExpr        = LogAndExpr { "OR" LogAndExpr } ;
LogAndExpr       = EqExpr { "AND" EqExpr } ;
EqExpr           = RelExpr { ( "==" | "!=" ) RelExpr } ;
RelExpr          = AddExpr { ( "<" | ">" | "<=" | ">=" ) AddExpr } ;
AddExpr          = MulExpr { ( "+" | "-" ) MulExpr } ;
MulExpr          = UnaryExpr { ( "*" | "/" | "%" ) UnaryExpr } ;
UnaryExpr        = [ "NOT" ] PrimaryExpr ;
PrimaryExpr      = Literal | Identifier | FuncCall | ArrayAccess 
                   | "(" Expression ")" ;
Literal          = IntLit | StringLit | BoolLit | ArrayLit ;
ArrayLit         = "[" [ Expression { "," Expression } ] "]" ;
FuncCall         = Identifier "(" [ ArgList ] ")" ;
ArgList          = Expression { "," Expression } ;
NetCall          = ( "NET::" | "#" ) DottedId "(" [ ArgList ] ")" ;
DottedId         = Identifier { "." Identifier } ;
```
