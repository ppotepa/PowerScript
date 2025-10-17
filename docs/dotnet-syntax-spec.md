# PowerScript .NET Interop Syntax Specification

This document defines the syntax rules for calling .NET methods and accessing .NET objects from PowerScript.

## Syntax Rules Summary

### ✅ VALID Syntax

1. **PowerScript Object Property Access** - Use `.` (dot)
   ```powerscript
   FLEX person = {name = "Alice", age = 30}
   FLEX name = person.name
   ```

2. **PSX Syntax Extensions** - Use `::` (double colon)
   ```powerscript
   FLEX numbers = {3, 1, 4}
   FLEX sorted = numbers::Sort()
   ```

3. **.NET Method Calls** - Use `#` + `->` (hash + arrow)
   ```powerscript
   #Console->WriteLine("Hello .NET")
   FLEX result = #Math->Abs(-42)
   FLEX str = #number->ToString()
   ```

### ❌ INVALID Syntax (Must Throw Errors)

1. **Arrow without Hash** - ILLEGAL
   ```powerscript
   // ❌ This should throw an error
   Console->WriteLine("test")
   number->ToString()
   ```

2. **NET Dot Syntax** - ILLEGAL
   ```powerscript
   // ❌ This should throw an error  
   NET.Console.WriteLine("test")
   NET.Math.Abs(-42)
   ```

3. **NET Double Colon Syntax** - ILLEGAL
   ```powerscript
   // ❌ This should throw an error
   NET::Console.WriteLine("test")
   ```

## Detailed Syntax Specifications

### PowerScript Objects (`.` Dot Operator)

**Purpose**: Access properties of PowerScript objects created with object literal syntax `{}`

**Examples**:
```powerscript
// Basic property access
FLEX person = {name = "Bob", age = 25}
FLEX name = person.name
FLEX age = person.age

// Nested objects
FLEX outer = {inner = {value = 42}}
FLEX val = outer.inner.value

// Type-annotated objects
FLEX point = {x = 10, y = 20} AS Point
FLEX x = point.x
```

**Key Points**:
- Only for PowerScript objects
- Case-insensitive property names
- Can chain: `obj.prop1.prop2.prop3`

### PSX Syntax Extensions (`::` Double Colon)

**Purpose**: Call extension methods defined in `.psx` files

**Examples**:
```powerscript
LINK "stdlib/syntax/arrays.psx"
LINK "stdlib/syntax/strings.psx"

// Array extensions
FLEX numbers = {5, 2, 8, 1}
FLEX sorted = numbers::Sort()
FLEX first = sorted::First()

// String extensions  
FLEX text = "hello"
FLEX upper = text::ToUpper()

// Method chaining
FLEX result = numbers::Sort()::First()
```

**Key Points**:
- Transforms to function calls (e.g., `numbers::Sort()` → `ARRAY_SORT(numbers)`)
- Defined in `.psx` files in `stdlib/syntax/`
- Supports chaining

### .NET Method Calls (`#` + `->` Hash + Arrow)

**Purpose**: Call .NET framework methods and constructors

**Syntax Forms**:

1. **Static Method Calls**:
   ```powerscript
   #Console->WriteLine("Hello")
   FLEX abs = #Math->Abs(-42)
   FLEX max = #Math->Max(10, 20)
   ```

2. **Instance Method Calls**:
   ```powerscript
   FLEX number = 123
   FLEX str = #number->ToString()
   
   FLEX text = "hello world"
   FLEX upper = #text->ToUpper()
   ```

3. **Property Access**:
   ```powerscript
   FLEX text = "hello"
   FLEX len = #text->Length
   ```

4. **Constructor Calls**:
   ```powerscript
   FLEX sb = #new System.Text.StringBuilder("Hello")
   #sb->Append(" World")
   FLEX result = #sb->ToString()
   ```

5. **File I/O**:
   ```powerscript
   #System.IO.File->WriteAllText("test.txt", "content")
   FLEX content = #System.IO.File->ReadAllText("test.txt")
   #System.IO.File->Delete("test.txt")
   ```

**Key Points**:
- Always prefix with `#`
- Use `->` for method calls
- Fully qualified names for System classes
- LINK System required

## Implementation Status

### ✅ Currently Working

- PowerScript object property access with `.`
- Object creation with `{}`
- Type annotations with `AS`
- **Arrow operator `->` (TEMPORARY - should be deprecated)**

### ❌ To Be Implemented

- `#`-prefixed .NET calls (`#Console->WriteLine()`)
- `::` operator parsing for PSX syntax
- Error throwing for plain `->` without `#`
- Error throwing for `NET.` syntax
- Error throwing for `NET::` syntax

### 🔄 To Be Deprecated

- Plain arrow operator `->` without `#` prefix
- `NET.` namespace syntax (if it exists)
- `NET::` namespace syntax

## Test Coverage

**Created**: `tests/PowerScript.StandardLibrary.Tests/DotNetSyntaxTests.cs`

**Test Categories**:
1. **Old Arrow Operator** (2 tests) - Should be illegal
2. **Hash + Arrow Syntax** (11 tests) - Future implementation
3. **Dot Syntax for Objects** (2 tests) - Currently working
4. **Error Cases** (6 tests) - Validation tests
5. **Syntax Clarity** (2 tests) - Demonstration tests

**Total**: 23 tests (2 passing, 18 skipped, 3 pending implementation)

## Migration Path

### Phase 1: Implement `#->` Syntax
1. Create HashToken for `#`
2. Update ExpressionParser to recognize `#identifier->method()`
3. Transform to .NET reflection calls
4. Add unit tests

### Phase 2: Deprecate Plain `->` 
1. Add warnings when `->` used without `#`
2. Update all stdlib files to use `#->`
3. Update all test files
4. Update documentation

### Phase 3: Make Plain `->` Illegal
1. Change warnings to errors
2. Remove old `->` parsing code
3. Ensure all tests pass

### Phase 4: Remove `NET.` and `NET::` Syntax
1. If `NET.` syntax exists, throw errors
2. If `NET::` syntax exists, throw errors  
3. Update any remaining code

## Examples: All Three Syntaxes Together

```powerscript
LINK System
LINK "stdlib/syntax/arrays.psx"

// PowerScript object with . (dot)
FLEX person = {
    name = "Alice",
    scores = {95, 87, 92, 88}
} AS Student

// PowerScript property access with . (dot)
FLEX name = person.name
FLEX scores = person.scores

// PSX syntax extension with :: (double colon)
FLEX sorted = scores::Sort()
FLEX highest = sorted::Last()

// .NET method call with #-> (hash + arrow)
#Console->WriteLine(name)
FLEX highestStr = #highest->ToString()
#Console->WriteLine(highestStr)
```

## Error Messages (Future)

When implementing, these errors should be clear:

```
Error: Arrow operator requires # prefix
  Found: Console->WriteLine("test")
  Expected: #Console->WriteLine("test")

Error: NET namespace syntax is no longer supported
  Found: NET.Console.WriteLine("test")
  Expected: #Console->WriteLine("test")

Error: NET:: syntax is no longer supported  
  Found: NET::Console.WriteLine("test")
  Expected: #Console->WriteLine("test")
```

## Documentation Updates Needed

Once implemented, update:
1. README.md - Syntax examples section
2. stdlib/StandardLibrary.ps - All `Console -> WriteLine` calls
3. All test files - Replace `->` with `#->`
4. PSXSyntaxTests.cs - Replace `Console -> WriteLine` with `#Console->WriteLine`
5. docs/custom-syntax-design.md - .NET interop section
