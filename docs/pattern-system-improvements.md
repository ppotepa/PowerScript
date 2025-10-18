# Pattern System Robustness Improvements

## Overview
This document describes the improvements made to the PowerScript pattern system to make it more robust and prevent issues when adding new patterns or making changes.

## Problems Addressed

### 1. Lack of Validation
- **Problem**: Patterns could be registered without validation, leading to runtime errors
- **Solution**: Created comprehensive `PatternValidator` class that validates patterns before registration

### 2. Poor Error Diagnostics
- **Problem**: When patterns failed to match, there was no way to debug why
- **Solution**: Created `PatternDiagnostics` class with detailed failure analysis

### 3. No Conflict Detection  
- **Problem**: New patterns could conflict with existing ones without warning
- **Solution**: Implemented conflict detection that checks for ambiguous patterns

### 4. Unsafe Registration
- **Problem**: Pattern registration could fail silently or crash the system
- **Solution**: Created `PatternExtensionSandbox` with safe registration and error handling

## New Components

### PatternValidator (`src/PowerScript.Parser/Validators/PatternValidator.cs`)

Validates patterns before registration:

```csharp
// Example usage
var errors = PatternValidator.ValidatePattern(pattern, transformation);
if (errors.Count > 0)
{
    // Handle validation errors
}
```

**Features:**
- Checks for reserved keyword conflicts
- Validates transformation format (must be valid function call)
- Ensures parameter consistency between pattern and transformation
- Validates type constraints
- Detects ambiguous patterns

**Methods:**
- `ValidatePattern(pattern, transformation)` - Comprehensive validation
- `CheckForConflicts(pattern, existingPatterns)` - Conflict detection
- `ValidateTypeConstraints(pattern)` - Type annotation validation

### PatternDiagnostics (`src/PowerScript.Parser/Diagnostics/PatternDiagnostics.cs`)

Provides debugging tools for pattern matching:

```csharp
// Example usage
var diagnosis = PatternDiagnostics.DiagnosePatternMatchFailure(token, pattern);
Console.WriteLine(diagnosis);
```

**Features:**
- Shows token stream with types and values
- Compares actual tokens against expected pattern
- Finds closest matching patterns for suggestions
- Logs pattern registry for inspection
- Validates token sequences against patterns

**Methods:**
- `DiagnosePatternMatchFailure(token, pattern)` - Detailed failure analysis
- `FindClosestPatternMatch(token, patterns)` - Finds similar patterns
- `LogPatternRegistry(patterns)` - Registry inspection
- `ValidateTokenSequence(token, pattern)` - Token-by-token validation

### PatternExtensionSandbox (`src/PowerScript.Parser/Extensions/PatternExtensionSandbox.cs`)

Safe registration system with validation and error handling:

```csharp
// Example usage
var sandbox = new PatternExtensionSandbox();
bool success = sandbox.RegisterPattern(pattern, transformation);

// Or with error throwing
sandbox.RegisterPattern(pattern, transformation, throwOnError: true);

// Batch registration from file
var result = sandbox.RegisterPatternsFromFile("patterns.psx");
Console.WriteLine($"Success: {result.SuccessCount}, Failed: {result.FailureCount}");
```

**Features:**
- Multi-step validation before registration
- Conflict warnings (non-blocking)
- Type constraint validation
- Batch registration from files
- Registration audit log
- Custom exceptions for different failure types

**Methods:**
- `RegisterPattern(pattern, transformation, throwOnError)` - Safe single registration
- `RegisterPatternsFromFile(filePath)` - Batch registration
- `GetRegistrationLog()` - Audit log access
- `ClearLog()` - Log management

**Exceptions:**
- `PatternValidationException` - Pattern syntax or format errors
- `PatternConflictException` - Pattern conflicts detected
- `PatternRegistrationException` - Unexpected registration errors

## Usage Examples

### Basic Validation

