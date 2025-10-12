# PowerScript Language Reference Manual
## Part 3: Operators and Expressions

### 3.1 Operators Overview

PowerScript provides a rich set of operators for arithmetic, comparison, logical, and assignment operations.

### 3.2 Arithmetic Operators

| Operator | Description | Example | Result |
|----------|-------------|---------|--------|
| `+` | Addition | `5 + 3` | `8` |
| `-` | Subtraction | `10 - 4` | `6` |
| `*` | Multiplication | `6 * 7` | `42` |
| `/` | Division | `20 / 4` | `5` |
| `%` | Modulo (remainder) | `17 % 5` | `2` |

**Examples:**

```powerscript
FLEX a = 10
FLEX b = 3

FLEX sum = a + b         // 13
FLEX difference = a - b  // 7
FLEX product = a * b     // 30
FLEX quotient = a / b    // 3
FLEX remainder = a % b   // 1
```

**Division Behavior:**

```powerscript
FLEX x = 10 / 3      // Integer division: 3
FLEX y = 10 / 4      // Integer division: 2
FLEX z = 10 / 2      // Exact division: 5
```

### 3.3 Comparison Operators

| Operator | Description | Example | Result |
|----------|-------------|---------|--------|
| `==` | Equal to | `5 == 5` | `TRUE` |
| `!=` | Not equal to | `5 != 3` | `TRUE` |
| `<` | Less than | `3 < 5` | `TRUE` |
| `>` | Greater than | `5 > 3` | `TRUE` |
| `<=` | Less than or equal | `3 <= 3` | `TRUE` |
| `>=` | Greater than or equal | `5 >= 3` | `TRUE` |

**Examples:**

```powerscript
FLEX x = 10
FLEX y = 20

FLEX isEqual = x == y        // FALSE
FLEX notEqual = x != y       // TRUE
FLEX isLess = x < y          // TRUE
FLEX isGreater = x > y       // FALSE
FLEX lessOrEqual = x <= 10   // TRUE
FLEX greaterOrEqual = y >= 20 // TRUE
```

**String Comparison:**

```powerscript
FLEX name1 = "Alice"
FLEX name2 = "Bob"

FLEX same = name1 == name2   // FALSE
FLEX different = name1 != name2  // TRUE
```

### 3.4 Logical Operators

| Operator | Description | Example | Result |
|----------|-------------|---------|--------|
| `AND` | Logical AND | `TRUE AND FALSE` | `FALSE` |
| `OR` | Logical OR | `TRUE OR FALSE` | `TRUE` |
| `NOT` | Logical NOT | `NOT TRUE` | `FALSE` |

**Truth Tables:**

**AND Operator:**
```powerscript
TRUE AND TRUE    // TRUE
TRUE AND FALSE   // FALSE
FALSE AND TRUE   // FALSE
FALSE AND FALSE  // FALSE
```

**OR Operator:**
```powerscript
TRUE OR TRUE     // TRUE
TRUE OR FALSE    // TRUE
FALSE OR TRUE    // TRUE
FALSE OR FALSE   // FALSE
```

**NOT Operator:**
```powerscript
NOT TRUE         // FALSE
NOT FALSE        // TRUE
```

**Examples:**

```powerscript
FLEX age = 25
FLEX hasLicense = TRUE

FLEX canDrive = age >= 18 AND hasLicense  // TRUE

FLEX x = 10
FLEX isInRange = x >= 0 AND x <= 100      // TRUE

FLEX a = 5
FLEX b = 10
FLEX isValid = a > 0 OR b > 0             // TRUE
```

### 3.5 Assignment Operator

| Operator | Description | Example |
|----------|-------------|---------|
| `=` | Assignment | `x = 10` |

**Examples:**

```powerscript
FLEX x = 5               // Simple assignment
FLEX y = x               // Assign from variable
FLEX z = x + y           // Assign from expression
FLEX arr[0] = 100        // Array element assignment
```

**Chained Assignments:**

```powerscript
FLEX a = 10
FLEX b = a
FLEX c = b
// a, b, c all equal 10
```

### 3.6 Operator Precedence

Operators are evaluated in the following order (highest to lowest):

1. **Parentheses**: `( )`
2. **Unary**: `NOT`, `-` (negation)
3. **Multiplicative**: `*`, `/`, `%`
4. **Additive**: `+`, `-`
5. **Comparison**: `<`, `>`, `<=`, `>=`
6. **Equality**: `==`, `!=`
7. **Logical AND**: `AND`
8. **Logical OR**: `OR`
9. **Assignment**: `=`

**Examples:**

```powerscript
FLEX result = 5 + 3 * 2
// Evaluates as: 5 + (3 * 2) = 11

FLEX result2 = (5 + 3) * 2
// Evaluates as: (8) * 2 = 16

FLEX bool1 = 10 > 5 AND 20 < 30
// Evaluates as: TRUE AND TRUE = TRUE
```

### 3.7 Parentheses and Grouping

Use parentheses to control evaluation order:

