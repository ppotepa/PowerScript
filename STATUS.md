# Custom Syntax Extensions - Implementation Status

## Completed ✅

### 1. Object System Foundation
- **Object Literals**: `{name = "John", age = 30}`
- **Type Annotations**: `{x = 10} AS Point`
- **Strict Types**: `{x = 10} AS Point!` (non-extendable)
- **Property Access**: `obj.property` with chaining support
- **Runtime Class**: `PowerScriptObject` with property storage
- **AST Nodes**: `ObjectLiteralExpression`, `PropertyAccessExpression`
- **Tests**: All passing (211/213 baseline maintained)

### 2. Token System Refactoring
- **Renamed**: `NamespaceOperatorToken` → `CustomSyntaxOperatorToken`
- **Purpose**: `::` now exclusively for custom syntax extensions
- **NET Syntax**: Changed from `NET::Class.Method` to `NET.Class.Method`
- **Dot Operator**: Now handles both objects and .NET access
- **Tests**: All passing, no regressions

### 3. Core Infrastructure
- **CustomSyntaxRegistry**: Singleton registry for syntax transformations
- **SyntaxTransformation**: Model for operator and pattern-based syntax
- **Design Document**: Complete specification in `docs/custom-syntax-design.md`

### 4. Documentation & Examples
- **Example Files**: `arrays.psx`, `strings.psx`, `objects.psx`
- **Demo Script**: `example_custom_syntax.ps` showing usage
- **README Updated**: Custom syntax section with examples
- **Help Text**: Updated all references from `::` to `.` for NET access

## In Progress 🔄

### Phase 1: PSX File Parser
- [ ] Create `PSXFileParser` class
- [ ] Parse `SYNTAX` keyword
- [ ] Extract pattern and transformation templates
- [ ] Identify captured variables ($target, $args, etc.)
- [ ] Register transformations with CustomSyntaxRegistry

### Phase 2: Operator Extension Processing
- [ ] Detect `::` operator in expression parsing
- [ ] Look up method name in CustomSyntaxRegistry
- [ ] Transform to function call expression
- [ ] Support chaining: `array::Sort()::First()`
- [ ] Test with simple array operations

### Phase 3: Pattern Extension Processing
- [ ] Create pattern matching engine
- [ ] Parse pattern keywords (FILTER, MAP, TAKE, etc.)
- [ ] Extract captured variables from usage
- [ ] Transform to function call with correct argument order
- [ ] Test with filter/map/take patterns

### Phase 4: LINK Statement Integration
- [ ] Update `LinkStatementProcessor` to detect .psx files
- [ ] Load and parse .psx files via PSXFileParser
- [ ] Cache parsed syntax transformations
- [ ] Prevent duplicate loading of same .psx file
- [ ] Auto-discover .psx files in stdlib/ and extensions/

## Pending ⏳

### Testing & Validation
- [ ] Create unit tests for CustomSyntaxRegistry
- [ ] Create unit tests for PSXFileParser
- [ ] Create integration tests with example .psx files
- [ ] Test error handling (syntax errors, name conflicts)
- [ ] Performance testing with large .psx files

### Standard Library Extensions
- [ ] Create `stdlib/arrays.psx` with array operations
- [ ] Create `stdlib/strings.psx` with string operations
- [ ] Create `stdlib/objects.psx` with object operations
- [ ] Create `stdlib/math.psx` with math extensions
- [ ] Document all standard extensions

### Advanced Features
- [ ] Support for multiple arguments in patterns
- [ ] Support for optional arguments
- [ ] Support for variadic arguments
- [ ] Type checking for custom syntax
- [ ] Syntax extension composition (one .psx using another)

## Technical Decisions

### Why `::` for Custom Syntax?
- **Familiar**: Similar to C++, Rust, Scala namespace operators
- **Distinct**: Clearly different from dot (`.`) for built-in members
- **Available**: No longer needed for NET access (uses `.` now)
- **Readable**: `array::Sort()` reads as "array syntax Sort"

