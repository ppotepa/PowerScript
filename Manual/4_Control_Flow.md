# PowerScript Language Reference Manual
## Part 4: Control Flow

### 4.1 Control Flow Overview

PowerScript provides conditional statements and loops to control program execution flow.

### 4.2 IF Statement

Execute code conditionally based on a boolean expression.

**Syntax:**

```powerscript
IF condition {
    // statements
}
```

**Examples:**

```powerscript
FLEX age = 18

IF age >= 18 {
    PRINT "Adult"
}
```

**With Multiple Statements:**

```powerscript
FLEX score = 85

IF score >= 60 {
    PRINT "Passed!"
    FLEX grade = "Pass"
    PRINT grade
}
```

**Nested IF Statements:**

```powerscript
FLEX age = 25
FLEX hasLicense = TRUE

IF age >= 18 {
    IF hasLicense {
        PRINT "Can drive"
    }
}
```

### 4.3 Comparison in Conditionals

**Numeric Comparisons:**

```powerscript
FLEX x = 10

IF x > 5 {
    PRINT "Greater than 5"
}

IF x == 10 {
    PRINT "Exactly 10"
}

IF x != 0 {
    PRINT "Not zero"
}
```

**Boolean Conditions:**

```powerscript
FLEX isActive = TRUE

IF isActive {
    PRINT "System is active"
}

IF NOT isActive {
    PRINT "System is inactive"
}
```

**Complex Conditions:**

```powerscript
FLEX temperature = 25
FLEX humidity = 60

IF temperature > 20 AND humidity < 70 {
    PRINT "Comfortable conditions"
}

FLEX hour = 14
IF hour < 12 OR hour > 18 {
    PRINT "Outside business hours"
}
```

### 4.4 CYCLE Loop

The `CYCLE` statement repeats code a fixed number of times.

**Syntax:**

```powerscript
CYCLE count AS iterator {
    // statements using iterator
}
```

**Parameters:**
- `count`: Number of iterations (INT expression)
- `iterator`: Loop variable (0 to count-1)

**Basic Examples:**

```powerscript
// Print numbers 0 to 4
CYCLE 5 AS i {
    PRINT i
}
// Output: 0, 1, 2, 3, 4
```

**Using Loop Variable:**

```powerscript
FLEX sum = 0
CYCLE 10 AS i {
    FLEX sum = sum + i
}
PRINT sum  // 45 (0+1+2+...+9)
```

**Counting Pattern:**

```powerscript
CYCLE 5 AS i {
    FLEX num = i + 1  // Start from 1 instead of 0
    PRINT num
}
// Output: 1, 2, 3, 4, 5
```

### 4.5 Nested Loops

Loops can be nested for multi-dimensional iteration.

**Basic Nested Loop:**

```powerscript
CYCLE 3 AS i {
    CYCLE 3 AS j {
        PRINT i
        PRINT j
    }
}
```

**Multiplication Table:**

```powerscript
CYCLE 10 AS i {
    CYCLE 10 AS j {
        FLEX result = (i + 1) * (j + 1)
        PRINT result
    }
}
```

**2D Array Processing:**

```powerscript
// Print all elements in a 2D concept
CYCLE 3 AS row {
    CYCLE 4 AS col {
        FLEX index = row * 4 + col
        PRINT index
    }
}
```

### 4.6 Loops with Conditionals

Combine loops and conditionals for selective processing.

**Filter Pattern:**

```powerscript
CYCLE 10 AS i {
    FLEX num = i + 1
    
    IF num % 2 == 0 {
        PRINT num  // Print only even numbers
    }
}
// Output: 2, 4, 6, 8, 10
```

**Range Check:**

```powerscript
FLEX arr = [5, 15, 25, 35, 45]

CYCLE 5 AS i {
    IF arr[i] >= 20 AND arr[i] <= 40 {
        PRINT arr[i]
    }
}
// Output: 25, 35
```

**Nested with Condition:**

```powerscript
CYCLE 5 AS i {
    CYCLE 5 AS j {
        IF i == j {
            PRINT i  // Print diagonal
        }
    }
}
```

### 4.7 Array Iteration

Use CYCLE to iterate over arrays.

**Sequential Access:**

```powerscript
FLEX numbers = [10, 20, 30, 40, 50]

CYCLE 5 AS i {
    PRINT numbers[i]
}
```

**Array Transformation:**

```powerscript
FLEX arr = [1, 2, 3, 4, 5]

CYCLE 5 AS i {
    FLEX arr[i] = arr[i] * 2
}
// arr is now [2, 4, 6, 8, 10]
```

**Array Search:**

```powerscript
FLEX arr = [10, 25, 30, 45, 50]
FLEX target = 30
FLEX found = FALSE

CYCLE 5 AS i {
    IF arr[i] == target {
        FLEX found = TRUE
        PRINT "Found at index: "
        PRINT i
    }
}
```

### 4.8 Counter Patterns

**Simple Counter:**

```powerscript
FLEX count = 0

CYCLE 10 AS i {
    FLEX count = count + 1
}
PRINT count  // 10
```

