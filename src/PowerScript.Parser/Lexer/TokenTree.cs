using PowerScript.Common.Logging;
using PowerScript.Core.AST;
using PowerScript.Core.DotNet;
using PowerScript.Core.Syntax;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Prompt;

namespace PowerScript.Parser.Lexer;

/// <summary>
///     Represents the complete token tree built from user input.
///     Handles tokenization, linking, and scope building.
/// </summary>
public partial class TokenTree
{
    /// <summary>
    ///     Static mapping of exact text strings to their corresponding token types.
    ///     This is checked first before the reflection-based TokenTypes dictionary.
    ///     Used for operators, delimiters, and core keywords that need exact matching.
    /// </summary>
    private static readonly Dictionary<string, Type> _map = new()
    {
        { "LINK", typeof(LinkKeywordToken) }, // Library/file import keyword
        { "FUNCTION", typeof(FunctionToken) }, // Function declaration keyword
        { "RETURN", typeof(ReturnKeywordToken) }, // Return statement keyword
        { "RETURNS", typeof(ReturnsKeywordToken) }, // Returns keyword (alt syntax)
        { "WITH", typeof(WithKeywordToken) }, // With keyword (alt syntax)
        // PRINT is now a stdlib function, not a keyword
        { "EXECUTE", typeof(ExecuteKeywordToken) }, // Execute script file keyword
        { "NET", typeof(NetKeywordToken) }, // .NET access keyword
        { "#", typeof(NetKeywordToken) }, // .NET access shorthand (# for C#)
        { "VAR", typeof(VarKeywordToken) }, // Variable declaration keyword
        { "FLEX", typeof(FlexKeywordToken) }, // Dynamic variable declaration keyword
        { "CYCLE", typeof(CycleKeywordToken) }, // Loop keyword (foreach equivalent)
        { "IN", typeof(InKeywordToken) }, // In keyword for loops
        { "AS", typeof(AsKeywordToken) }, // As keyword for renaming
        { "WHILE", typeof(WhileKeywordToken) }, // While keyword for CYCLE WHILE loops
        { "TO", typeof(ToKeywordToken) }, // To keyword for CYCLE range loops
        { "FROM", typeof(FromKeywordToken) }, // From keyword for CYCLE RANGE FROM ... TO loops
        { "ELEMENTS", typeof(ElementsKeywordToken) }, // Elements keyword for CYCLE ELEMENTS OF loops
        { "RANGE", typeof(RangeKeywordToken) }, // Range keyword for CYCLE RANGE FROM ... TO loops
        { "OF", typeof(OfKeywordToken) }, // Of keyword for array literals (CHAIN OF [...])
        { "IF", typeof(IfKeywordToken) }, // Conditional statement keyword
        { "ELSE", typeof(ElseKeywordToken) }, // Else block keyword
        { "AND", typeof(AndKeywordToken) }, // Logical AND operator
        { "OR", typeof(OrKeywordToken) }, // Logical OR operator
        { "INT", typeof(IntToken) }, // Integer type keyword
        { "BOOL", typeof(BoolToken) }, // Boolean type keyword
        { "BOOLEAN", typeof(BoolToken) }, // Boolean type keyword (alias)
        { "TRUE", typeof(TrueToken) }, // Boolean true literal
        { "FALSE", typeof(FalseToken) }, // Boolean false literal
        { "PREC", typeof(PrecToken) }, // Precision/float type keyword
        { "CHAR", typeof(CharToken) }, // Character type keyword
        { "STRING", typeof(StringToken) }, // String type keyword (CHAR CHAIN)
        { "NUMBER", typeof(NumberToken) }, // Number type keyword
        { "CHAIN", typeof(ChainToken) }, // Collection/array type modifier
        { "{", typeof(ScopeStartToken) }, // Scope/block start (also object literal start)
        { "}", typeof(ScopeEndToken) }, // Scope/block end (also object literal end)
        { "![", typeof(CustomSyntaxBlockOpen) }, // Custom syntax block open
        { "[", typeof(BracketOpen) }, // Return type bracket open
        { "]", typeof(BracketClosed) }, // Return type bracket close
        { "!", typeof(ExclamationToken) }, // Exclamation mark for strict types
        { ",", typeof(CommaToken) }, // Comma delimiter
        { "+", typeof(PlusToken) }, // Addition operator
        { "->", typeof(ArrowToken) }, // Arrow operator for .NET member access
        { "-", typeof(MinusToken) }, // Subtraction operator
        { "*", typeof(MultiplyToken) }, // Multiplication operator
        { "/", typeof(DivideToken) }, // Division operator
        { "%", typeof(ModuloToken) }, // Modulo operator
        { "=", typeof(EqualsToken) }, // Assignment operator
        { ">", typeof(GreaterThanToken) }, // Greater than comparison
        { "<", typeof(LessThanToken) }, // Less than comparison
        { ">=", typeof(GreaterThanOrEqualToken) }, // Greater than or equal
        { "<=", typeof(LessThanOrEqualToken) }, // Less than or equal
        { "==", typeof(EqualsEqualsToken) }, // Equality comparison
        { "!=", typeof(NotEqualsToken) }, // Not equal comparison
        { "::", typeof(CustomSyntaxOperatorToken) }, // Custom syntax operator for extensions
        { ".", typeof(DotToken) } // Dot operator for member access
    };

