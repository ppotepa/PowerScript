# Custom Syntax Files (.psx) - Design Document

## Overview

Custom Syntax Files (`.psx`) allow extending PowerScript with new syntax patterns that transform into function calls or other PowerScript code. This provides a way to create domain-specific language extensions without modifying the core language.

## File Extension

- **Extension**: `.psx` (PowerScript eXtension)
- **Loading**: Via `LINK` statement or auto-discovery
- **Scope**: Syntax extensions are file-scoped (only affect files that import them)

## Two Types of Syntax Extensions

### 1. Operator-Based Extensions (::)

Transform method-like syntax into function calls.

**Syntax**:
```powerscript
// In .psx file:
SYNTAX $target::MethodName($args) => FUNCTION_NAME($target, $args)

// Usage in .ps file:
array::Sort()        → ARRAY_SORT(array)
list::First()        → LIST_FIRST(list)
str::ToLower()       → STR_LOWER(str)
numbers::Max()       → ARRAY_MAX(numbers)
```

**Key Features**:
- `::` is the operator for PowerScript extensions
- Left side (`$target`) becomes first argument
- Method name maps to function name
- Optional arguments `($args)` are appended

### 2. Pattern-Based Extensions

Transform natural language patterns into function calls.

**Syntax**:
```powerscript
// In .psx file:
SYNTAX FILTER Properties {$props} OF $collection => SELECT_PROPERTIES($props, $collection)
SYNTAX MAP $collection WITH $function => ARRAY_MAP($collection, $function)
SYNTAX TAKE $count FROM $collection => ARRAY_TAKE($collection, $count)

// Usage in .ps file:
FILTER Properties {name, age} OF users     → SELECT_PROPERTIES({name, age}, users)
MAP numbers WITH Square                    → ARRAY_MAP(numbers, Square)
TAKE 5 FROM results                        → ARRAY_TAKE(results, 5)
```

**Key Features**:
- Natural language-like patterns
- Variables prefixed with `$` are captured
- Curly braces `{...}` capture object literals
- Pattern keywords are case-insensitive

## PSX File Format

```powerscript
// File: arrays.psx
// Description: Array manipulation syntax extensions

// Operator-based extensions
SYNTAX $array::Sort() => ARRAY_SORT($array)
SYNTAX $array::Reverse() => ARRAY_REVERSE($array)
SYNTAX $array::First() => ARRAY_FIRST($array)
SYNTAX $array::Last() => ARRAY_LAST($array)
SYNTAX $array::Length() => ARRAY_LENGTH($array)
SYNTAX $array::Contains($item) => ARRAY_CONTAINS($array, $item)

// Pattern-based extensions
SYNTAX FILTER $array WHERE $condition => ARRAY_FILTER($array, $condition)
SYNTAX MAP $array WITH $function => ARRAY_MAP($array, $function)
SYNTAX TAKE $count FROM $array => ARRAY_TAKE($array, $count)
SYNTAX SKIP $count IN $array => ARRAY_SKIP($array, $count)
```

## Loading Syntax Extensions

### Option 1: Explicit LINK

```powerscript
LINK "arrays.psx"

// Now can use array syntax
VAR numbers = [3, 1, 4, 1, 5]
VAR sorted = numbers::Sort()
PRINT sorted
```

### Option 2: Auto-discovery

```powerscript
// PowerScript looks for .psx files in:
// 1. Same directory as script
// 2. ./stdlib/ directory
// 3. ./extensions/ directory
```

## Implementation Plan

### Phase 1: Core Infrastructure
- [ ] Create `PSXFileParser` class
- [ ] Create `CustomSyntaxRegistry` class
- [ ] Add `::` token (DoubleColonToken)
- [ ] Implement syntax pattern matching

### Phase 2: Operator Extensions
- [ ] Parse `$target::Method($args)` patterns
- [ ] Transform to function calls during parsing
- [ ] Test with simple array operations

### Phase 3: Pattern Extensions
- [ ] Parse `PATTERN $var KEYWORD $var` syntax
- [ ] Implement variable capture
- [ ] Transform to function calls
- [ ] Test with filter/map/take patterns

### Phase 4: Integration
- [ ] Integrate with LINK statement
- [ ] Add auto-discovery of .psx files
- [ ] Create standard library .psx files
- [ ] Documentation and examples

## Example Use Cases

### Array Operations
```powerscript
LINK "arrays.psx"

VAR numbers = [5, 2, 8, 1, 9]
VAR sorted = numbers::Sort()
VAR first = sorted::First()
VAR reversed = sorted::Reverse()

FILTER numbers WHERE IsEven
MAP numbers WITH Square
```

### String Operations
```powerscript
LINK "strings.psx"

VAR text = "Hello World"
VAR lower = text::ToLower()
VAR words = text::Split(" ")
VAR length = text::Length()
```

### Object Operations
```powerscript
LINK "objects.psx"

VAR users = [
    {name = "Alice", age = 30},
    {name = "Bob", age = 25}
]

FILTER Properties {name} OF users
SELECT age FROM users WHERE age > 25
```

## Benefits

1. **Extensibility**: Add new syntax without modifying core language
2. **Readability**: Domain-specific syntax improves code clarity
3. **Modularity**: Extensions are optional and file-scoped
4. **Backward Compatibility**: Original function call syntax still works
5. **Type Safety**: Extensions map to existing functions with validation

## Notes

- `.psx` files are parsed once and cached
- Syntax extensions don't affect runtime performance (compile-time transformation)
- Multiple .psx files can be loaded in same script
- Name conflicts between extensions cause compilation error
- `::` operator is reserved for PowerScript extensions only
- `#` and `->` remain reserved for .NET interop (future feature)