**Conditional Counter:**

```powerscript
FLEX arr = [5, 12, 8, 15, 20, 3, 18]
FLEX count = 0

CYCLE 7 AS i {
    IF arr[i] >= 10 {
        FLEX count = count + 1
    }
}
PRINT count  // 4 (values >= 10)
```

**Multiple Counters:**

```powerscript
FLEX positiveCount = 0
FLEX negativeCount = 0
FLEX arr = [5, -3, 8, -1, 0, 7, -9]

CYCLE 7 AS i {
    IF arr[i] > 0 {
        FLEX positiveCount = positiveCount + 1
    }
    IF arr[i] < 0 {
        FLEX negativeCount = negativeCount + 1
    }
}
```

### 4.9 Accumulator Patterns

**Sum Accumulator:**

```powerscript
FLEX numbers = [5, 10, 15, 20, 25]
FLEX sum = 0

CYCLE 5 AS i {
    FLEX sum = sum + numbers[i]
}
PRINT sum  // 75
```

**Product Accumulator:**

```powerscript
FLEX factorial = 1

CYCLE 5 AS i {
    FLEX num = i + 1
    FLEX factorial = factorial * num
}
PRINT factorial  // 120 (5!)
```

**String Accumulator:**

```powerscript
FLEX result = ""

CYCLE 5 AS i {
    FLEX result = result + "X"
}
PRINT result  // "XXXXX"
```

### 4.10 Algorithmic Patterns

**Find Maximum:**

```powerscript
FLEX arr = [23, 45, 12, 67, 34, 89, 56]
FLEX max = arr[0]

CYCLE 7 AS i {
    IF arr[i] > max {
        FLEX max = arr[i]
    }
}
PRINT max  // 89
```

**Find Minimum:**

```powerscript
FLEX arr = [23, 45, 12, 67, 34, 89, 56]
FLEX min = arr[0]

CYCLE 7 AS i {
    IF arr[i] < min {
        FLEX min = arr[i]
    }
}
PRINT min  // 12
```

**Bubble Sort:**

```powerscript
FLEX arr = [64, 34, 25, 12, 22]
FLEX size = 5

CYCLE size AS i {
    CYCLE 4 AS j {
        FLEX nextIdx = j + 1
        
        IF arr[j] > arr[nextIdx] {
            FLEX temp = arr[j]
            FLEX arr[j] = arr[nextIdx]
            FLEX arr[nextIdx] = temp
        }
    }
}
// arr is now sorted: [12, 22, 25, 34, 64]
```

**Prime Number Check:**

```powerscript
FLEX num = 17
FLEX isPrime = TRUE

IF num <= 1 {
    FLEX isPrime = FALSE
}

CYCLE num AS i {
    IF i > 1 AND i < num {
        IF num % i == 0 {
            FLEX isPrime = FALSE
        }
    }
}
PRINT isPrime  // TRUE
```

### 4.11 Control Flow Best Practices

**1. Keep Conditions Simple:**

```powerscript
// Good
FLEX isAdult = age >= 18
IF isAdult {
    // ...
}

// Less clear
IF age >= 18 {
    // ...
}
```

**2. Avoid Deep Nesting:**

```powerscript
// Avoid
IF condition1 {
    IF condition2 {
        IF condition3 {
            // ...
        }
    }
}

// Better: Use combined conditions
IF condition1 AND condition2 AND condition3 {
    // ...
}
```

**3. Use Meaningful Loop Variables:**

```powerscript
// Good
CYCLE 10 AS index {
    PRINT arr[index]
}

// Also good for specific contexts
CYCLE 5 AS row {
    CYCLE 5 AS col {
        // Matrix operation
    }
}
```

**4. Initialize Before Loops:**

```powerscript
// Good
FLEX sum = 0
FLEX count = 0

CYCLE 10 AS i {
    FLEX sum = sum + i
    FLEX count = count + 1
}
```

**5. Use Early Conditions:**

```powerscript
// Good: Check condition early
IF arr[i] == target {
    PRINT "Found!"
    // Process...
}

// Process other cases...
```

### 4.12 Common Pitfalls

**1. Off-by-One Errors:**

```powerscript
// CYCLE 5 iterates 0-4, not 1-5
CYCLE 5 AS i {
    PRINT i  // 0, 1, 2, 3, 4
}

// To iterate 1-5:
CYCLE 5 AS i {
    FLEX num = i + 1
    PRINT num  // 1, 2, 3, 4, 5
}
```

**2. Array Bounds:**

```powerscript
FLEX arr = [10, 20, 30]

// Correct: Array has 3 elements (indices 0-2)
CYCLE 3 AS i {
    PRINT arr[i]
}

// Incorrect: Would cause out-of-bounds error
// CYCLE 4 AS i {
//     PRINT arr[i]  // Error at i=3
// }
```

**3. Infinite Logic:**

```powerscript
// Be careful with conditions that might never be true
FLEX found = FALSE

CYCLE 100 AS i {
    IF someCondition {
        FLEX found = TRUE
        // Consider: Should we stop searching?
    }
}
```
