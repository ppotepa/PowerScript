# PowerScript Language Reference Manual
## Part 9: Best Practices and Style Guide

### 9.1 Code Organization

#### 9.1.1 File Structure

Organize your PowerScript files in a consistent manner:

```powerscript
// 1. LINK statements (namespaces first, then files)
LINK System
LINK System.IO
LINK "Libs/StdLib.ps"

// 2. Function declarations
FUNCTION helper(INT value)[INT] {
    // Implementation
}

// 3. Main execution code
FLEX result = helper(10)
PRINT result
```

#### 9.1.2 Project Structure

Recommended directory layout:

```
project/
├── main.ps              # Main entry point
├── Libs/                # Reusable libraries
│   ├── StdLib.ps
│   ├── MathLib.ps
│   └── ArrayLib.ps
├── Scripts/             # Application scripts
│   ├── process.ps
│   └── analyze.ps
└── Tests/               # Test scripts
    └── test_math.ps
```

### 9.2 Naming Conventions

#### 9.2.1 Variables

Use descriptive, camelCase or snake_case names:

```powerscript
// Good
FLEX userAge = 25
FLEX totalCount = 100
FLEX is_active = TRUE

// Avoid
FLEX x = 25
FLEX tmp = 100
FLEX a = TRUE
```

#### 9.2.2 Functions

Use descriptive verb-based names:

```powerscript
// Good
FUNCTION calculateTotal(INT price, INT quantity)[INT]
FUNCTION validateInput(STRING input)[BOOL]
FUNCTION printReport(STRING title)[]

// Avoid
FUNCTION calc(INT p, INT q)[INT]
FUNCTION check(STRING s)[BOOL]
FUNCTION pr(STRING t)[]
```

#### 9.2.3 Constants

Use UPPERCASE for values that don't change:

```powerscript
FLEX MAX_RETRIES = 3
FLEX DEFAULT_TIMEOUT = 30
FLEX APP_VERSION = "1.0.0"
```

### 9.3 Code Formatting

#### 9.3.1 Indentation

Use consistent indentation (4 spaces recommended):

```powerscript
FUNCTION process(INT value)[INT] {
    IF value > 0 {
        FLEX result = value * 2
        RETURN result
    }
    RETURN 0
}
```

#### 9.3.2 Spacing

Use spaces for readability:

```powerscript
// Good
FLEX sum = a + b
FLEX result = (x * 2) + (y / 3)

// Less readable
FLEX sum=a+b
FLEX result=(x*2)+(y/3)
```

#### 9.3.3 Line Length

Keep lines under 80-100 characters:

```powerscript
// Good
FLEX message = "This is a long message that " +
               "spans multiple lines for readability"

// Avoid very long lines
FLEX message = "This is an extremely long message that goes on and on and makes the code hard to read"
```

### 9.4 Comments

#### 9.4.1 File Headers

Include file purpose and metadata:

```powerscript
// MathLib.ps
// Purpose: Mathematical utility functions
// Version: 1.0.0
// Author: PowerScript Team
// Last Updated: 2025-10-12

LINK System

FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}
```

#### 9.4.2 Function Documentation

Document function purpose and parameters:

```powerscript
// Calculate the factorial of n
// Parameters:
//   n: The number to calculate factorial for
// Returns:
//   The factorial of n (n!)
FUNCTION factorial(INT n)[INT] {
    IF n <= 1 {
        RETURN 1
    }
    RETURN n * factorial(n - 1)
}
```

#### 9.4.3 Inline Comments

Explain complex logic:

```powerscript
FUNCTION bubbleSort(INT arr, INT size)[] {
    CYCLE size AS i {
        // Inner loop compares adjacent elements
        CYCLE size AS j {
            FLEX nextIdx = j + 1
            
            // Swap if current element is greater than next
            IF j < size - 1 AND arr[j] > arr[nextIdx] {
                FLEX temp = arr[j]
                FLEX arr[j] = arr[nextIdx]
                FLEX arr[nextIdx] = temp
            }
        }
    }
}
```

