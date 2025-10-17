using PowerScript.Common.Logging;
using PowerScript.Core.AST;
using PowerScript.Core.DotNet;
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
        { "PREC", typeof(PrecToken) }, // Precision/float type keyword
        { "CHAR", typeof(CharToken) }, // Character type keyword
        { "STRING", typeof(StringToken) }, // String type keyword (CHAR CHAIN)
        { "NUMBER", typeof(NumberToken) }, // Number type keyword
        { "CHAIN", typeof(ChainToken) }, // Collection/array type modifier
        { "{", typeof(ScopeStartToken) }, // Scope/block start (also object literal start)
        { "}", typeof(ScopeEndToken) }, // Scope/block end (also object literal end)
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

        // Step 1: Convert raw text tokens into strongly-typed token objects
        LoggerService.Logger.Debug($"Raw tokens count: {prompt.RawTokens.Length}");
        Token[] tokens = [.. new RawTokenCollection(prompt.RawTokens).Select((token, index) => ToToken(token, index))];
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
    ///     Converts a raw text token into a strongly-typed Token object.
    ///     Uses a three-tier lookup strategy:
    ///     1) Check static _map for exact matches (operators, keywords, delimiters)
    ///     2) Check dynamic TokenTypes for reflection-discovered types
    ///     3) Fall back to switch statement for special cases and identifiers
    /// </summary>
    private static Token ToToken(RawToken rawToken, int index)
    {
        Type? targetType;

        // First priority: Check static map for exact string matches
        // Keywords must be UPPERCASE in the source code to be recognized
        // If lowercase, treat as identifier even if it matches a keyword when uppercased
        if (_map.TryGetValue(rawToken.Text, out Type? mappedType) && rawToken.OriginalText == rawToken.Text)
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

        // Special handling for DecimalToken - parse the value at creation time
        if (targetType == typeof(DecimalToken))
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
}