# PowerScript Language Reference Manual
## Part 8: Standard Library Reference

### 8.1 Standard Library Overview

The PowerScript Standard Library (`StdLib.ps`) provides essential functions for input/output operations. It is located in the `Libs/` folder and is automatically copied to the output directory.

### 8.2 Including the Standard Library

```powerscript
LINK "Libs/StdLib.ps"
```

After linking, all standard library functions are available.

### 8.3 Output Functions

#### 8.3.1 out(INT value)

Outputs a value to the console using `Console.WriteLine`.

**Signature:**
```powerscript
FUNCTION out(INT value)[]
```

**Parameters:**
- `value` (INT): The value to output

**Returns:** None (void function)

**Example:**
```powerscript
LINK "Libs/StdLib.ps"

FLEX number = 42
out(number)  // Prints: 42
```

**Implementation:**
```powerscript
FUNCTION out(INT value)[] {
    #Console.WriteLine(value)
}
```

#### 8.3.2 outln(INT value)

Outputs a value to the console with newline. Equivalent to `out()`.

**Signature:**
```powerscript
FUNCTION outln(INT value)[]
```

**Parameters:**
- `value` (INT): The value to output

**Returns:** None (void function)

**Example:**
```powerscript
LINK "Libs/StdLib.ps"

outln(100)
outln(200)
// Prints:
// 100
// 200
```

#### 8.3.3 outNum(INT number)

Specifically for outputting numeric values.

**Signature:**
```powerscript
FUNCTION outNum(INT number)[]
```

**Parameters:**
- `number` (INT): The number to output

**Returns:** None (void function)

**Example:**
```powerscript
LINK "Libs/StdLib.ps"

FLEX age = 25
outNum(age)  // Prints: 25

FLEX total = 100 + 50
outNum(total)  // Prints: 150
```

#### 8.3.4 outStr(STRING text)

Outputs string values to the console.

**Signature:**
```powerscript
FUNCTION outStr(STRING text)[]
```

**Parameters:**
- `text` (STRING): The string to output

**Returns:** None (void function)

**Example:**
```powerscript
LINK "Libs/StdLib.ps"

outStr("Hello, World!")
// Prints: Hello, World!

FLEX name = "PowerScript"
outStr(name)
// Prints: PowerScript
```

**Implementation:**
```powerscript
FUNCTION outStr(STRING text)[] {
    #Console.WriteLine(text)
}
```

#### 8.3.5 outMulti(INT a, INT b)

Outputs two values sequentially.

**Signature:**
```powerscript
FUNCTION outMulti(INT a, INT b)[]
```

**Parameters:**
- `a` (INT): First value
- `b` (INT): Second value

**Returns:** None (void function)

**Example:**
```powerscript
LINK "Libs/StdLib.ps"

outMulti(10, 20)
// Prints:
// 10
// 20

FLEX x = 5
FLEX y = 15
outMulti(x, y)
// Prints:
// 5
// 15
```

**Implementation:**
```powerscript
FUNCTION outMulti(INT a, INT b)[] {
    #Console.WriteLine(a)
    #Console.WriteLine(b)
}
```

#### 8.3.6 outThree(INT a, INT b, INT c)

Outputs three values sequentially.

**Signature:**
```powerscript
FUNCTION outThree(INT a, INT b, INT c)[]
```

**Parameters:**
- `a` (INT): First value
- `b` (INT): Second value
- `c` (INT): Third value

**Returns:** None (void function)

**Example:**
```powerscript
LINK "Libs/StdLib.ps"

outThree(1, 2, 3)
// Prints:
// 1
// 2
// 3

FLEX x = 10
FLEX y = 20
FLEX z = 30
outThree(x, y, z)
```

**Implementation:**
```powerscript
FUNCTION outThree(INT a, INT b, INT c)[] {
    #Console.WriteLine(a)
    #Console.WriteLine(b)
    #Console.WriteLine(c)
}
```

### 8.4 Utility Functions

#### 8.4.1 newline(INT dummy)

Prints a blank line (newline character).

**Signature:**
```powerscript
FUNCTION newline(INT dummy)[]
```

**Parameters:**
- `dummy` (INT): Unused parameter (pass any value, typically 0)

**Returns:** None (void function)

**Example:**
```powerscript
LINK "Libs/StdLib.ps"

outStr("Line 1")
newline(0)
outStr("Line 3")
// Prints:
// Line 1
// 
// Line 3
```

**Implementation:**
```powerscript
FUNCTION newline(INT dummy)[] {
    #Console.WriteLine("")
}
```

**Why dummy parameter?**
PowerScript currently requires all functions to have at least one parameter for certain internal mechanisms. Pass `0` as a placeholder.

#### 8.4.2 space(INT dummy)

Prints a single space character without newline.

**Signature:**
```powerscript
FUNCTION space(INT dummy)[]
```

**Parameters:**
- `dummy` (INT): Unused parameter (pass any value, typically 0)

**Returns:** None (void function)

**Example:**
```powerscript
LINK "Libs/StdLib.ps"

outNum(10)
space(0)
outNum(20)
space(0)
outNum(30)
// Prints: 10 20 30 (with spaces between)
```

**Implementation:**
```powerscript
FUNCTION space(INT dummy)[] {
    #Console.Write(" ")
}
```