### 9.5 Error Handling

#### 9.5.1 Validate Input

Check inputs before processing:

```powerscript
FUNCTION divide(INT a, INT b)[INT] {
    IF b == 0 {
        PRINT "Error: Division by zero"
        RETURN 0
    }
    RETURN a / b
}
```

#### 9.5.2 Array Bounds

Verify array indices:

```powerscript
FUNCTION getElement(INT arr, INT index, INT size)[INT] {
    IF index < 0 OR index >= size {
        PRINT "Error: Index out of bounds"
        RETURN -1
    }
    RETURN arr[index]
}
```

#### 9.5.3 Meaningful Error Messages

Provide clear error information:

```powerscript
FUNCTION processAge(INT age)[BOOL] {
    IF age < 0 {
        PRINT "Error: Age cannot be negative"
        RETURN FALSE
    }
    
    IF age > 150 {
        PRINT "Error: Age exceeds maximum expected value"
        RETURN FALSE
    }
    
    RETURN TRUE
}
```

### 9.6 Function Design

#### 9.6.1 Single Responsibility

Each function should do one thing well:

```powerscript
// Good: Focused functions
FUNCTION calculateTax(INT amount)[INT] {
    RETURN amount * 8 / 100
}

FUNCTION calculateTotal(INT subtotal, INT tax)[INT] {
    RETURN subtotal + tax
}

// Avoid: Doing too much
FUNCTION processOrder(INT price, INT quantity)[INT] {
    FLEX subtotal = price * quantity
    FLEX tax = subtotal * 8 / 100
    FLEX total = subtotal + tax
    FLEX discount = total * 10 / 100
    FLEX final = total - discount
    RETURN final
}
```

#### 9.6.2 Function Length

Keep functions concise (20-30 lines max):

```powerscript
// Good: Short, focused function
FUNCTION isEven(INT num)[BOOL] {
    RETURN num % 2 == 0
}

// If a function grows too large, break it down
FUNCTION processData(INT arr, INT size)[] {
    validateData(arr, size)
    sortData(arr, size)
    analyzeData(arr, size)
    displayResults(arr, size)
}
```

#### 9.6.3 Parameter Count

Limit parameters (3-4 maximum):

```powerscript
// Good
FUNCTION createPoint(INT x, INT y)[INT]

// Acceptable
FUNCTION createRect(INT x, INT y, INT width, INT height)[INT]

// Consider refactoring if more parameters needed
// Instead of: FUNCTION complex(INT a, INT b, INT c, INT d, INT e, INT f)
// Use: Pass array or break into smaller functions
```

### 9.7 Code Clarity

#### 9.7.1 Use Intermediate Variables

Break complex expressions:

```powerscript
// Good: Clear intent
FLEX subtotal = price * quantity
FLEX tax = subtotal * taxRate / 100
FLEX total = subtotal + tax

// Less clear
FLEX total = price * quantity + (price * quantity * taxRate / 100)
```

#### 9.7.2 Meaningful Boolean Names

Use positive, descriptive names:

```powerscript
// Good
FLEX isValid = TRUE
FLEX hasPermission = TRUE
FLEX canProceed = TRUE

// Avoid
FLEX flag = TRUE
FLEX check = TRUE
FLEX ok = TRUE
```

#### 9.7.3 Guard Clauses

Handle edge cases early:

```powerscript
// Good: Early return pattern
FUNCTION process(INT value)[INT] {
    IF value < 0 {
        RETURN 0  // Guard clause
    }
    
    // Main logic
    FLEX result = value * 2
    RETURN result
}

// Avoid: Deep nesting
FUNCTION process(INT value)[INT] {
    IF value >= 0 {
        FLEX result = value * 2
        RETURN result
    }
    RETURN 0
}
```