    /// <summary>The root scope containing all top-level declarations</summary>
    public Scope? RootScope { get; private set; }

    /// <summary>All tokens in the tree</summary>
    public Token[]? Tokens { get; private set; }

    /// <summary>The .NET linker for resolving types from linked namespaces</summary>
    public IDotNetLinker DotNetLinker { get; }

    /// <summary>Stack tracking the current lexical context during tokenization</summary>
    private Stack<LexicalContext> _contextStack = new Stack<LexicalContext>();

    /// <summary>
    ///     Creates the token tree from a user prompt.
    ///     Steps: 1) Convert raw tokens to typed tokens, 2) Link tokens bidirectionally, 3) Build scope hierarchy
    /// </summary>
    public TokenTree Create(UserPrompt prompt)
    {
        LoggerService.Logger.Info("");
        LoggerService.Logger.Info("=== Building Token Tree ===");
        LoggerService.Logger.Info($"Processing: '{prompt.WrappedPrompt}'");
        LoggerService.Logger.Info("");

        // Initialize context stack with root context
        _contextStack.Clear();
        _contextStack.Push(new RootContext());

        // Step 1: Convert raw text tokens into strongly-typed token objects
        LoggerService.Logger.Debug($"Raw tokens count: {prompt.RawTokens.Length}");
        var rawTokenList = prompt.RawTokens.ToList();
        Token[] tokens = [.. rawTokenList.Select((token, index) =>
        {
            RawToken? previousToken = index > 0 ? rawTokenList[index - 1] : null;
            RawToken? nextToken = index < rawTokenList.Count - 1 ? rawTokenList[index + 1] : null;
            return ToToken(token, index, previousToken, nextToken);
        }).Where(t => t != null)]; // Filter out null tokens (consumed as part of compound tokens)
        LoggerService.Logger.Info($"Tokens created: {tokens.Length}");

        // Step 2: Link all tokens with Previous/Next references for easy navigation
        tokens = [.. tokens.Select((element, index) => Link(element, index, tokens)).ToArray()];
        LoggerService.Logger.Info("Tokens linked");

        // Step 3: Build the scope hierarchy starting from root scope
        Scope scope = CreateScope(tokens[0], new Scope("ROOT"));
        LoggerService.Logger.Info("Scope creation complete");
        LoggerService.Logger.Info("");

        // Store the results
        Tokens = tokens;
        RootScope = scope;

        return this;
    }

