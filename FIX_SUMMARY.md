# PowerScript Interpreter Fix Summary

## Issue: Infinite Recursion in Test_3_15_PowerFunction

### Symptom
Test_3_15_PowerFunction was causing infinite recursion when calling `power(2, 8)`, resulting in "Maximum recursion depth exceeded" error.

### Root Cause
**Parsing Bug in `ScopeBuilder.BuildScope`**

The `ScopeBuilder.BuildScope` method was not stopping at `ScopeEndToken` (`}`), causing it to parse statements beyond the scope's closing brace. This resulted in:

1. When parsing a CYCLE loop body, the parser would continue past the loop's closing `}`
2. Statements following the loop (including the `RETURN` statement and even the top-level `PRINT power(2, 8)` call) were incorrectly included in the loop body
3. Each loop iteration would execute `PRINT power(2, 8)`, causing recursive function calls
4. This created an infinite recursion chain: `power(2,8)` → loop iteration 0 → `PRINT power(2,8)` → `power(2,8)` → ...

### The Fix

**File: `Tree/Builders/ScopeBuilder.cs` (Line 43)**

Changed:
```csharp
while (currentToken is not null)
```

To:
```csharp
while (currentToken is not null and not Tokens.Scoping.ScopeEndToken)
```

This ensures that `BuildScope` stops processing tokens when it encounters a scope end token (`}`), preventing it from parsing statements that belong to outer scopes.

### Verification

The fix was verified through systematic debugging:

1. **Initial Investigation**: Added recursion tracking and call stack logging
2. **Pattern Discovery**: Found that recursion triggered when executing CycleLoopStatement
3. **Statement Tracing**: Discovered loop body contained 4 statements instead of 2:
   - ✓ `FLEX result = result * base` (correct)
   - ✓ `FLEX i = i + 1` (correct)
   - ✗ `RETURN result` (belongs to function body, not loop)
   - ✗ `PRINT power(2, 8)` (belongs to main code, not loop)
4. **Root Cause Identification**: Traced to `ScopeBuilder.BuildScope` not respecting scope boundaries
5. **Fix Validation**: After fix, loop body correctly contains only 2 statements

### Test Results

**Before Fix**: 45/47 tests passing (95.7%)
- Test_3_2_MatrixOperations: Failed (missing file - expected)
- Test_3_15_PowerFunction: Failed (infinite recursion)

**After Fix**: 46/47 tests passing (97.9%)
- Test_3_2_MatrixOperations: Failed (missing file - expected)
- Test_3_15_PowerFunction: ✅ **PASSED**

### Impact

This fix resolves a critical parsing bug that affected any code with nested scopes (loops within functions, conditional blocks, etc.). The bug would cause statements following a scope to be incorrectly included in that scope, leading to unexpected behavior or infinite recursion.

### Additional Changes

As part of the debugging process, the following enhancements were implemented:

1. **Function Call Support in PRINT Statements** (`PrintStatementProcessor.cs`):
   - Added detection for `identifier(...)` patterns
   - Creates `FunctionCallExpression` for function calls in PRINT

2. **Function Call Interpreter** (`PowerScriptCompiler.cs`):
   - Implemented `EvaluateFunctionCall` method
   - Added `ParseFunctionArguments` helper
   - Added recursion depth tracking (max 1000 calls)
   - Added call stack for debugging

3. **Recursion Protection** (`PowerScriptCompiler.cs`):
   - Maximum recursion depth: 1000
   - Clear error messages with call stack trace
   - Proper cleanup in finally blocks

### Files Modified

1. `Tree/Builders/ScopeBuilder.cs` - **Primary fix** (scope boundary parsing)
2. `Tree/PowerScriptCompiler.cs` - Function call interpreter with recursion protection
3. `Tree/Builders/PrintStatementProcessor.cs` - Function call parsing in PRINT + null safety fix

### Final Status

✅ **46 out of 47 tests passing (97.9%)**

**Passing Tests**:
- All basic feature tests (1_1 through 1_12) ✓
- All intermediate tests (2_1 through 2_10) ✓
- All complex tests except 3_2 (3_1, 3_3 through 3_25) ✓

**Remaining Failure**:
- Test_3_2_MatrixOperations: Expected failure - script file `3_2_matrix_operations.ps` does not exist

### Test Case Details

**Test_3_15_PowerFunction** tests:
```powerscript
FUNCTION power RETURNS NUMBER WITH base, exp {
    IF exp == 0 { RETURN 1 }
    FLEX result = base
    FLEX i = 1
    CYCLE exp - 1 {
        FLEX result = result * base
        FLEX i = i + 1
    }
    RETURN result
}
PRINT power(2, 8)
```

Expected output: `256`
Result: ✅ **Test now passes**

---

**Date**: 2024 (session)
**Fix Type**: Critical Bug Fix (Parsing)
**Status**: ✅ Complete
