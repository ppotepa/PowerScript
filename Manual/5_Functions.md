# PowerScript Language Reference Manual
## Part 5: Functions

### 5.1 Function Overview

Functions are reusable blocks of code that can accept parameters and return values. PowerScript functions are statically typed and support recursion.

### 5.2 Function Declaration

**Syntax:**

```powerscript
FUNCTION functionName(TYPE param1, TYPE param2)[RETURN_TYPE] {
    // function body
    RETURN value
}
```

**Components:**
- `FUNCTION`: Keyword to declare a function
- `functionName`: Identifier for the function
- Parameters: Comma-separated typed parameters `(TYPE name, ...)`
- Return type: Type in brackets `[TYPE]` or `[]` for void
- Body: Statements enclosed in `{ }`
- `RETURN`: Statement to return a value (required for non-void functions)

### 5.3 Void Functions

Functions that don't return a value use empty brackets `[]`.

**Simple Void Function:**

```powerscript
FUNCTION greet(STRING name)[] {
    PRINT "Hello, " + name + "!"
}

// Call the function
greet("Alice")  // Prints: Hello, Alice!
```

**Void Function with Multiple Parameters:**

```powerscript
FUNCTION printSum(INT a, INT b)[] {
    FLEX sum = a + b
    PRINT sum
}

printSum(5, 3)  // Prints: 8
```

**Void Function with No Parameters:**

```powerscript
FUNCTION sayHello()[] {
    PRINT "Hello, World!"
}

sayHello()  // Prints: Hello, World!
```

### 5.4 Functions with Return Values

Functions that return values must specify the return type.

**Simple Return Function:**

```powerscript
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

FLEX result = add(10, 5)
PRINT result  // 15
```

**String Return Function:**

```powerscript
FUNCTION getGreeting(STRING name)[STRING] {
    RETURN "Hello, " + name
}

FLEX message = getGreeting("Bob")
PRINT message  // Hello, Bob
```

**Boolean Return Function:**

```powerscript
FUNCTION isEven(INT num)[BOOL] {
    FLEX remainder = num % 2
    RETURN remainder == 0
}

FLEX check = isEven(4)
PRINT check  // TRUE
```

### 5.5 Function Parameters

**Single Parameter:**

```powerscript
FUNCTION square(INT x)[INT] {
    RETURN x * x
}

PRINT square(5)  // 25
```

**Multiple Parameters:**

```powerscript
FUNCTION multiply(INT a, INT b, INT c)[INT] {
    RETURN a * b * c
}

PRINT multiply(2, 3, 4)  // 24
```

**Mixed Parameter Types:**

```powerscript
FUNCTION formatMessage(STRING prefix, INT count)[STRING] {
    RETURN prefix + ": " + count
}

PRINT formatMessage("Total", 42)  // Total: 42
```

### 5.6 Return Statement

The `RETURN` statement exits a function and optionally provides a return value.

**Required for Non-Void Functions:**

```powerscript
FUNCTION getMax(INT a, INT b)[INT] {
    IF a > b {
        RETURN a
    }
    RETURN b  // Must have return on all paths
}
```

**Early Return:**

```powerscript
FUNCTION findValue(INT target)[INT] {
    FLEX arr = [10, 20, 30, 40, 50]
    
    CYCLE 5 AS i {
        IF arr[i] == target {
            RETURN i  // Early return when found
        }
    }
    
    RETURN -1  // Not found
}
```

**Multiple Return Points:**

```powerscript
FUNCTION getGrade(INT score)[STRING] {
    IF score >= 90 {
        RETURN "A"
    }
    IF score >= 80 {
        RETURN "B"
    }
    IF score >= 70 {
        RETURN "C"
    }
    RETURN "F"
}
```

### 5.7 Function Calls

**Simple Call:**

```powerscript
FUNCTION sayHi()[] {
    PRINT "Hi!"
}

sayHi()  // Prints: Hi!
```

**Call with Arguments:**

```powerscript
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

FLEX sum = add(5, 3)
```

**Nested Calls:**

```powerscript
FUNCTION double(INT x)[INT] {
    RETURN x * 2
}

FUNCTION quadruple(INT x)[INT] {
    RETURN double(double(x))
}

PRINT quadruple(5)  // 20
```

**Call in Expression:**

```powerscript
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

FLEX result = add(10, 5) * 2
PRINT result  // 30
```

### 5.8 Recursion

PowerScript supports recursive function calls.

**Factorial:**

```powerscript
FUNCTION factorial(INT n)[INT] {
    IF n <= 1 {
        RETURN 1
    }
    RETURN n * factorial(n - 1)
}

PRINT factorial(5)  // 120
```

**Fibonacci:**

```powerscript
FUNCTION fibonacci(INT n)[INT] {
    IF n <= 1 {
        RETURN n
    }
    RETURN fibonacci(n - 1) + fibonacci(n - 2)
}

PRINT fibonacci(7)  // 13
```

**Sum of Array (Recursive):**

```powerscript
FUNCTION sumArray(INT arr, INT index, INT size)[INT] {
    IF index >= size {
        RETURN 0
    }
    RETURN arr[index] + sumArray(arr, index + 1, size)
}
```

### 5.9 Local Variables in Functions

Variables declared inside functions are local to that function.

```powerscript
FUNCTION calculate()[INT] {
    FLEX local1 = 10
    FLEX local2 = 20
    FLEX sum = local1 + local2
    RETURN sum
}

// local1, local2, sum are not accessible here
FLEX result = calculate()
PRINT result  // 30
```