    /// <summary>
    ///     Visualizes the entire token tree structure.
    ///     Shows all scopes, declarations, statements, and their relationships.
    /// </summary>
    public void Visualize()
    {
        LoggerService.Logger.Info("");
        LoggerService.Logger.Info("╔════════════════════════════════════════╗");
        LoggerService.Logger.Info("║       TOKEN TREE VISUALIZATION         ║");
        LoggerService.Logger.Info("╚════════════════════════════════════════╝");
        LoggerService.Logger.Info("");

        if (RootScope != null)
        {
            RootScope.Visualize();
        }
        else
        {
            LoggerService.Logger.Error("No root scope found!");
        }

        LoggerService.Logger.Info("");
    }

    /// <summary>
    ///     Links tokens bidirectionally to form a doubly-linked list.
    ///     This allows processors to easily navigate forward (Next) and backward (Prev).
    /// </summary>
    private static Token Link(Token token, int index, Token[] tokens)
    {
        // First token: only set Next
        if (index is 0)
        {
            token.Next = tokens[index + 1];
        }
        // Middle tokens: set both Prev and Next
        else if (index < tokens.Length - 1)
        {
            token.Next = tokens[index + 1];
            token.Prev = tokens[index - 1];
        }
        // Last token: only set Prev
        else if (tokens.Length - 1 == index)
        {
            token.Prev = tokens[index - 1];
        }

        return token;
    }

    /// <summary>
    ///     Checks if the next token is an opening parenthesis.
    ///     Used to detect function calls: DIVIDE(a, b) vs pattern syntax: DIVIDE a BY b
    /// </summary>
    private static bool IsFollowedByParen(RawToken? nextToken)
    {
        return nextToken?.Text == "(";
    }

    private static bool IsFollowedByEquals(RawToken? nextToken)
    {
        return nextToken?.Text == "=";
    }

    /// <summary>
    ///     Determines if a token is in a valid context to be recognized as a pattern keyword.
    ///     With lookahead checking (IsFollowedByParen), we can be more permissive here.
    ///     Pattern keywords are recognized when:
    ///     - After = ( or , for starting patterns
    ///     - After values/identifiers for pattern continuation (e.g., "TAKE 3 FROM")
    ///     Function calls are filtered out by the lookahead check for '('
    /// </summary>
    private static bool IsValidPatternContext(RawToken? previousToken, string currentWord)
    {
        if (previousToken == null)
        {
            // Start of input - only allow if this keyword can start a pattern
            return CustomSyntaxRegistry.Instance.CanStartPattern(currentWord);
        }

        string prevText = previousToken.Text.ToUpperInvariant();

        // If previous token is a member-access operator or .NET shorthand, this
        // current word is part of a member access (e.g. #str->Length or obj.Property)
        // and must NOT be treated as a pattern keyword.
        if (previousToken.Text == "->" || previousToken.Text == "." || previousToken.Text == "#")
        {
            return false;
        }

        // Check if previous token is a pattern keyword
        if (CustomSyntaxRegistry.Instance.IsPatternKeyword(prevText, out var _))
        {
            // Keywords can't directly follow each other without values between
            // e.g., "TAKE FROM" is invalid, needs "TAKE 3 FROM"
            return false;
        }

        // Context-based rules:
        return prevText switch
        {
            // After assignment: result = TAKE 3 FROM array
            "=" => CustomSyntaxRegistry.Instance.CanStartPattern(currentWord),

            // After opening paren: PRINT(TAKE 3 FROM array)
            // Allow any pattern keyword for both starting and continuation
            "(" => true,

            // After comma: func(x, TAKE 3 FROM array)
            "," => CustomSyntaxRegistry.Instance.CanStartPattern(currentWord),

            // After FUNCTION keyword: never allow (function names)
            "FUNCTION" => false,

            // After RETURN keyword: never allow (usually variable reference, use parens for patterns)
            "RETURN" => false,

            // After type keywords: never allow (parameter/variable names)
            "STRING" or "INT" or "FLOAT" or "DECIMAL" or "FLEX" or "BOOL" or "ARRAY" => false,

            // After any other token (values, identifiers, operators):
            // Allow pattern keywords for continuation (e.g., "3 FROM" in "TAKE 3 FROM array")
            // Function calls are already filtered by IsFollowedByParen check
            _ => CustomSyntaxRegistry.Instance.IsPatternKeyword(currentWord, out var _)
        };
    }

