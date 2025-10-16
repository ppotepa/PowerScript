# PowerScript Parser Bug Reproductions

This directory contains minimal reproduction cases for all discovered parser bugs that block test progress.

## Purpose

These files are designed to:
1. Provide clear, isolated examples of each parser bug
2. Help parser developers understand and fix the issues
3. Serve as regression tests once bugs are fixed

## How to Test

Each file can be compiled individually to reproduce the bug:

```powershell
dotnet run --project src\PowerScript.CLI -- "parser-bug-reproductions\BUG_X_Name.ps"
```

## Bug Inventory

### CRITICAL Bugs (Block 85+ tests)

#### BUG #1: Variable Assignment in Nested Scopes
- **File**: `BUG_1_NestedScopeAssignment.ps`
- **Error**: `Unexpected token type: EqualsToken`
- **Impact**: Blocks ~48 tests (MathLib, StringLib, ConversionLib)
- **Example**: `result = result + 1` inside IF block fails
- **Root Cause**: VariableAssignmentProcessor fails in nested scopes (IF, CYCLE) in multi-function files
- **Test**: 
  ```powershell
  dotnet run --project src\PowerScript.CLI -- "parser-bug-reproductions\BUG_1_NestedScopeAssignment.ps"
  ```

#### BUG #2: CYCLE Scope Variable Access
- **File**: `BUG_2_CycleScopeVariableAccess.ps`
- **Error**: `Variable 'result' is not declared`
- **Impact**: Blocks 37 MathLib tests
- **Example**: Variables from function scope not accessible in CYCLE loops
- **Root Cause**: Scope resolution fails for CYCLE blocks when multiple functions defined
- **Test**:
  ```powershell
  dotnet run --project src\PowerScript.CLI -- "parser-bug-reproductions\BUG_2_CycleScopeVariableAccess.ps"
  ```

#### BUG #8: String Literals in RETURN Statements (NEW)
- **File**: `BUG_8_StringLiteralsInReturn.ps`
- **Error**: `expected identifier or value or parenthesisopen`
- **Impact**: Blocks 22 ConversionLib tests + others
- **Example**: `RETURN "true"` inside IF block fails when function returns STRING
- **Root Cause**: Parser fails on RETURN statements with string literals in nested scopes
- **Test**:
  ```powershell
  dotnet run --project src\PowerScript.CLI -- "parser-bug-reproductions\BUG_8_StringLiteralsInReturn.ps"
  ```

### HIGH Priority Bugs (Block 20-30 tests)

#### BUG #3: Nested Function Calls
- **File**: `BUG_3_NestedFunctionCalls.ps`
- **Error**: `Expected ',' or ')' but found 'ParenthesisOpen'`
- **Impact**: Blocks ~20 tests
- **Example**: `ABS(MAKE_NEGATIVE(value))` not supported
- **Workaround**: Use intermediate variables
- **Test**:
  ```powershell
  dotnet run --project src\PowerScript.CLI -- "parser-bug-reproductions\BUG_3_NestedFunctionCalls.ps"
  ```

### MEDIUM Priority Bugs (Block <10 tests)

#### BUG #4: String Indexing
- **File**: `BUG_4_StringIndexing.ps`
- **Impact**: Blocks 6 character validation functions
- **Example**: `c = s[i]` syntax not supported
- **Workaround**: Limited - can use Substring but not for character iteration
- **Note**: This is a language feature, not a bug

#### BUG #5: Reserved Keywords in Parameters
- **File**: `BUG_5_ReservedKeywords.ps`
- **Impact**: Naming restrictions, workaround exists
- **Example**: `expectedLen` tokenized as `EXPECTED` + `LEN`
- **Workaround**: Use different parameter names (target, threshold, etc.)
- **Test**:
  ```powershell
  dotnet run --project src\PowerScript.CLI -- "parser-bug-reproductions\BUG_5_ReservedKeywords.ps"
  ```

#### BUG #6: Negative Literals in Function Calls
- **File**: `BUG_6_NegativeLiterals.ps`
- **Impact**: Blocks 1 test
- **Example**: Cannot pass `-5` directly as function argument
- **Workaround**: Use variable: `INT negFive = 0 - 5; ABS(negFive)`
- **Test**:
  ```powershell
  dotnet run --project src\PowerScript.CLI -- "parser-bug-reproductions\BUG_6_NegativeLiterals.ps"
  ```

#### BUG #7: Conditional CYCLE Loops
- **File**: `BUG_7_ConditionalCycle.ps`
- **Impact**: Language feature limitation
- **Example**: Only `CYCLE count AS i` works, not `CYCLE condition`
- **Note**: This may be by design, not a bug

## Bug Statistics

- **Total bugs documented**: 8
- **Critical bugs**: 3 (Bugs #1, #2, #8)
- **High priority bugs**: 1 (Bug #3)
- **Medium priority bugs**: 4 (Bugs #4, #5, #6, #7)
- **Total tests blocked**: ~113 tests (34% of all tests, 64% of StandardLibrary)
- **Current test coverage**: 195/328 (59.5%)
- **Potential with fixes**: ~306/328 (93%)

## Priority Recommendations

**Fix in this order for maximum test impact:**

1. **BUG #1** (Variable Assignment in Nested Scopes) → +48 tests
2. **BUG #2** (CYCLE Scope Variable Access) → +37 tests  
3. **BUG #8** (Multi-Condition OR) → +22 tests
4. **BUG #3** (Nested Function Calls) → +20 tests
5. Remaining bugs → +6 tests

**Total potential unlock**: 133 tests → 93% overall coverage

## Testing After Fixes

Once a bug is fixed, you can verify by:

1. Compiling the reproduction file (should succeed)
2. Running the affected test category:
   - Bug #1: StringLib, MathLib, ConversionLib
   - Bug #2: MathLib
   - Bug #3: Various categories
   - Bug #8: ConversionLib

Example:
```powershell
# After fixing Bug #1
dotnet run --project src\PowerScript.CLI -- "parser-bug-reproductions\BUG_1_NestedScopeAssignment.ps"
# Should compile successfully

# Then verify tests
dotnet test tests\PowerScript.StandardLibrary.Tests --filter "MathLib" --no-build
# Should see improvement in pass rate
```

## Related Documentation

- **PARSER_BUGS.md** - Comprehensive documentation of all bugs with examples and analysis
- **VALIDATION_LIB_STATUS.md** - ValidationLib implementation status and workarounds
- **SESSION_SUMMARY.md** - Full session report with statistics and recommendations

## Notes for Parser Developers

- All bugs have been tested on PowerScript compiler as of October 2025
- Each reproduction file is minimal and self-contained
- Comments indicate the exact line causing the error
- Some bugs only manifest in multi-function files (especially Bugs #1 and #2)
- Bug #8 was discovered during ConversionLib investigation
- See PARSER_BUGS.md for detailed technical analysis and suspected root causes
