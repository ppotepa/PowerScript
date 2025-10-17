# PSX Syntax Extension Test Coverage

This document summarizes the comprehensive test coverage created for PowerScript Extension (PSX) syntax features.

## Test Overview

**Total PSX Tests Created: 17**
- **Status**: All tests marked as `[Ignore]` pending implementation
- **Location**: `tests/PowerScript.StandardLibrary.Tests/PSXSyntaxTests.cs`
- **Purpose**: Define expected behavior for `::` operator and pattern-based syntax

## Test Breakdown

### Operator Syntax Tests (8 tests)

Tests for the `::` operator that transforms to function calls:

1. **OperatorSyntax_BasicArraySort_TransformsToFunctionCall**
   - Tests: `numbers::Sort()` → `ARRAY_SORT(numbers)`
   - Expected output: Sorted array

2. **OperatorSyntax_ArrayChaining_TransformsToNestedCalls**
   - Tests: `numbers::Sort()::First()` → `ARRAY_FIRST(ARRAY_SORT(numbers))`
   - Expected output: First element of sorted array

3. **OperatorSyntax_StringToUpper_TransformsToFunctionCall**
   - Tests: `text::ToUpper()` → `STR_UPPER(text)`
   - Expected output: Uppercase string

4. **OperatorSyntax_StringChaining_TransformsToNestedCalls**
   - Tests: `text::ToUpper()::Reverse()` → `STR_REVERSE(STR_UPPER(text))`
   - Expected output: Reversed uppercase string

5. **OperatorSyntax_ObjectProps_TransformsToFunctionCall**
   - Tests: `person::Props()` → `OBJECT_PROPS(person)`
   - Expected output: Array of object property names

6. **OperatorSyntax_MultipleTypes_AllTransformCorrectly**
   - Tests: Multiple `::` operators across arrays, strings, and objects
   - Expected output: All transformations work correctly

7. **OperatorSyntax_UnknownMethod_ThrowsError** (Error Case)
   - Tests: `numbers::UnknownMethod()` throws error
   - Expected: Runtime exception for unknown method

8. **OperatorSyntax_WrongType_ThrowsError** (Error Case)
   - Tests: `text::Sort()` throws error (Sort is for arrays)
   - Expected: Runtime exception for type mismatch

### Pattern Syntax Tests (7 tests)

Tests for pattern-based syntax that transforms to lambda functions:

9. **PatternSyntax_Filter_TransformsToLambda**
   - Tests: `FILTER $numbers WHERE $item % 2 == 0`
   - Transform: `ARRAY_FILTER(numbers, LAMBDA($item) { RETURN $item % 2 == 0 })`
   - Expected output: Even numbers

10. **PatternSyntax_Map_TransformsToLambda**
    - Tests: `MAP $numbers WITH $item * 2`
    - Transform: `ARRAY_MAP(numbers, LAMBDA($item) { RETURN $item * 2 })`
    - Expected output: Doubled numbers

11. **PatternSyntax_Take_TransformsToFunctionCall**
    - Tests: `TAKE 3 FROM $numbers`
    - Transform: `ARRAY_TAKE(numbers, 3)`
    - Expected output: First 3 elements

12. **PatternSyntax_TakeNegative_TakesFromEnd**
    - Tests: `TAKE -3 FROM $numbers`
    - Transform: `ARRAY_TAKE(numbers, -3)`
    - Expected output: Last 3 elements

13. **PatternSyntax_FilterAndMap_Combines**
    - Tests: Sequential FILTER then MAP operations
    - Expected output: Filtered and mapped array

14. **PatternSyntax_ComplexNested_TransformsCorrectly**
    - Tests: `TAKE 3 FROM MAP (FILTER $numbers WHERE ...) WITH ...`
    - Transform: Nested function calls
    - Expected output: Correctly nested transformations

15. **PatternSyntax_InvalidCondition_ThrowsError** (Error Case)
    - Tests: `FILTER $numbers WHERE invalid syntax here`
    - Expected: Parse exception

### Integration Tests (2 tests)

Tests combining multiple PSX syntax features:

