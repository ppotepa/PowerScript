# Token Folder Structure Proposal

## Current Structure (Flat)
All tokens are currently in a single folder, making it harder to navigate and understand token categories.

## Proposed Structure

```
Tokens/
├── Base/
│   ├── Token.cs                    # Abstract base class for all tokens
│   └── TokenRoot.cs                # Root token of the tree
│
├── Interfaces/
│   ├── IKeyWordToken.cs            # Interface for keyword tokens
│   ├── IValue.cs                   # Interface for value-bearing tokens
│   ├── IVariable.cs                # Interface for variable tokens
│   └── IScope.cs                   # Interface for scope-related tokens
│
├── Keywords/
│   ├── ReturnKeywordToken.cs      # 'return' keyword
│   └── FunctionToken.cs            # 'function' keyword (or similar)
│
├── Identifiers/
│   ├── IdentifierToken.cs          # Generic identifier
│   └── ParameterArrayToken.cs      # Parameter array identifier
│
├── Operators/
│   └── CommaSeparatorToken.cs      # Comma separator operator
│
├── Delimiters/
│   ├── ParenthesisOpen.cs          # Opening parenthesis '('
│   └── ParenthesisClosed.cs        # Closing parenthesis ')'
│
├── Scoping/
│   ├── ScopeToken.cs               # Scope definition
│   └── ScopeEnd.cs                 # Scope terminator
│
├── Values/
│   └── ValueToken.cs               # Value/literal token
│
└── Raw/
    ├── RawToken.cs                 # Unprocessed token
    └── RawTokenCollection.cs       # Collection of raw tokens
```

## Rationale

### 1. **Base/** - Foundation Classes
Contains the core token classes that all other tokens inherit from.
- Easy to find fundamental types
- Clear separation of base functionality

### 2. **Interfaces/** - Contracts
All token interfaces in one place for easy reference.
- Developers can quickly see all available token behaviors
- Clear contract definitions
- Easy to implement new token types

### 3. **Keywords/** - Reserved Words
Language keywords like `return`, `function`, etc.
- Reserved words that have special meaning
- Easy to add new keywords as the language grows
- Clear distinction from identifiers

### 4. **Identifiers/** - Named Elements
User-defined names for functions, variables, parameters, etc.
- Tokens that represent named elements in code
- Separate from keywords which are reserved

### 5. **Operators/** - Operations
Operators and separators like commas, arithmetic operators, etc.
- Could be expanded with: `+`, `-`, `*`, `/`, `=`, `==`, etc.
- Clear category for operational tokens

### 6. **Delimiters/** - Structural Markers
Parentheses, brackets, braces, etc.
- Tokens that define structure but aren't operators
- Future additions: `{`, `}`, `[`, `]`, etc.

### 7. **Scoping/** - Scope Management
Tokens related to code blocks and scoping.
- Manages code organization and variable scope
- Important for structured programming

### 8. **Values/** - Literals
Literal values and constants.
- Could be expanded to: NumberToken, StringToken, BooleanToken, etc.
- Clear separation from variables and identifiers

### 9. **Raw/** - Preprocessing
Already organized - raw/unprocessed tokens before classification.
- First stage of tokenization
- Separate from processed tokens

## Alternative Structure (By Usage Pattern)

If you prefer grouping by usage rather than type:

```
Tokens/
├── Core/              # Token, TokenRoot, Interfaces
├── Syntax/            # Keywords, Operators, Delimiters
├── Semantic/          # Identifiers, Values, Variables
├── Structure/         # Scoping, Parentheses
└── Raw/              # RawToken, RawTokenCollection
```

## Alternative Structure (Minimal)

If you prefer fewer folders:

```
Tokens/
├── Base/              # Token, TokenRoot
├── Contracts/         # All interfaces
├── Keywords/          # ReturnKeywordToken, FunctionToken
├── Symbols/           # Operators, Delimiters (CommaSeparatorToken, Parentheses)
├── Identifiers/       # IdentifierToken, ParameterArrayToken
├── Values/            # ValueToken
├── Scoping/           # ScopeToken, ScopeEnd
└── Raw/              # RawToken, RawTokenCollection
```

## Recommended Approach

**I recommend the first proposed structure** (9 folders) because:
1. ✅ Clear categorization
2. ✅ Easy to locate specific token types
3. ✅ Scalable for future tokens
4. ✅ Follows common compiler/parser patterns
5. ✅ Self-documenting organization

## Future Additions by Category

As your tokenizer grows, here's where new tokens would fit:

- **Keywords/**: `if`, `else`, `while`, `for`, `class`, `var`, `const`, etc.
- **Operators/**: `+`, `-`, `*`, `/`, `=`, `==`, `!=`, `&&`, `||`, etc.
- **Delimiters/**: `{`, `}`, `[`, `]`, `:`, `;`, etc.
- **Values/**: `NumberToken`, `StringToken`, `BooleanToken`, `NullToken`, etc.
- **Identifiers/**: `ClassNameToken`, `MethodNameToken`, `VariableNameToken`, etc.

## Migration Notes

When reorganizing:
1. Move files to new folders
2. Update namespace declarations in each file
3. Update using statements in files that reference moved tokens
4. Update TokenTree.cs to handle new namespaces
5. Run tests to ensure everything still works
