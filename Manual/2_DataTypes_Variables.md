# PowerScript Language Reference Manual
## Part 2: Data Types and Variables

### 2.1 Data Types Overview

PowerScript supports the following data types:

| Type | Description | Example Values |
|------|-------------|----------------|
| INT | Integer numbers | `42`, `-10`, `0` |
| STRING | Text strings | `"Hello"`, `"World"` |
| BOOL | Boolean values | `TRUE`, `FALSE` |
| ARRAY | Collections of values | `[1, 2, 3]`, `["a", "b"]` |

### 2.2 Variable Declaration

Variables are declared using the `FLEX` keyword:

```powerscript
FLEX variableName = value
```

**Rules:**
- Variable names are case-insensitive
- Must start with a letter or underscore
- Can contain letters, numbers, and underscores
- Cannot use reserved keywords

**Examples:**

```powerscript
FLEX x = 5
FLEX myNumber = 100
FLEX user_name = "Alice"
FLEX isActive = TRUE
```

### 2.3 INT Type

Integer type for whole numbers.

```powerscript
FLEX age = 25
FLEX temperature = -5
FLEX count = 0
FLEX largeNumber = 1000000
```

**Supported Operations:**
- Arithmetic: `+`, `-`, `*`, `/`, `%` (modulo)
- Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=`
- Assignment: `=`

**Examples:**

```powerscript
FLEX a = 10
FLEX b = 3
FLEX sum = a + b        // 13
FLEX product = a * b    // 30
FLEX remainder = a % b  // 1
FLEX isGreater = a > b  // TRUE
```

### 2.4 STRING Type

Text strings enclosed in double quotes.

```powerscript
FLEX greeting = "Hello, World!"
FLEX name = "PowerScript"
FLEX empty = ""
```

**String Concatenation:**

```powerscript
FLEX firstName = "John"
FLEX lastName = "Doe"
FLEX fullName = firstName + " " + lastName
PRINT fullName  // "John Doe"
```

**Escape Sequences:**

```powerscript
FLEX quote = "He said \"Hello\""
FLEX newLine = "Line 1\nLine 2"
FLEX tab = "Column1\tColumn2"
```

### 2.5 BOOL Type

Boolean values for logical operations.

```powerscript
FLEX isActive = TRUE
FLEX isComplete = FALSE
```

**Boolean Operations:**

```powerscript
FLEX a = TRUE
FLEX b = FALSE

FLEX andResult = a AND b    // FALSE
FLEX orResult = a OR b      // TRUE
FLEX notResult = NOT a      // FALSE
```

**Comparison Results:**

```powerscript
FLEX x = 10
FLEX y = 20
FLEX isEqual = x == y       // FALSE
FLEX isLess = x < y         // TRUE
```

### 2.6 ARRAY Type

Arrays hold collections of values.

**Array Literals:**

```powerscript
FLEX numbers = [1, 2, 3, 4, 5]
FLEX names = ["Alice", "Bob", "Charlie"]
FLEX mixed = [1, "two", 3, "four"]  // Mixed types allowed
FLEX empty = []
```

**Array Indexing:**

Arrays use zero-based indexing:

```powerscript
FLEX arr = [10, 20, 30, 40, 50]

FLEX first = arr[0]   // 10
FLEX third = arr[2]   // 30
FLEX last = arr[4]    // 50

PRINT arr[1]          // Prints: 20
```

**Array Assignment:**

Modify array elements:

```powerscript
FLEX arr = [1, 2, 3, 4, 5]

FLEX arr[0] = 100     // arr is now [100, 2, 3, 4, 5]
FLEX arr[2] = 999     // arr is now [100, 2, 999, 4, 5]
```

**Dynamic Indexing:**

```powerscript
FLEX arr = [10, 20, 30, 40, 50]

CYCLE 5 AS i {
    PRINT arr[i]      // Prints all elements
}
```

### 2.7 Type Coercion

PowerScript performs automatic type coercion in certain contexts:

**Number to String:**

```powerscript
FLEX num = 42
FLEX text = "The answer is " + num
PRINT text  // "The answer is 42"
```

**String to Number (in arithmetic):**

```powerscript
FLEX strNum = "100"
FLEX result = strNum + 50  // 150 (automatic conversion)
```

### 2.8 Variable Scope

**Global Scope:**

Variables declared in root scope are accessible throughout the program:

```powerscript
FLEX globalVar = 100

FUNCTION test()[INT] {
    RETURN globalVar  // Accessible here
}
```

**Local Scope:**

Variables declared inside functions or loops are local:

```powerscript
FUNCTION calculate()[INT] {
    FLEX localVar = 50  // Only accessible within function
    RETURN localVar
}

// localVar is not accessible here
```

**Loop Scope:**

```powerscript
CYCLE 5 AS i {
    FLEX temp = i * 2  // temp is local to loop iteration
    PRINT temp
}
// temp is not accessible here
```

### 2.9 Variable Reassignment

Variables can be reassigned:

```powerscript
FLEX x = 10
PRINT x       // 10

FLEX x = 20   // Reassign
PRINT x       // 20

FLEX x = x + 5
PRINT x       // 25
```

### 2.10 Constants

PowerScript does not have explicit constant declarations. By convention, use UPPERCASE names for values that should not change:

```powerscript
FLEX MAX_SIZE = 100
FLEX PI = 3.14159
FLEX APP_NAME = "PowerScript"
```

### 2.11 Common Patterns

**Counter Pattern:**

```powerscript
FLEX counter = 0
CYCLE 10 AS i {
    FLEX counter = counter + 1
}
PRINT counter  // 10
```

**Accumulator Pattern:**

```powerscript
FLEX sum = 0
FLEX arr = [1, 2, 3, 4, 5]

CYCLE 5 AS i {
    FLEX sum = sum + arr[i]
}
PRINT sum  // 15
```

**Swap Pattern:**

```powerscript
FLEX a = 10
FLEX b = 20
FLEX temp = a
FLEX a = b
FLEX b = temp
// Now a=20, b=10
```

### 2.12 Type Safety

PowerScript enforces type safety at compile time:

**Function Parameters:**

```powerscript
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

// Compiler ensures correct types are passed
```

**Array Type Consistency:**

While arrays can hold mixed types, using consistent types is recommended:

```powerscript
// Recommended: Homogeneous arrays
FLEX numbers = [1, 2, 3, 4, 5]
FLEX names = ["Alice", "Bob", "Charlie"]

// Allowed but not recommended: Mixed types
FLEX mixed = [1, "two", TRUE, [4, 5]]
```

### 2.13 Best Practices

1. **Use descriptive names**: `userCount` instead of `uc`
2. **Initialize variables**: Always assign initial values
3. **Keep scope minimal**: Declare variables where needed
4. **Use appropriate types**: Choose the correct type for data
5. **Avoid magic numbers**: Use named variables for constants

```powerscript
// Good
FLEX MAX_RETRIES = 3
FLEX attemptCount = 0

// Less good
FLEX x = 3
FLEX y = 0
```