### Why Dot for NET Access?
- **Consistency**: Objects use dot, NET uses dot
- **Simplicity**: One operator for all member access
- **Intuitive**: `NET.System.Console.WriteLine` matches C# syntax
- **Redundancy**: `::` after NET was unnecessary

### Why Pattern-Based Extensions?
- **Readability**: `TAKE 5 FROM array` is more natural than `array.Take(5)`
- **DSL Support**: Enables domain-specific language patterns
- **Flexibility**: Can match multi-token patterns
- **PowerShell-like**: Matches PowerScript's command-oriented style

## File Structure

```
tokenez/
├── docs/
│   └── custom-syntax-design.md          # Complete specification
├── examples/
│   ├── arrays.psx                       # Array extension examples
│   ├── strings.psx                      # String extension examples
│   ├── objects.psx                      # Object extension examples
│   └── example_custom_syntax.ps         # Usage demonstration
├── src/
│   ├── PowerScript.Compiler/
│   │   ├── CustomSyntaxRegistry.cs      # ✅ Registry for transformations
│   │   └── Models/
│   │       └── SyntaxTransformation.cs  # ✅ Transformation model
│   ├── PowerScript.Core/
│   │   ├── AST/Expressions/
│   │   │   ├── ObjectLiteralExpression.cs    # ✅ Object literals
│   │   │   └── PropertyAccessExpression.cs   # ✅ Property access
│   │   └── Syntax/Tokens/Operators/
│   │       └── CustomSyntaxOperatorToken.cs  # ✅ :: operator
│   ├── PowerScript.Parser/
│   │   ├── PSXFileParser.cs                  # ⏳ TODO
│   │   └── Processors/
│   │       ├── CustomSyntaxProcessor.cs      # ⏳ TODO
│   │       └── PatternSyntaxProcessor.cs     # ⏳ TODO
│   └── PowerScript.Runtime/
│       └── Models/
│           └── PowerScriptObject.cs          # ✅ Object runtime
└── stdlib/
    └── (future .psx files)                   # ⏳ TODO
```

## Next Steps

1. **Implement PSXFileParser**
   - Parse `SYNTAX` lines
   - Extract patterns and transformations
   - Register with CustomSyntaxRegistry

2. **Add :: Operator Handling**
   - Detect in ExpressionParser
   - Look up transformation
   - Transform to FunctionCallExpression

3. **Test with Simple Extensions**
   - Load arrays.psx
   - Test `numbers::Sort()`
   - Verify transformation works

4. **Add Pattern Matching**
   - Create pattern matcher
   - Handle `TAKE $count FROM $array`
   - Transform to function calls

5. **Integration Testing**
   - Run example_custom_syntax.ps
   - Verify all syntax works
   - Fix any issues

## Success Criteria

- [ ] Can load .psx files with LINK statement
- [ ] `array::Sort()` transforms to `ARRAY_SORT(array)`
- [ ] `TAKE 3 FROM array` transforms to `ARRAY_TAKE(array, 3)`
- [ ] Chaining works: `array::Sort()::First()`
- [ ] All existing tests still pass (211/213)
- [ ] New tests for custom syntax pass
- [ ] Example script runs successfully

## Timeline Estimate

- **PSXFileParser**: 2-3 hours
- **Operator Extensions**: 2-3 hours
- **Pattern Extensions**: 4-5 hours
- **Testing & Debugging**: 3-4 hours
- **Documentation**: 1-2 hours
- **Total**: ~15-20 hours

## Current Commit Status

Branch: `features/custom-syntax-operators`

Commits:
1. `1c13eab` - feat: implement object literals with type annotations
2. `84527cc` - refactor: repurpose :: operator for custom syntax extensions
3. `6df54f9` - docs: add custom syntax extension documentation and examples

Ready for: Phase 1 implementation (PSXFileParser)
