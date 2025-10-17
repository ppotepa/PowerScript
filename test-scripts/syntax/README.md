# PowerScript Syntax Tests

This folder contains test scripts for PowerScript language syntax features, particularly the object literal syntax.

## Object Syntax Tests

### Basic Features

- **syntax_object_basic.ps** - Basic object creation and property access
  ```powerscript
  FLEX person = {name = "John", age = 30}
  FLEX name = person.name
  ```

- **syntax_object_empty.ps** - Empty object creation
  ```powerscript
  FLEX empty = {}
  ```

### Type Annotations

- **syntax_object_typed.ps** - Objects with type annotations
  ```powerscript
  FLEX employee = {name = "Alice", role = "Developer"} AS Employee
  ```

- **syntax_object_strict.ps** - Strict (non-extendable) type objects
  ```powerscript
  FLEX point = {x = 10, y = 20} AS Point!
  ```

### Advanced Features

- **syntax_object_expressions.ps** - Objects with expressions as values
  ```powerscript
  FLEX data = {sum = 5 + 10, product = 6 * 7}
  ```

- **syntax_object_nested.ps** - Nested objects and deep property access
  ```powerscript
  FLEX outer = {inner = {value = 42}}
  FLEX value = outer.inner.value
  ```

- **syntax_object_multiple.ps** - Multiple objects in same script
  ```powerscript
  FLEX person1 = {name = "Alice"}
  FLEX person2 = {name = "Bob"}
  ```

- **syntax_object_complex.ps** - Complex objects with mixed types
  ```powerscript
  FLEX config = {host = "localhost", port = 8080, ssl = 1}
  ```

### Variable Operations

- **syntax_object_reassignment.ps** - Object variable reassignment
  ```powerscript
  FLEX obj = {value = 1}
  FLEX obj = {value = 2}  // reassign
  ```

- **syntax_object_case_insensitive.ps** - Case-insensitive property access
  ```powerscript
  FLEX item = {Name = "Test"}
  FLEX name = item.name  // lowercase access works
  ```

## Running the Tests

Run individual tests with:
```bash
.\src\PowerScript.CLI\bin\Debug\net8.0\PowerScript.CLI.exe test-scripts\syntax\syntax_object_basic.ps
```

Run all syntax tests:
```bash
Get-ChildItem test-scripts\syntax\*.ps | ForEach-Object { 
    Write-Host "Running: $($_.Name)"
    .\src\PowerScript.CLI\bin\Debug\net8.0\PowerScript.CLI.exe $_.FullName
    Write-Host ""
}
```

## Expected Output

Each test should:
1. Print section headers (e.g., "=== Basic Object Creation ===")
2. Print object representations showing properties
3. Print individual property values when accessed
4. End with "Test completed successfully"

## Notes

- All property names are stored in uppercase internally (e.g., `name` → `NAME`)
- Property values preserve their original casing
- Objects support case-insensitive property access
- Empty objects are represented as `{}`
- Type annotations appear in object's string representation
- Strict types (with `!`) are marked as non-extendable