### 9.8 Performance Considerations

#### 9.8.1 Avoid Redundant Calculations

```powerscript
// Good: Calculate once
FLEX size = 10
CYCLE size AS i {
    // Use i
}

// Avoid: Recalculating
CYCLE 10 AS i {
    FLEX size = 10  // Don't recalculate inside loop
}
```

#### 9.8.2 Efficient Loops

```powerscript
// Good: Efficient iteration
FLEX arr = [1, 2, 3, 4, 5]
FLEX size = 5

CYCLE size AS i {
    PRINT arr[i]
}

// Avoid: Unnecessary work
CYCLE 100 AS i {
    IF i < 5 {
        PRINT arr[i]
    }
}
```

### 9.9 Testing and Debugging

#### 9.9.1 Write Testable Code

```powerscript
// Good: Easy to test
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

// Test
FLEX result = add(2, 3)
IF result == 5 {
    PRINT "Test passed"
}
```

#### 9.9.2 Debug Output

Use debug functions:

```powerscript
LINK "Libs/StdLib.ps"

FUNCTION debugPrint(STRING label, INT value)[] {
    outStr("[DEBUG] " + label + " = ")
    outNum(value)
}

FLEX x = 10
debugPrint("x", x)
```

#### 9.9.3 Assertions

Validate assumptions:

```powerscript
FUNCTION processPositive(INT value)[INT] {
    // Assert value is positive
    IF value <= 0 {
        PRINT "Assertion failed: value must be positive"
        RETURN -1
    }
    
    RETURN value * 2
}
```

### 9.10 Common Patterns

#### 9.10.1 Accumulator Pattern

```powerscript
FLEX sum = 0
FLEX arr = [1, 2, 3, 4, 5]

CYCLE 5 AS i {
    FLEX sum = sum + arr[i]
}
```

#### 9.10.2 Filter Pattern

```powerscript
FLEX evenCount = 0
FLEX arr = [1, 2, 3, 4, 5, 6]

CYCLE 6 AS i {
    IF arr[i] % 2 == 0 {
        FLEX evenCount = evenCount + 1
    }
}
```

#### 9.10.3 Map Pattern

```powerscript
FLEX original = [1, 2, 3, 4, 5]
FLEX doubled = [0, 0, 0, 0, 0]

CYCLE 5 AS i {
    FLEX doubled[i] = original[i] * 2
}
```

### 9.11 Code Review Checklist

- [ ] Descriptive variable and function names
- [ ] Consistent indentation and spacing
- [ ] Comments for complex logic
- [ ] Input validation where needed
- [ ] No magic numbers (use named constants)
- [ ] Functions have single responsibility
- [ ] No deep nesting (max 3 levels)
- [ ] Error cases handled
- [ ] Code is DRY (Don't Repeat Yourself)
- [ ] LINK statements at top of file

### 9.12 Anti-Patterns to Avoid

**1. Magic Numbers:**
```powerscript
// Bad
IF score > 60 {
    PRINT "Pass"
}

// Good
FLEX PASSING_SCORE = 60
IF score > PASSING_SCORE {
    PRINT "Pass"
}
```

**2. Copy-Paste Code:**
```powerscript
// Bad: Duplicated logic
FLEX sum1 = arr1[0] + arr1[1] + arr1[2]
FLEX sum2 = arr2[0] + arr2[1] + arr2[2]

// Good: Reusable function
FUNCTION sum3(INT arr)[INT] {
    RETURN arr[0] + arr[1] + arr[2]
}
```

**3. Deep Nesting:**
```powerscript
// Bad
IF condition1 {
    IF condition2 {
        IF condition3 {
            // Too deep
        }
    }
}

// Good
IF NOT condition1 {
    RETURN
}
IF NOT condition2 {
    RETURN
}
IF NOT condition3 {
    RETURN
}
// Main logic
```