    /// <summary>
    ///     Converts a raw text token into a strongly-typed Token object.
    ///     Uses a four-tier lookup strategy:
    ///     1) Check if it's a pattern keyword from custom syntax (highest priority)
    ///     2) Check static _map for exact matches (operators, keywords, delimiters)
    ///     3) Check dynamic TokenTypes for reflection-discovered types
    ///     4) Fall back to switch statement for special cases and identifiers
    ///     
    ///     Uses context stack to determine if custom keywords are allowed in current context.
    /// </summary>
    private Token ToToken(RawToken rawToken, int index, RawToken? previousToken, RawToken? nextToken)
    {
        Type? targetType;

        // Update context stack based on previous token
        UpdateContextStack(previousToken);

        // Get current context to check if custom keywords are allowed
        var currentContext = _contextStack.Peek();
        bool allowCustomKeywords = currentContext.AllowsCustomKeyword(rawToken.Text);

        // SPECIAL CASE: Check if '!' is followed by '[' to form custom syntax block opener '!['
        // This must be checked BEFORE the _map lookup to handle compound token correctly
        if (rawToken.Text == "!" && nextToken?.Text == "[")
        {
            // Create a merged RawToken for '![' 
            var mergedRawToken = RawToken.Create("![");
            // Push custom syntax block context immediately
            _contextStack.Push(new CustomSyntaxBlockContext());
            LoggerService.Logger.Debug("[Context] Pushed CustomSyntaxBlockContext after '!['");
            return new CustomSyntaxBlockOpen(mergedRawToken);
        }

        // SPECIAL CASE: Skip '[' if it directly follows '!' (already consumed as part of '![')
        if (rawToken.Text == "[" && previousToken?.Text == "!")
        {
            // This '[' was already consumed as part of '![', return a placeholder or skip
            // We'll return null and filter it out later
            return null!; // Mark for removal
        }

        // HIGHEST PRIORITY: Check if this is a pattern keyword from custom syntax
        // This must come before checking _map so pattern keywords override built-in keywords
        // For example, "FROM" in "TAKE 3 FROM array" should be CustomKeywordToken, not FromKeywordToken
        // BUT: Only recognize as pattern keyword if:
        // 1. Current context allows custom keywords
        // 2. Next token is NOT '(' (would be a function call)
        // 3. Next token is NOT '=' (would be a variable assignment)
        // 4. Valid pattern context rules
        if (allowCustomKeywords &&
            CustomSyntaxRegistry.Instance.IsPatternKeyword(rawToken.Text, out var _) &&
            !IsFollowedByParen(nextToken) &&
            !IsFollowedByEquals(nextToken) &&
            IsValidPatternContext(previousToken, rawToken.Text))
        {
            targetType = typeof(CustomKeywordToken);
        }
        // Second priority: Check static map for exact string matches
        // Keywords must be UPPERCASE in the source code to be recognized
        // If lowercase, treat as identifier even if it matches a keyword when uppercased
        else if (_map.TryGetValue(rawToken.Text, out Type? mappedType) && rawToken.OriginalText == rawToken.Text)
        {
            _ = index;
            targetType = mappedType;
        }
        else
        {
            switch (rawToken.Text.Trim())
            {
                case "(":
                    targetType = typeof(ParenthesisOpen);
                    break;
                case ")":
                    targetType = typeof(ParenthesisClosed);
                    break;
                case ",":
                    targetType = typeof(CommaSeparatorToken);
                    break;
                case "{":
                    targetType = typeof(ScopeStartToken);
                    break;
                case "}":
                    targetType = typeof(ScopeEndToken);
                    break;
                default:
                    // Check if it's a template string (starts and ends with backticks)
                    if (IsTemplateString(rawToken.Text))
                    {
                        targetType = typeof(TemplateStringToken);
                    }
                    // Check if it's a string literal (starts and ends with quotes)
                    else if (IsStringLiteral(rawToken.Text))
                    {
                        targetType = typeof(StringLiteralToken);
                    }
                    // Check if it's a numeric literal
                    else if (IsNumericLiteral(rawToken.Text))
                    {
                        // Use DecimalToken if it contains a decimal point, otherwise ValueToken for integers
                        targetType = rawToken.Text.Contains('.') ? typeof(DecimalToken) : typeof(ValueToken);
                    }
                    else
                    {
                        // If no match found, treat as identifier (variable/function name)
                        targetType = typeof(IdentifierToken);
                    }

                    break;
            }
        }

        // Create instance of the appropriate token type
        Token token;

        // Special handling for CustomKeywordToken - need to pass pattern info
        if (targetType == typeof(CustomKeywordToken))
        {
            // Get the pattern keyword info (we checked this above)
            if (CustomSyntaxRegistry.Instance.IsPatternKeyword(rawToken.Text, out var patternInfoList) &&
                patternInfoList != null && patternInfoList.Count > 0)
            {
                // For now, use the first pattern match
                // TODO: Handle ambiguity if a keyword appears in multiple patterns
                var patternInfo = patternInfoList[0];
                token = new CustomKeywordToken(
                    rawToken,
                    patternInfo.PatternId,
                    patternInfo.PositionInPattern,
                    patternInfo.PatternText);
            }
            else
            {
                // This shouldn't happen since we just checked above, but fallback to identifier
                token = new IdentifierToken(rawToken);
            }
        }
        // Special handling for DecimalToken - parse the value at creation time
        else if (targetType == typeof(DecimalToken))
        {
            if (double.TryParse(rawToken.Text, out double decimalValue))
            {
                token = new DecimalToken(rawToken, decimalValue);
            }
            else
            {
                throw new InvalidOperationException($"Failed to parse decimal value: {rawToken.Text}");
            }
        }
        else
        {
            if (Activator.CreateInstance(targetType, rawToken) is not Token createdToken)
            {
                throw new InvalidOperationException($"Failed to create token of type {targetType.Name}");
            }
            token = createdToken;
        }

        LoggerService.Logger.Debug($"\t[{index}] {rawToken.Text} → {targetType.Name}");
        return token;
    }

