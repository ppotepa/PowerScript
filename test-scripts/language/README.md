# PowerScript Language Feature Tests

This folder contains focused tests for individual PowerScript language features.

## Test Organization

Each test file focuses on a **single language feature** for clarity and maintainability.

### Type System Tests (1-6)
| File | Feature | Description |
|------|---------|-------------|
| `1_flex_type.ps` | FLEX type | Dynamic typing with type changes |
| `2_var_type.ps` | VAR type | Auto-typed from initial value |
| `3_int_type.ps` | INT type | Static integer type |
| `4_string_type.ps` | STRING type | Static string type |
| `5_number_type.ps` | NUMBER type | Flexible numeric (int or double) |
| `6_prec_type.ps` | PREC type | Explicit floating-point type |

### Bit-Width Specification Tests (7-10)
| File | Feature | Description |
|------|---------|-------------|
| `7_bitwidth_int8.ps` | INT[8] | 8-bit integer declarations |
| `8_bitwidth_int16.ps` | INT[16] | 16-bit integer declarations |
| `9_bitwidth_number.ps` | NUMBER[n] | Bit-width with NUMBER type |
| `10_bitwidth_arithmetic.ps` | Arithmetic | Operations with bit-width types |

### Control Flow Tests (11-12)
| File | Feature | Description |
|------|---------|-------------|
| `11_cycle_loop.ps` | CYCLE loop | PowerScript's primary loop construct |
| `12_if_else.ps` | IF/ELSE | Conditional statements |

### Operator Tests (13-16)
| File | Feature | Description |
|------|---------|-------------|
| `13_comparison_operators.ps` | Comparison | `>`, `<`, `>=`, `<=`, `==`, `!=` |
| `14_logical_and.ps` | Logical AND | AND operator |
| `15_logical_or.ps` | Logical OR | OR operator |
| `16_arithmetic_operators.ps` | Arithmetic | `+`, `-`, `*`, `/` |

### Advanced Feature Tests (17-25)
| File | Feature | Description |
|------|---------|-------------|
| `17_nested_loops.ps` | Nesting | Nested CYCLE loops |
| `18_nested_conditionals.ps` | Nesting | Nested IF statements |
| `19_operator_precedence.ps` | Precedence | Order of operations |
| `20_print_statement.ps` | PRINT | Output statement |
| `21_string_literals.ps` | Strings | String literal syntax |
| `22_variable_reassignment.ps` | Reassignment | FLEX variable changes |
| `23_mixed_types.ps` | Mixed types | Combining different types |
| `24_bitwidth_mixed.ps` | Mixed | Bit-width + non-bit-width |
| `25_complex_expression.ps` | Complex | Multi-operator expressions |

## Running the Tests

### Run all language feature tests:
```bash
dotnet test --filter Category=Language
```

### Run specific feature test:
```bash
# Example: Run only type system tests (1-6)
cd test-scripts/language
# Run individual script with interpreter
```

### Test Results
**Total Tests: 25**  
**Status: 25/25 PASSING (100%)** ✅

All language features are fully implemented and tested!

## Test Format

Each test file:
1. **Focuses on ONE feature** - Clear, isolated testing
2. **Includes comments** - Explains what is being tested
3. **Has minimal code** - Easy to understand and debug
4. **Prints output** - Verifies execution

## Comparison to Other Test Folders

| Folder | Purpose | Tests |
|--------|---------|-------|
| **language/** | Individual language features | 25 tests |
| **simple/** | Basic combined usage | 12 tests |
| **moderate/** | Medium complexity algorithms | 10 tests |
| **complex/** | Advanced algorithms | 27 tests |

The `language/` folder is the best place to:
- ✅ Learn PowerScript features
- ✅ Test new language features
- ✅ Verify feature implementation
- ✅ Debug specific functionality

## Adding New Language Feature Tests

When adding a new feature to PowerScript:

1. **Create test file**: `test-scripts/language/N_feature_name.ps`
2. **Add NUnit test**: Add to `ScriptFileTests.cs`
3. **Follow naming convention**: Sequential numbering, descriptive name
4. **Keep it simple**: Focus on ONE feature
5. **Add to this README**: Update the table above

## Examples

### Type Test Example (1_flex_type.ps)
```powerscript
// Test: FLEX type - dynamic typing that allows type changes
FLEX x = 10
PRINT x

FLEX x = "hello"
PRINT x
```

### Bit-Width Test Example (7_bitwidth_int8.ps)
```powerscript
// Test: Bit-width specification - INT[8]
INT[8] byte = 255
PRINT byte

INT[8] small = 100
PRINT small
```

### Control Flow Test Example (11_cycle_loop.ps)
```powerscript
// Test: CYCLE loop - PowerScript's primary loop construct
FLEX sum = 0
CYCLE 5 AS i {
    FLEX sum = sum + 1
}
PRINT sum
```

---

**All tests verified working as of December 2024** ✅