```powerscript
// Without parentheses
FLEX a = 5 + 3 * 2      // 11 (multiplication first)

// With parentheses
FLEX b = (5 + 3) * 2    // 16 (addition first)

// Nested parentheses
FLEX c = ((2 + 3) * (4 + 1)) - ((10 - 2) / 2)
// (5 * 5) - (8 / 2) = 25 - 4 = 21
```

**Complex Logical Expressions:**

```powerscript
FLEX x = 10
FLEX y = 20
FLEX z = 15

// Without parentheses (may be ambiguous)
FLEX result1 = x < y OR x > z AND y > z

// With parentheses (clear intent)
FLEX result2 = (x < y) OR ((x > z) AND (y > z))
```

### 3.8 Expression Types

**Literal Expressions:**

```powerscript
FLEX num = 42               // Integer literal
FLEX text = "Hello"         // String literal
FLEX flag = TRUE            // Boolean literal
FLEX arr = [1, 2, 3]        // Array literal
```

**Identifier Expressions:**

```powerscript
FLEX x = 10
FLEX y = x                  // Identifier expression
```

**Binary Expressions:**

```powerscript
FLEX sum = 5 + 3            // Addition
FLEX product = 4 * 7        // Multiplication
FLEX comparison = 10 > 5    // Comparison
FLEX logical = TRUE AND FALSE  // Logical
```

**Index Expressions:**

```powerscript
FLEX arr = [10, 20, 30]
FLEX value = arr[1]         // Index expression: 20
```

**Function Call Expressions:**

```powerscript
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

FLEX result = add(5, 3)     // Function call expression: 8
```

### 3.9 String Concatenation

Use `+` operator to concatenate strings:

```powerscript
FLEX firstName = "John"
FLEX lastName = "Doe"
FLEX fullName = firstName + " " + lastName
PRINT fullName  // "John Doe"

// Mixing strings and numbers
FLEX age = 30
FLEX message = "Age: " + age
PRINT message   // "Age: 30"
```

### 3.10 Complex Expressions

**Nested Arithmetic:**

```powerscript
FLEX result = ((10 + 5) * 3 - (20 / 4)) * 2
// ((15) * 3 - (5)) * 2
// (45 - 5) * 2
// 40 * 2
// 80
```

**Combined Logical:**

```powerscript
FLEX age = 25
FLEX hasID = TRUE
FLEX hasTicket = TRUE

FLEX canEnter = (age >= 18) AND (hasID OR hasTicket)
// TRUE AND (TRUE OR TRUE)
// TRUE AND TRUE
// TRUE
```

**Array Expressions:**

```powerscript
FLEX arr = [10, 20, 30, 40, 50]
FLEX index = 2
FLEX doubled = arr[index] * 2        // 60
FLEX sum = arr[0] + arr[1] + arr[2]  // 60
```

### 3.11 Short-Circuit Evaluation

PowerScript evaluates logical expressions with short-circuit behavior:

**AND Operator:**
```powerscript
// If first operand is FALSE, second is not evaluated
FLEX result = FALSE AND someFunction()
// someFunction() is never called
```

**OR Operator:**
```powerscript
// If first operand is TRUE, second is not evaluated
FLEX result = TRUE OR someFunction()
// someFunction() is never called
```

### 3.12 Common Expression Patterns

**Calculation Pattern:**

```powerscript
FLEX price = 100
FLEX taxRate = 0.08
FLEX tax = price * taxRate
FLEX total = price + tax
PRINT total  // 108
```

**Conditional Expression:**

```powerscript
FLEX score = 85
FLEX passed = score >= 60
PRINT passed  // TRUE
```

**Range Check:**

```powerscript
FLEX temperature = 25
FLEX comfortable = temperature >= 20 AND temperature <= 26
PRINT comfortable  // TRUE
```

**Array Calculation:**

```powerscript
FLEX numbers = [5, 10, 15, 20, 25]
FLEX sum = 0

CYCLE 5 AS i {
    FLEX sum = sum + numbers[i]
}
PRINT sum  // 75
```

### 3.13 Best Practices

1. **Use parentheses for clarity**: Even when not required
   ```powerscript
   FLEX result = (a + b) * c  // Clear
   ```

2. **Break complex expressions**: Into multiple steps
   ```powerscript
   // Instead of one complex line
   FLEX subtotal = price * quantity
   FLEX tax = subtotal * taxRate
   FLEX total = subtotal + tax
   ```

3. **Avoid deep nesting**: Keep expressions readable
   ```powerscript
   // Hard to read
   FLEX x = ((a + b) * (c - d)) / ((e + f) * (g - h))
   
   // Better
   FLEX numerator = (a + b) * (c - d)
   FLEX denominator = (e + f) * (g - h)
   FLEX x = numerator / denominator
   ```

4. **Use meaningful names**: For intermediate results
   ```powerscript
   FLEX isAdult = age >= 18
   FLEX hasPermission = isAdult AND hasConsent
   ```