### 8.5 Standard Library Complete Reference

**Full StdLib.ps Source:**

```powerscript
// PowerScript Standard Library
// This library contains common utility functions

// Link .NET System namespace for Console operations
LINK System

// ============================================================
// OUTPUT FUNCTIONS
// ============================================================

FUNCTION out(INT value)[] {
    #Console.WriteLine(value)
}

FUNCTION outln(INT value)[] {
    #Console.WriteLine(value)
}

FUNCTION outNum(INT number)[] {
    #Console.WriteLine(number)
}

FUNCTION outStr(STRING text)[] {
    #Console.WriteLine(text)
}

FUNCTION outMulti(INT a, INT b)[] {
    #Console.WriteLine(a)
    #Console.WriteLine(b)
}

FUNCTION outThree(INT a, INT b, INT c)[] {
    #Console.WriteLine(a)
    #Console.WriteLine(b)
    #Console.WriteLine(c)
}

// ============================================================
// HELPER FUNCTIONS
// ============================================================

FUNCTION newline(INT dummy)[] {
    #Console.WriteLine("")
}

FUNCTION space(INT dummy)[] {
    #Console.Write(" ")
}
```

### 8.6 Usage Patterns

**Pattern 1: Simple Output**

```powerscript
LINK "Libs/StdLib.ps"

FLEX x = 42
outNum(x)
```

**Pattern 2: Formatted Output**

```powerscript
LINK "Libs/StdLib.ps"

outStr("=== Results ===")
newline(0)
outStr("Score: ")
outNum(95)
newline(0)
outStr("Grade: ")
outStr("A")
```

**Pattern 3: Multiple Values**

```powerscript
LINK "Libs/StdLib.ps"

FLEX a = 10
FLEX b = 20
FLEX c = 30

outStr("Values:")
outThree(a, b, c)
```

**Pattern 4: Array Output**

```powerscript
LINK "Libs/StdLib.ps"

FLEX numbers = [1, 2, 3, 4, 5]

outStr("Array contents:")
CYCLE 5 AS i {
    outNum(numbers[i])
}
```

**Pattern 5: Calculation Results**

```powerscript
LINK "Libs/StdLib.ps"

FLEX price = 100
FLEX quantity = 5
FLEX total = price * quantity

outStr("Price: ")
outNum(price)
outStr("Quantity: ")
outNum(quantity)
outStr("Total: ")
outNum(total)
```

### 8.7 Common Use Cases

**Use Case 1: Debugging**

```powerscript
LINK "Libs/StdLib.ps"

FUNCTION debugValue(INT value)[] {
    outStr("[DEBUG] Value = ")
    outNum(value)
}

FLEX x = 10
debugValue(x)
FLEX y = x * 2
debugValue(y)
```

**Use Case 2: Progress Indicator**

```powerscript
LINK "Libs/StdLib.ps"

outStr("Processing...")
newline(0)

CYCLE 5 AS i {
    outNum(i)
    outStr("% complete")
}

newline(0)
outStr("Done!")
```

**Use Case 3: Report Generation**

```powerscript
LINK "Libs/StdLib.ps"

FUNCTION printReport(STRING title, INT value1, INT value2)[] {
    outStr("=== " + title + " ===")
    newline(0)
    outStr("First value: ")
    outNum(value1)
    outStr("Second value: ")
    outNum(value2)
    outStr("Total: ")
    outNum(value1 + value2)
    newline(0)
}

printReport("Monthly Report", 1000, 2000)
```

### 8.8 Extending the Standard Library

**Add Custom Functions:**

You can extend StdLib.ps or create your own library:

```powerscript
// MyStdLib.ps - Extended standard library
LINK "Libs/StdLib.ps"
LINK System

// Additional output function
FUNCTION outBool(BOOL value)[] {
    IF value {
        outStr("TRUE")
    }
    IF NOT value {
        outStr("FALSE")
    }
}

// Formatted number output
FUNCTION outFormatted(STRING label, INT value)[] {
    outStr(label + ": ")
    outNum(value)
}

// Output separator line
FUNCTION separator(INT dummy)[] {
    outStr("====================")
}
```

**Using Extended Library:**

```powerscript
LINK "MyStdLib.ps"

FLEX isActive = TRUE
outBool(isActive)  // Prints: TRUE

outFormatted("Age", 25)  // Prints: Age: 25

separator(0)
```

### 8.9 Standard Library Best Practices

**1. Always Link at Top:**

```powerscript
// Good
LINK "Libs/StdLib.ps"

FLEX x = 10
outNum(x)
```

**2. Use Appropriate Function:**

```powerscript
// Good: Use outStr for strings
outStr("Hello")

// Good: Use outNum for numbers
outNum(42)

// Less clear: Using out() everywhere
out(42)
```

**3. Formatting Output:**

```powerscript
// Good: Clear, formatted output
outStr("=== Report ===")
newline(0)
outStr("Total: ")
outNum(total)

// Less readable: All on one attempt
outStr("=== Report ===Total: ")
```

**4. Comments for Clarity:**

```powerscript
LINK "Libs/StdLib.ps"

// Print header
outStr("=== Data Analysis ===")
newline(0)

// Print results
outStr("Sum: ")
outNum(sum)
```
