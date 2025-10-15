# PowerScript Quick Reference Card

## Essential Syntax at a Glance

### Variables
```powerscript
FLEX variableName = value
FLEX x = 10                    // Integer
FLEX name = "Alice"            // String
FLEX isActive = TRUE           // Boolean
FLEX arr = [1, 2, 3, 4, 5]    // Array
```

### Data Types
| Type | Example | Description |
|------|---------|-------------|
| INT | `42` | Integer numbers |
| STRING | `"Hello"` | Text strings |
| BOOL | `TRUE`, `FALSE` | Boolean values |
| ARRAY | `[1, 2, 3]` | Collections |

### Operators
```powerscript
// Arithmetic
x + y    x - y    x * y    x / y    x % y

// Comparison
x == y   x != y   x < y    x > y    x <= y   x >= y

// Logical
a AND b    a OR b    NOT a

// Assignment
x = value
```

### Control Flow
```powerscript
// IF statement
IF condition {
    // statements
}

// CYCLE loop
CYCLE count AS i {
    // statements using i
}

// Nested
CYCLE 5 AS i {
    IF i % 2 == 0 {
        PRINT i
    }
}
```

### Functions
```powerscript
// Void function
FUNCTION greet(STRING name)[] {
    PRINT "Hello, " + name
}

// Function with return
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

// Call function
FLEX result = add(10, 5)
greet("Alice")
```

### Arrays
```powerscript
// Create array
FLEX arr = [10, 20, 30, 40, 50]

// Access element
FLEX value = arr[0]        // 10

// Modify element
FLEX arr[2] = 999

// Iterate
CYCLE 5 AS i {
    PRINT arr[i]
}
```

### File Linking
```powerscript
// Link .NET namespace
LINK System

// Link PowerScript file
LINK "Libs/StdLib.ps"

// Link multiple
LINK System.IO
LINK "Libs/MathLib.ps"
```

### .NET Interoperability
```powerscript
// Full syntax
NET::System.Console.WriteLine("Text")

// Short syntax (requires LINK)
LINK System
#Console.WriteLine("Text")
#Math.Max(10, 20)
```

### Standard Library Functions
```powerscript
LINK "Libs/StdLib.ps"

out(value)              // Output value
outln(value)            // Output with newline
outNum(number)          // Output number
outStr("text")          // Output string
outMulti(a, b)          // Output two values
outThree(a, b, c)       // Output three values
newline(0)              // Print newline
space(0)                // Print space
```

### Comments
```powerscript
// Single-line comment

/* Multi-line
   comment */
```

### Common Patterns

#### Counter Pattern
```powerscript
FLEX count = 0
CYCLE 10 AS i {
    FLEX count = count + 1
}
```

#### Accumulator Pattern
```powerscript
FLEX sum = 0
FLEX arr = [1, 2, 3, 4, 5]
CYCLE 5 AS i {
    FLEX sum = sum + arr[i]
}
```

#### Find Maximum
```powerscript
FLEX arr = [23, 45, 12, 67, 34]
FLEX max = arr[0]
CYCLE 5 AS i {
    IF arr[i] > max {
        FLEX max = arr[i]
    }
}
```

#### Bubble Sort
```powerscript
FLEX arr = [64, 34, 25, 12, 22]
CYCLE 5 AS i {
    CYCLE 4 AS j {
        FLEX next = j + 1
        IF arr[j] > arr[next] {
            FLEX temp = arr[j]
            FLEX arr[j] = arr[next]
            FLEX arr[next] = temp
        }
    }
}
```

### Program Template
```powerscript
// 1. Link statements
LINK System
LINK "Libs/StdLib.ps"

// 2. Function declarations
FUNCTION helper(INT value)[INT] {
    RETURN value * 2
}

// 3. Main execution
FLEX x = 10
FLEX result = helper(x)
outNum(result)
```

### Operator Precedence (Highest to Lowest)
1. Parentheses `( )`
2. Unary `NOT`, `-`
3. Multiplicative `*`, `/`, `%`
4. Additive `+`, `-`
5. Comparison `<`, `>`, `<=`, `>=`
6. Equality `==`, `!=`
7. Logical AND `AND`
8. Logical OR `OR`
9. Assignment `=`

### Keywords Reference
| Category | Keywords |
|----------|----------|
| Control | `IF`, `CYCLE`, `AS` |
| Functions | `FUNCTION`, `RETURN` |
| Variables | `FLEX` |
| Types | `INT`, `STRING`, `BOOL`, `ARRAY` |
| Operators | `AND`, `OR`, `NOT` |
| I/O | `PRINT` |
| Interop | `LINK`, `NET::`, `EXECUTE` |
| Values | `TRUE`, `FALSE` |

### File Structure
```
project/
├── main.ps              # Main script
├── Libs/                # Libraries (auto-copied)
│   ├── StdLib.ps
│   ├── MathLib.ps
│   └── ArrayLib.ps
└── Scripts/
    └── helper.ps
```

### Common Error Fixes

**Missing Return:**
```powerscript
// ❌ Error
FUNCTION add(INT a, INT b)[INT] {
    FLEX sum = a + b
}

// ✅ Fixed
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}
```

**Array Out of Bounds:**
```powerscript
// ❌ Error
FLEX arr = [1, 2, 3]
FLEX value = arr[5]  // Only 0-2 valid

// ✅ Fixed
IF index >= 0 AND index < 3 {
    FLEX value = arr[index]
}
```

**Type Mismatch:**
```powerscript
// ❌ Error
FUNCTION getName()[INT] {
    RETURN "Alice"
}

// ✅ Fixed
FUNCTION getName()[STRING] {
    RETURN "Alice"
}
```

### Tips & Tricks

**Use Meaningful Names:**
```powerscript
// ✅ Good
FLEX userAge = 25
FLEX isActive = TRUE

// ❌ Avoid
FLEX x = 25
FLEX flag = TRUE
```

**Break Complex Expressions:**
```powerscript
// ✅ Good
FLEX subtotal = price * quantity
FLEX tax = subtotal * 8 / 100
FLEX total = subtotal + tax

// ❌ Avoid
FLEX total = price * quantity + (price * quantity * 8 / 100)
```

**Use Guard Clauses:**
```powerscript
// ✅ Good
FUNCTION divide(INT a, INT b)[INT] {
    IF b == 0 {
        RETURN 0
    }
    RETURN a / b
}
```

### Running PowerScript
```bash
# Execute a script
powerscript.exe script.ps

# Build project
dotnet build

# Run from output directory
cd bin/Debug/net8.0
powerscript.exe script.ps
```

---

**For complete documentation, see the [Full Reference Manual](README.md)**

**Quick Links:**
- [Introduction](1_Introduction.md)
- [Data Types](2_DataTypes_Variables.md)
- [Control Flow](4_Control_Flow.md)
- [Functions](5_Functions.md)
- [Standard Library](8_Standard_Library.md)
- [Complete Index](INDEX.md)