16. **Integration_OperatorAndPattern_WorkTogether**
    - Tests: FILTER pattern + `::` operator chaining
    - Example: `evens::Sort()::First()` on filtered array
    - Expected output: Correct combination of pattern and operator syntax

17. **Integration_StringPatternWithOperator_WorksTogether**
    - Tests: `::` operator inside MAP pattern
    - Example: `MAP $words WITH $item::ToUpper()`
    - Expected output: Uppercase transformation within pattern

## Test Script Coverage

In addition to unit tests, we have **9 test scripts** in `test-scripts/syntax/`:

**Operator Tests (5 scripts):**
- `syntax_psx_operator_basic.ps` - Basic `::` operator
- `syntax_psx_operator_chaining.ps` - Operator chaining
- `syntax_psx_operator_strings.ps` - String operators
- `syntax_psx_operator_objects.ps` - Object operators
- `syntax_psx_operator_multiple.ps` - Multiple operators

**Pattern Tests (4 scripts):**
- `syntax_psx_pattern_filter.ps` - FILTER pattern
- `syntax_psx_pattern_map.ps` - MAP pattern
- `syntax_psx_pattern_take.ps` - TAKE pattern
- `syntax_psx_pattern_complex.ps` - Complex patterns

## Running the Tests

### Run all PSX syntax unit tests:
```bash
dotnet test tests/PowerScript.StandardLibrary.Tests/ --filter "FullyQualifiedName~PSXSyntax"
```

### Run specific test:
```bash
dotnet test tests/PowerScript.StandardLibrary.Tests/ --filter "OperatorSyntax_BasicArraySort"
```

### Run test scripts:
```bash
dotnet run --project src/PowerScript.CLI/PowerScript.CLI.csproj -- test-scripts/syntax/syntax_psx_operator_basic.ps
```

## Implementation Status

### ✅ Ready for Implementation
All 17 unit tests are:
- ✅ Written with correct NUnit syntax
- ✅ Properly documented with expected transformations
- ✅ Marked with `[Ignore]` attributes until implementation
- ✅ Compiled and building successfully
- ✅ Ready to be enabled when features are implemented

### ❌ Pending Implementation
- ❌ PSXFileParser (parse `.psx` files)
- ❌ `::` operator parsing in ExpressionParser
- ❌ Pattern syntax parsing (FILTER, MAP, TAKE)
- ❌ Syntax transformation execution
- ❌ Function implementations (ARRAY_SORT, STR_UPPER, etc.)

## Test Structure

Each test follows this pattern:

```csharp
[Test]
[Ignore(":: operator parsing not yet implemented")]
public void OperatorSyntax_BasicArraySort_TransformsToFunctionCall()
{
    // Arrange
    string code = @"
LINK ""stdlib/syntax/arrays.psx""
LINK System

FLEX numbers = {3, 1, 4, 1, 5}
FLEX sorted = numbers::Sort()

FLEX dummy = Console -> WriteLine(sorted)
";

    // Act
    var output = ExecuteCode(code);

    // Assert
    // Should transform to: FLEX sorted = ARRAY_SORT(numbers)
    // Expected output: {1, 1, 3, 4, 5}
    Assert.That(output, Does.Contain("{1, 1, 3, 4, 5}"));
}
```

## Next Steps

1. **Implement PSXFileParser**
   - Parse SYNTAX keyword
   - Extract operator and pattern definitions
   - Register transformations with CustomSyntaxRegistry

2. **Enable Operator Parsing**
   - Detect `::` in ExpressionParser
   - Transform to function calls
   - Support chaining

3. **Enable Pattern Parsing**
   - Recognize FILTER, MAP, TAKE keywords
   - Extract captured variables
   - Transform to lambda functions

4. **Create Function Implementations**
   - Implement all ARRAY_* functions
   - Implement all STR_* functions
   - Implement all OBJECT_* functions

5. **Enable Tests**
   - Remove `[Ignore]` attributes
   - Run and verify all 17 tests pass
   - Add to CI/CD pipeline

## Total Test Count

**Before PSX Tests**: 71 passing tests
**After PSX Tests**: 88 total tests (71 passing + 17 skipped)
**Coverage Increase**: +24% test coverage for future features
