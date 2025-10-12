# PowerScript Language Reference Manual
## Part 7: File Linking and Modules

### 7.1 File Linking Overview

PowerScript supports modular programming through file linking. The `LINK` statement can import both .NET namespaces and PowerScript source files.

### 7.2 LINK Statement Syntax

**For .NET Namespaces:**

```powerscript
LINK NamespaceName
```

**For PowerScript Files:**

```powerscript
LINK "filepath.ps"
```

### 7.3 Linking PowerScript Files

**Basic File Linking:**

```powerscript
LINK "Libs/StdLib.ps"
```

**Relative Paths:**

```powerscript
// Link file in same directory
LINK "helper.ps"

// Link file in subdirectory
LINK "utils/math.ps"

// Link file in Libs folder
LINK "Libs/StdLib.ps"
```

**Multiple File Links:**

```powerscript
LINK "Libs/StdLib.ps"
LINK "Libs/MathLib.ps"
LINK "Libs/StringLib.ps"
```

### 7.4 How File Linking Works

**Preprocessing:**

1. PowerScript reads the source file
2. Finds all `LINK "file.ps"` statements
3. Expands linked files inline before tokenization
4. Adds comment markers to show file boundaries
5. Prevents circular references with tracking

**Expansion Example:**

```powerscript
// main.ps
LINK "library.ps"

FLEX result = add(5, 3)
PRINT result
```

Expands to:

```powerscript
// === Linked from: library.ps ===
LINK System
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}
// === End of: library.ps ===

FLEX result = add(5, 3)
PRINT result
```

### 7.5 Standard Library

**Using the Standard Library:**

```powerscript
LINK "Libs/StdLib.ps"

// Now can use StdLib functions
outStr("Hello, World!")
outNum(42)
newline(0)
```

**Standard Library Functions:**

```powerscript
LINK "Libs/StdLib.ps"

// Output functions
out(100)              // Print value
outln(200)            // Print value with newline
outNum(42)            // Print number
outStr("Text")        // Print string
outMulti(10, 20)      // Print two values
outThree(1, 2, 3)     // Print three values

// Utility functions
newline(0)            // Print newline
space(0)              // Print space
```

### 7.6 Creating Library Files

**Example: MathLib.ps**

```powerscript
LINK System

FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

FUNCTION subtract(INT a, INT b)[INT] {
    RETURN a - b
}

FUNCTION multiply(INT a, INT b)[INT] {
    RETURN a * b
}

FUNCTION divide(INT a, INT b)[INT] {
    IF b == 0 {
        #Console.WriteLine("Error: Division by zero")
        RETURN 0
    }
    RETURN a / b
}

FUNCTION power(INT base, INT exponent)[INT] {
    IF exponent == 0 {
        RETURN 1
    }
    
    FLEX result = 1
    CYCLE exponent AS i {
        FLEX result = result * base
    }
    RETURN result
}
```

**Using MathLib:**

```powerscript
LINK "Libs/MathLib.ps"

FLEX sum = add(10, 5)
FLEX product = multiply(6, 7)
FLEX squared = power(5, 2)

PRINT sum       // 15
PRINT product   // 42
PRINT squared   // 25
```

### 7.7 Creating String Library

**Example: StringLib.ps**

```powerscript
LINK System

FUNCTION concat(STRING a, STRING b)[STRING] {
    RETURN a + b
}

FUNCTION concatThree(STRING a, STRING b, STRING c)[STRING] {
    RETURN a + b + c
}

FUNCTION printFormatted(STRING label, STRING value)[] {
    FLEX output = label + ": " + value
    #Console.WriteLine(output)
}

FUNCTION repeat(STRING text, INT count)[STRING] {
    FLEX result = ""
    CYCLE count AS i {
        FLEX result = result + text
    }
    RETURN result
}
```

**Using StringLib:**

```powerscript
LINK "Libs/StringLib.ps"

FLEX full = concat("Hello", " World")
PRINT full  // Hello World

FLEX repeated = repeat("X", 5)
PRINT repeated  // XXXXX

printFormatted("Name", "Alice")
// Output: Name: Alice
```

### 7.8 Organizing Libraries

**Recommended Structure:**

```
project/
├── main.ps
├── Libs/
│   ├── StdLib.ps      (Standard library)
│   ├── MathLib.ps     (Math functions)
│   ├── StringLib.ps   (String utilities)
│   ├── ArrayLib.ps    (Array operations)
│   └── FileLib.ps     (File operations)
└── Scripts/
    ├── script1.ps
    └── script2.ps
```

**Libs Folder Benefits:**

1. **Auto-Copy**: Configured to copy to output directory
2. **Organization**: Keeps libraries separate from scripts
3. **Reusability**: Libraries can be used across projects
4. **Maintainability**: Easy to find and update library code

### 7.9 Library Best Practices

**1. Single Responsibility:**

Each library should focus on one area:

```powerscript
// Good: MathLib.ps contains only math functions
FUNCTION add(INT a, INT b)[INT]
FUNCTION multiply(INT a, INT b)[INT]

// Bad: Mixed responsibilities
FUNCTION add(INT a, INT b)[INT]
FUNCTION printString(STRING s)[]  // Belongs in different library
```