    /// <summary>
    ///     Checks if a string represents a string literal (enclosed in quotes).
    /// </summary>
    private static bool IsStringLiteral(string text)
    {
        return text.StartsWith('"') && text.EndsWith('"') && text.Length >= 2;
    }

    /// <summary>
    ///     Checks if a string represents a template string (enclosed in backticks).
    /// </summary>
    private static bool IsTemplateString(string text)
    {
        return text.StartsWith('`') && text.EndsWith('`') && text.Length >= 2;
    }

    /// <summary>
    ///     Checks if a string represents a numeric literal (integer or decimal).
    /// </summary>
    private static bool IsNumericLiteral(string text)
    {
        return double.TryParse(text, out _);
    }

    /// <summary>
    ///     Updates the context stack based on the previous token.
    ///     Pushes new contexts for special tokens and pops ephemeral contexts.
    /// </summary>
    private void UpdateContextStack(RawToken? previousToken)
    {
        // Pop ephemeral contexts (they only apply to a single token)
        if (_contextStack.Count > 1 && _contextStack.Peek().IsEphemeral)
        {
            _contextStack.Pop();
            LoggerService.Logger.Debug($"[Context] Popped ephemeral context, now: {_contextStack.Peek().ContextName}");
        }

        if (previousToken == null)
        {
            return;
        }

        string prevText = previousToken.Text;

        // Push contexts based on previous token
        switch (prevText)
        {
            case "->":
            case ".":
            case "#":
                // Member access - next token must be an identifier, not a custom keyword
                _contextStack.Push(new MemberAccessContext());
                LoggerService.Logger.Debug($"[Context] Pushed MemberAccessContext after '{prevText}'");
                break;

            case "=":
                // Assignment - next token starts an expression (no custom keywords)
                // Pop any expression/function call context first, then push new expression context
                if (_contextStack.Count > 1 &&
                    (_contextStack.Peek() is ExpressionContext || _contextStack.Peek() is FunctionCallContext))
                {
                    _contextStack.Pop();
                }
                _contextStack.Push(new ExpressionContext());
                LoggerService.Logger.Debug($"[Context] Pushed ExpressionContext after '='");
                break;

            case "==":
            case "!=":
            case "<":
            case ">":
            case "<=":
            case ">=":
                // Comparison operators - next token is an expression (no custom keywords)
                _contextStack.Push(new ExpressionContext());
                LoggerService.Logger.Debug($"[Context] Pushed ExpressionContext after '{prevText}'");
                break;

            case "(":
                // Function call arguments - expressions, not custom keywords
                _contextStack.Push(new FunctionCallContext());
                LoggerService.Logger.Debug($"[Context] Pushed FunctionCallContext after '('");
                break;

            case ")":
                // Close function call context if we're in one
                if (_contextStack.Count > 1 && _contextStack.Peek() is FunctionCallContext)
                {
                    _contextStack.Pop();
                    LoggerService.Logger.Debug($"[Context] Popped FunctionCallContext after ')'");
                }
                break;

            case "[":
                // Array literal - no custom keywords inside
                _contextStack.Push(new ArrayLiteralContext());
                LoggerService.Logger.Debug($"[Context] Pushed ArrayLiteralContext after '['");
                break;

            case "]":
                // Close custom syntax block context if we're in one, otherwise array literal context
                if (_contextStack.Count > 1 && _contextStack.Peek() is CustomSyntaxBlockContext)
                {
                    _contextStack.Pop();
                    LoggerService.Logger.Debug($"[Context] Popped CustomSyntaxBlockContext after ']'");
                }
                else if (_contextStack.Count > 1 && _contextStack.Peek() is ArrayLiteralContext)
                {
                    _contextStack.Pop();
                    LoggerService.Logger.Debug($"[Context] Popped ArrayLiteralContext after ']'");
                }
                break;

            case "{":
                // Block start - custom keywords allowed for statement patterns
                _contextStack.Push(new BlockContext());
                LoggerService.Logger.Debug("[Context] Pushed BlockContext after '{'");
                break;

            case "}":
                // Close block context if we're in one
                if (_contextStack.Count > 1 && _contextStack.Peek() is BlockContext)
                {
                    _contextStack.Pop();
                    LoggerService.Logger.Debug("[Context] Popped BlockContext after '}'");
                }
                break;

            case "IF":
            case "WHILE":
                // Control flow - next tokens should be condition in expression context
                _contextStack.Push(new ExpressionContext());
                LoggerService.Logger.Debug($"[Context] Pushed ExpressionContext after '{prevText}'");
                break;

            case "RETURN":
                // Return statement - next token is an expression (no custom keywords)
                _contextStack.Push(new ReturnContext());
                LoggerService.Logger.Debug($"[Context] Pushed ReturnContext after 'RETURN'");
                break;

            case "INT":
            case "STRING":
            case "FLOAT":
            case "BOOL":
            case "FLEX":
                // Type annotation - next token is the identifier/parameter name
                _contextStack.Push(new TypeAnnotationContext());
                LoggerService.Logger.Debug($"[Context] Pushed TypeAnnotationContext after '{prevText}'");
                break;

            case ",":
                // Comma in expression context - keep expression context
                // Comma in function call - keep function call context
                // No new context needed
                break;
        }
    }
}