```csharp
using PowerScript.Parser.Validators;

// Validate before registering
var errors = PatternValidator.ValidatePattern(
    "FILTER $array WHERE $condition",
    "ARRAY_FILTER($array, $condition)"
);

if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"ERROR: {error}");
    }
}
```

### Safe Registration

```csharp
using PowerScript.Parser.Extensions;

var sandbox = new PatternExtensionSandbox();

// Register with automatic error handling
if (!sandbox.RegisterPattern(
    "MULTIPLY $a:INT BY $b:INT",
    "MULTIPLY($a, $b)"))
{
    Console.WriteLine("Registration failed - see logs");
}

// Register with exception on error
try
{
    sandbox.RegisterPattern(
        "INVALID PATTERN",
        "BAD()",
        throwOnError: true);
}
catch (PatternValidationException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
    foreach (var error in ex.ValidationErrors)
    {
        Console.WriteLine($"  - {error}");
    }
}
```

### Debugging Pattern Failures

```csharp
using PowerScript.Parser.Diagnostics;

// When a pattern fails to match
var diagnosis = PatternDiagnostics.DiagnosePatternMatchFailure(currentToken, pattern);
LoggerService.Logger.Error(diagnosis);

// Find similar patterns
var closestMatch = PatternDiagnostics.FindClosestPatternMatch(
    currentToken,
    CustomSyntaxRegistry.Instance.GetPatternTransformations()
);

if (closestMatch != null)
{
    Console.WriteLine($"Did you mean: {closestMatch.Pattern}?");
}
```

### Batch Registration

```csharp
var sandbox = new PatternExtensionSandbox();
var result = sandbox.RegisterPatternsFromFile("custom_patterns.psx");

Console.WriteLine($"Registered: {result.SuccessCount}");
Console.WriteLine($"Failed: {result.FailureCount}");

if (!result.IsSuccess)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"ERROR: {error}");
    }
}

// View audit log
var log = sandbox.GetRegistrationLog();
foreach (var entry in log)
{
    Console.WriteLine(entry);
}
```

## Benefits

### 1. Early Error Detection
- Patterns are validated before registration
- Errors are caught at development time, not runtime
- Clear error messages guide developers to fix issues

### 2. Better Debugging
- Detailed diagnostics show exactly why patterns fail
- Token streams are visualized for inspection
- Closest matches suggest alternatives

### 3. Conflict Prevention
- Ambiguous patterns are detected automatically
- Warnings are issued before problems occur
- Registry can be inspected to understand existing patterns

### 4. Safe Extensions
- Third-party patterns can be added safely
- Batch operations handle errors gracefully
- Audit logs track all registrations

### 5. Type Safety
- Type constraints are validated
- Parameter mismatches are caught early
- Invalid types are rejected

## Integration with Existing Code

The new components are designed to work alongside existing pattern registration:

```csharp
// Old way (still works)
CustomSyntaxRegistry.Instance.Register(new SyntaxTransformation
{
    Pattern = "SORT $array",
    Transformation = "ARRAY_SORT($array)"
});

// New way (with validation)
var sandbox = new PatternExtensionSandbox();
sandbox.RegisterPattern("SORT $array", "ARRAY_SORT($array)");
```

## Future Improvements

1. **Pattern Testing Framework**: Unit tests for patterns
2. **Pattern Documentation Generator**: Auto-generate docs from registered patterns
3. **Pattern Performance Profiling**: Track which patterns are used most
4. **Pattern Versioning**: Support multiple versions of patterns
5. **Pattern Deprecation**: Mark patterns as deprecated with warnings

## Conclusion

These improvements make the PowerScript pattern system significantly more robust. When adding new patterns or making changes, developers now have:

- **Validation**: Catch errors before they cause problems
- **Diagnostics**: Understand why patterns fail
- **Safety**: Register patterns without breaking the system
- **Confidence**: Know that patterns will work correctly

This foundation ensures that as the pattern system grows, it remains maintainable and reliable.