**2. Consistent Naming:**

```powerscript
// Good: Consistent prefixes or patterns
FUNCTION arraySum(INT arr, INT size)[INT]
FUNCTION arrayMax(INT arr, INT size)[INT]
FUNCTION arrayMin(INT arr, INT size)[INT]
```

**3. Documentation:**

```powerscript
// MathLib.ps - Mathematical utility functions
LINK System

// Calculate the factorial of n
// Returns n! (n factorial)
FUNCTION factorial(INT n)[INT] {
    IF n <= 1 {
        RETURN 1
    }
    RETURN n * factorial(n - 1)
}
```

**4. Include Necessary Links:**

```powerscript
// Always include required LINK statements in library files
LINK System

FUNCTION output(STRING message)[] {
    #Console.WriteLine(message)
}
```

### 7.10 Circular Reference Prevention

PowerScript automatically prevents circular references:

**Example:**

```powerscript
// FileA.ps
LINK "FileB.ps"

// FileB.ps
LINK "FileA.ps"  // Would create circular reference

// PowerScript prevents infinite expansion
```

**How It Works:**

- Tracks expanded files in a HashSet
- Skips files already being processed
- Prevents infinite recursion during expansion

### 7.11 Combining Namespace and File Links

**Common Pattern:**

```powerscript
// Link .NET namespaces
LINK System
LINK System.IO

// Link PowerScript libraries
LINK "Libs/StdLib.ps"
LINK "Libs/MathLib.ps"

// Now can use both .NET and PowerScript functions
#Console.WriteLine("Starting...")
FLEX result = add(10, 5)
outNum(result)
```

### 7.12 Advanced Library Example

**ArrayLib.ps:**

```powerscript
LINK System

FUNCTION arraySum(INT arr, INT size)[INT] {
    FLEX sum = 0
    CYCLE size AS i {
        FLEX sum = sum + arr[i]
    }
    RETURN sum
}

FUNCTION arrayMax(INT arr, INT size)[INT] {
    FLEX max = arr[0]
    CYCLE size AS i {
        IF arr[i] > max {
            FLEX max = arr[i]
        }
    }
    RETURN max
}

FUNCTION arrayMin(INT arr, INT size)[INT] {
    FLEX min = arr[0]
    CYCLE size AS i {
        IF arr[i] < min {
            FLEX min = arr[i]
        }
    }
    RETURN min
}

FUNCTION arrayAverage(INT arr, INT size)[INT] {
    FLEX sum = arraySum(arr, size)
    RETURN sum / size
}

FUNCTION arrayPrint(INT arr, INT size)[] {
    #Console.WriteLine("Array contents:")
    CYCLE size AS i {
        #Console.Write(arr[i])
        #Console.Write(" ")
    }
    #Console.WriteLine("")
}

FUNCTION arraySort(INT arr, INT size)[] {
    CYCLE size AS i {
        CYCLE size AS j {
            IF j + 1 < size {
                FLEX nextIdx = j + 1
                IF arr[j] > arr[nextIdx] {
                    FLEX temp = arr[j]
                    FLEX arr[j] = arr[nextIdx]
                    FLEX arr[nextIdx] = temp
                }
            }
        }
    }
}
```

**Using ArrayLib:**

```powerscript
LINK "Libs/ArrayLib.ps"

FLEX numbers = [64, 34, 25, 12, 22]
FLEX size = 5

arrayPrint(numbers, size)

FLEX sum = arraySum(numbers, size)
FLEX max = arrayMax(numbers, size)
FLEX min = arrayMin(numbers, size)
FLEX avg = arrayAverage(numbers, size)

outStr("Sum: ")
outNum(sum)
outStr("Max: ")
outNum(max)
outStr("Min: ")
outNum(min)
outStr("Average: ")
outNum(avg)

arraySort(numbers, size)
outStr("Sorted:")
arrayPrint(numbers, size)
```

### 7.13 Library Versioning

**Best Practice: Version Comments:**

```powerscript
// MathLib.ps
// Version: 1.0.0
// Last Updated: 2025-10-12
// Description: Mathematical utility functions

LINK System

FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}
```

### 7.14 File Link Debugging

**Debug Output:**

When files are linked, PowerScript outputs:

```
[LINK] Starting file expansion preprocessing...
[LINK] Expanding file: Libs/StdLib.ps
[LINK] File expansion completed. Linked 1 file(s).
```

**Comment Markers in Expanded Code:**

```powerscript
// === Linked from: Libs/StdLib.ps ===
LINK System
FUNCTION out(INT value)[] {
    #Console.WriteLine(value)
}
// === End of: Libs/StdLib.ps ===
```

### 7.15 Module Pattern Summary

**Typical Usage Pattern:**

```powerscript
// 1. Link .NET namespaces
LINK System
LINK System.IO

// 2. Link standard library
LINK "Libs/StdLib.ps"

// 3. Link specialized libraries
LINK "Libs/MathLib.ps"
LINK "Libs/ArrayLib.ps"

// 4. Use library functions
FLEX data = [10, 20, 30, 40, 50]
FLEX sum = arraySum(data, 5)
outNum(sum)
```
