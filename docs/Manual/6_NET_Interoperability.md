# PowerScript Language Reference Manual
## Part 6: .NET Interoperability

### 6.1 .NET Interoperability Overview

PowerScript provides seamless integration with the .NET Framework, allowing you to call .NET methods directly from your scripts.

### 6.2 LINK Statement for Namespaces

Use `LINK` to import .NET namespaces.

**Syntax:**

```powerscript
LINK NamespaceName
```

**Common Namespaces:**

```powerscript
LINK System
LINK System.IO
LINK System.Collections
LINK System.Text
```

**Multiple LINK Statements:**

```powerscript
LINK System
LINK System.IO
LINK System.Text

// Now can use classes from these namespaces
```

### 6.3 .NET Method Call Syntax

PowerScript provides two syntaxes for calling .NET methods:

**1. Full Syntax (NET::):**

```powerscript
NET::FullNamespace.ClassName.MethodName(arguments)
```

**2. Short Syntax (#):**

```powerscript
#ClassName.MethodName(arguments)
```

The short syntax requires the namespace to be linked first.

### 6.4 Console Operations

**Using Full Syntax:**

```powerscript
NET::System.Console.WriteLine("Hello, World!")
NET::System.Console.Write("No newline")
```

**Using Short Syntax:**

```powerscript
LINK System

#Console.WriteLine("Hello, World!")
#Console.Write("No newline")
```

**With Variables:**

```powerscript
LINK System

FLEX message = "PowerScript"
#Console.WriteLine(message)

FLEX number = 42
#Console.WriteLine(number)
```

**Multiple Arguments:**

```powerscript
LINK System

FLEX name = "Alice"
FLEX age = 30
#Console.WriteLine("Name: " + name + ", Age: " + age)
```

### 6.5 String Operations

**String Methods:**

```powerscript
LINK System

FLEX text = "Hello, World!"

// String length (property access may vary)
#Console.WriteLine(text)

// String concatenation
FLEX combined = text + " PowerScript"
#Console.WriteLine(combined)
```

**String Formatting:**

```powerscript
LINK System

FLEX firstName = "John"
FLEX lastName = "Doe"
FLEX fullName = firstName + " " + lastName

#Console.WriteLine(fullName)
```

### 6.6 Math Operations

**.NET Math Class:**

```powerscript
LINK System

// Using .NET Math methods
NET::System.Math.Abs(-42)
NET::System.Math.Max(10, 20)
NET::System.Math.Min(10, 20)
```

**With Short Syntax:**

```powerscript
LINK System

#Math.Abs(-42)
#Math.Max(10, 20)
#Math.Min(10, 20)
```

### 6.7 File Operations

**File I/O with System.IO:**

```powerscript
LINK System.IO

// Write to file
NET::System.IO.File.WriteAllText("output.txt", "Hello from PowerScript!")

// Read from file
NET::System.IO.File.ReadAllText("input.txt")
```

**Directory Operations:**

```powerscript
LINK System.IO

// Check if directory exists
NET::System.IO.Directory.Exists("MyFolder")

// Create directory
NET::System.IO.Directory.CreateDirectory("NewFolder")

// List files
NET::System.IO.Directory.GetFiles(".")
```

### 6.8 DateTime Operations

**Working with Dates:**

```powerscript
LINK System

// Get current date/time
NET::System.DateTime.Now

// Date properties
NET::System.DateTime.Today
```

### 6.9 Common .NET Patterns

**Console Input/Output:**

```powerscript
LINK System

FUNCTION greet(STRING name)[] {
    #Console.WriteLine("Hello, " + name + "!")
}

FUNCTION printNumber(INT num)[] {
    #Console.WriteLine(num)
}
```

**String Manipulation:**

```powerscript
LINK System

FUNCTION formatMessage(STRING prefix, STRING message)[STRING] {
    FLEX formatted = prefix + ": " + message
    #Console.WriteLine(formatted)
    RETURN formatted
}
```

**File Writing:**

```powerscript
LINK System.IO

FUNCTION saveData(STRING filename, STRING content)[] {
    NET::System.IO.File.WriteAllText(filename, content)
    #Console.WriteLine("Data saved to " + filename)
}
```

### 6.10 Combining PowerScript and .NET

**Example: Logging System:**

```powerscript
LINK System

FUNCTION log(STRING message)[] {
    FLEX timestamp = "LOG: "
    FLEX fullMessage = timestamp + message
    #Console.WriteLine(fullMessage)
}

log("Application started")
log("Processing data...")
log("Application finished")
```

**Example: Data Processing:**

```powerscript
LINK System

FUNCTION processNumbers(INT arr, INT size)[] {
    FLEX sum = 0
    
    CYCLE size AS i {
        FLEX sum = sum + arr[i]
    }
    
    #Console.WriteLine("Sum: " + sum)
    
    FLEX average = sum / size
    #Console.WriteLine("Average: " + average)
}

FLEX numbers = [10, 20, 30, 40, 50]
processNumbers(numbers, 5)
```

### 6.11 Best Practices

**1. Link Namespaces at Top:**

```powerscript
// Good: All LINK statements at the beginning
LINK System
LINK System.IO
LINK System.Text

FUNCTION main()[] {
    // Function code
}
```

**2. Use Short Syntax When Possible:**

```powerscript
// Good: Readable with short syntax
LINK System

#Console.WriteLine("Message")

// Less readable: Full syntax when unnecessary
NET::System.Console.WriteLine("Message")
```

**3. Consistent Naming:**

```powerscript
// Good: PowerScript conventions
LINK System

FUNCTION outputMessage(STRING msg)[] {
    #Console.WriteLine(msg)
}
```

**4. Error Handling Context:**

```powerscript
LINK System
LINK System.IO

FUNCTION safeReadFile(STRING path)[] {
    // Check before accessing
    IF NET::System.IO.File.Exists(path) {
        FLEX content = NET::System.IO.File.ReadAllText(path)
        #Console.WriteLine(content)
    }
}
```

### 6.12 Common .NET Classes

**System Namespace:**
- `Console`: Input/output operations
- `Math`: Mathematical functions
- `String`: String manipulation
- `DateTime`: Date and time operations
- `Environment`: System information

**System.IO Namespace:**
- `File`: File operations
- `Directory`: Directory operations
- `Path`: Path manipulation
- `StreamReader`: Reading streams
- `StreamWriter`: Writing streams

**System.Collections Namespace:**
- `ArrayList`: Dynamic arrays
- `Hashtable`: Key-value pairs
- `Queue`: FIFO collection
- `Stack`: LIFO collection

### 6.13 Limitations

**Current Limitations:**

1. **Method Calls Only**: Can call methods but not instantiate classes directly
2. **Static Methods**: Primarily designed for static method calls
3. **Simple Types**: Best with primitive types and strings
4. **No Property Access**: Limited property getter/setter support

**Workarounds:**

```powerscript
// Use method calls for most operations
LINK System

#Console.WriteLine("Output")
NET::System.Math.Max(10, 20)
```

### 6.14 Advanced Examples

**Example 1: File Logger:**

```powerscript
LINK System
LINK System.IO

FLEX logFile = "application.log"

FUNCTION writeLog(STRING message)[] {
    FLEX timestamp = "[LOG] "
    FLEX entry = timestamp + message + "\n"
    
    // Append to log file
    NET::System.IO.File.AppendAllText(logFile, entry)
    #Console.WriteLine(entry)
}

writeLog("Application started")
writeLog("Processing data")
writeLog("Application finished")
```

**Example 2: Data Processor:**

```powerscript
LINK System

FUNCTION analyzeArray(INT arr, INT size)[] {
    #Console.WriteLine("=== Array Analysis ===")
    
    FLEX sum = 0
    FLEX min = arr[0]
    FLEX max = arr[0]
    
    CYCLE size AS i {
        FLEX sum = sum + arr[i]
        
        IF arr[i] < min {
            FLEX min = arr[i]
        }
        
        IF arr[i] > max {
            FLEX max = arr[i]
        }
    }
    
    #Console.WriteLine("Count: " + size)
    #Console.WriteLine("Sum: " + sum)
    #Console.WriteLine("Min: " + min)
    #Console.WriteLine("Max: " + max)
    
    FLEX avg = sum / size
    #Console.WriteLine("Average: " + avg)
}

FLEX data = [15, 23, 8, 42, 16, 31, 4]
analyzeArray(data, 7)
```

### 6.15 Debugging with .NET

**Console Output for Debugging:**

```powerscript
LINK System

FUNCTION debugPrint(STRING label, INT value)[] {
    FLEX output = "[DEBUG] " + label + " = " + value
    #Console.WriteLine(output)
}

FLEX x = 10
debugPrint("x", x)

FLEX y = x * 2
debugPrint("y", y)
```

**Tracing Execution:**

```powerscript
LINK System

FUNCTION trace(STRING functionName)[] {
    FLEX msg = "[TRACE] Entering " + functionName
    #Console.WriteLine(msg)
}

FUNCTION myFunction()[] {
    trace("myFunction")
    // Function logic
}
```