### 5.10 Global Variable Access

Functions can access global variables.

```powerscript
FLEX globalValue = 100

FUNCTION useGlobal()[INT] {
    RETURN globalValue + 50
}

PRINT useGlobal()  // 150
```

**Modifying Globals:**

```powerscript
FLEX counter = 0

FUNCTION increment()[] {
    FLEX counter = counter + 1
}

increment()
increment()
PRINT counter  // 2
```

### 5.11 Common Function Patterns

**Validation Function:**

```powerscript
FUNCTION isValidAge(INT age)[BOOL] {
    RETURN age >= 0 AND age <= 120
}

IF isValidAge(25) {
    PRINT "Valid age"
}
```

**Calculation Function:**

```powerscript
FUNCTION calculateTotal(INT price, INT quantity)[INT] {
    FLEX subtotal = price * quantity
    FLEX tax = subtotal * 8 / 100
    RETURN subtotal + tax
}

FLEX total = calculateTotal(100, 5)
PRINT total  // 540
```

**Array Processing Function:**

```powerscript
FUNCTION findMax(INT arr, INT size)[INT] {
    FLEX max = arr[0]
    
    CYCLE size AS i {
        IF arr[i] > max {
            FLEX max = arr[i]
        }
    }
    
    RETURN max
}

FLEX numbers = [23, 45, 12, 67, 34]
FLEX maximum = findMax(numbers, 5)
PRINT maximum  // 67
```

**Helper Functions:**

```powerscript
FUNCTION isEven(INT num)[BOOL] {
    RETURN num % 2 == 0
}

FUNCTION countEven(INT arr, INT size)[INT] {
    FLEX count = 0
    
    CYCLE size AS i {
        IF isEven(arr[i]) {
            FLEX count = count + 1
        }
    }
    
    RETURN count
}
```

### 5.12 Function Composition

Build complex functionality from simple functions.

```powerscript
FUNCTION double(INT x)[INT] {
    RETURN x * 2
}

FUNCTION addTen(INT x)[INT] {
    RETURN x + 10
}

FUNCTION doubleThenAddTen(INT x)[INT] {
    FLEX doubled = double(x)
    RETURN addTen(doubled)
}

PRINT doubleThenAddTen(5)  // 20 (5*2 + 10)
```

### 5.13 Mathematical Functions

**Power Function:**

```powerscript
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

PRINT power(2, 10)  // 1024
```

**GCD Function:**

```powerscript
FUNCTION gcd(INT a, INT b)[INT] {
    IF b == 0 {
        RETURN a
    }
    RETURN gcd(b, a % b)
}

PRINT gcd(48, 18)  // 6
```

**Is Prime:**

```powerscript
FUNCTION isPrime(INT n)[BOOL] {
    IF n <= 1 {
        RETURN FALSE
    }
    
    IF n <= 3 {
        RETURN TRUE
    }
    
    CYCLE n AS i {
        IF i > 1 AND i < n {
            IF n % i == 0 {
                RETURN FALSE
            }
        }
    }
    
    RETURN TRUE
}

PRINT isPrime(17)  // TRUE
```

### 5.14 Best Practices

**1. Single Responsibility:**

```powerscript
// Good: Each function does one thing
FUNCTION calculateTax(INT amount)[INT] {
    RETURN amount * 8 / 100
}

FUNCTION calculateTotal(INT subtotal)[INT] {
    FLEX tax = calculateTax(subtotal)
    RETURN subtotal + tax
}
```

**2. Meaningful Names:**

```powerscript
// Good
FUNCTION calculateMonthlyPayment(INT principal, INT months)[INT]

// Less clear
FUNCTION calc(INT p, INT m)[INT]
```

**3. Keep Functions Small:**

```powerscript
// Good: Break down complex logic
FUNCTION validateInput(STRING input)[BOOL] {
    FLEX hasMinLength = checkMinLength(input)
    FLEX hasValidChars = checkValidChars(input)
    RETURN hasMinLength AND hasValidChars
}
```

**4. Document Complex Logic:**

```powerscript
// Calculate compound interest
// P * (1 + r)^t where P=principal, r=rate, t=time
FUNCTION compoundInterest(INT principal, INT rate, INT years)[INT] {
    // Implementation
}
```

**5. Validate Parameters:**

```powerscript
FUNCTION divide(INT a, INT b)[INT] {
    IF b == 0 {
        PRINT "Error: Division by zero"
        RETURN 0
    }
    RETURN a / b
}
```

### 5.15 Common Pitfalls

**1. Missing Return Statement:**

```powerscript
// Error: Non-void function must return
FUNCTION add(INT a, INT b)[INT] {
    FLEX sum = a + b
    // Missing RETURN statement!
}
```

**2. Type Mismatch:**

```powerscript
// Error: Return type doesn't match declaration
FUNCTION getName()[INT] {
    RETURN "John"  // Should return INT, not STRING
}
```

**3. Wrong Parameter Types:**

```powerscript
FUNCTION add(INT a, INT b)[INT] {
    RETURN a + b
}

// Error: Wrong parameter types
// add("5", "3")  // Expects INT, not STRING
```

**4. Unreachable Code:**

```powerscript
FUNCTION example()[INT] {
    RETURN 10
    PRINT "This will never execute"  // Unreachable
}
```
