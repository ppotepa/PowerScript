using PowerScript.Core.AST;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Exceptions;
using PowerScript.Core.Syntax;
using PowerScript.Core.Syntax.Tokens;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Lexer;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
/// Processes pattern-based custom syntax statements.
/// Examples: FILTER $array WHERE $condition => ARRAY_FILTER($array, $condition)
///           TAKE $count FROM $array => ARRAY_TAKE($array, $count)
/// </summary>
public class PatternSyntaxProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        // Skip pattern processing if this is part of a function definition
        // (previous token is FUNCTION keyword)
        if (token.Prev != null && token.Prev is FunctionToken)
        {
            return false;
        }

        // Skip pattern processing if this looks like a function call
        // (next token is opening parenthesis)
        if (token.Next != null && token.Next is ParenthesisOpen)
        {
            return false;
        }

        // Skip pattern processing if this is a variable assignment
        // (next token is an equals sign)
        if (token.Next != null && token.Next is EqualsToken)
        {
            return false;
        }



        // Check if this token is a CustomKeywordToken at position 0 (start of pattern)
        if (token is CustomKeywordToken customKeywordToken)
        {
            // CustomKeywordToken already knows it's a pattern keyword
            // Only process if it's at position 0 (start of pattern)
            return customKeywordToken.PositionInPattern == 0;
        }

        // Fallback: Check if this is an identifier that matches a pattern start
        // (This handles cases where pattern keywords haven't been registered yet)
        if (token is not IdentifierToken identToken)
            return false;

        string firstWord = identToken.Value.ToUpperInvariant();

        // Skip single-letter identifiers - these are likely variables, not pattern keywords
        if (firstWord.Length == 1)
        {
            return false;
        }

        // IMPORTANT: Only match patterns if the identifier was written in UPPERCASE in source code.
        // This prevents lowercase variable names like "sum" from triggering patterns like "SUM OF".
        // Pattern keywords should be written in uppercase (e.g., "SUM OF numbers" not "sum of numbers")
        string originalText = identToken.RawToken?.OriginalText ?? firstWord;
        if (originalText != originalText.ToUpperInvariant())
        {
            // Identifier is not all uppercase in source, skip pattern matching
            return false;
        }

        // Get all pattern transformations from registry
        var patterns = CustomSyntaxRegistry.Instance.GetPatternTransformations();

        foreach (var pattern in patterns)
        {
            // Extract first word from pattern (e.g., "FILTER" from "FILTER $array WHERE $condition")
            string patternFirstWord = ExtractFirstWord(pattern.Pattern);
            if (patternFirstWord == firstWord)
            {
                return true;
            }
        }

        return false;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        string firstWord;
        string patternId;

        // Extract the first word and pattern ID
        if (token is CustomKeywordToken customKeywordToken)
        {
            firstWord = customKeywordToken.RawToken?.Text?.ToUpperInvariant() ?? "";
            patternId = customKeywordToken.PatternId;
        }
        else if (token is IdentifierToken identToken)
        {
            firstWord = identToken.Value.ToUpperInvariant();
            patternId = ""; // Will find pattern by matching first word
        }
        else
        {
            throw new InvalidOperationException($"Pattern syntax processor received unexpected token type: {token.GetType().Name}");
        }

        // Try to match against patterns
        var patterns = CustomSyntaxRegistry.Instance.GetPatternTransformations();

        foreach (var pattern in patterns)
        {
            string patternFirstWord = ExtractFirstWord(pattern.Pattern);
            if (patternFirstWord != firstWord)
                continue;

            // Try to match this pattern
            var match = TryMatchPattern(token, pattern);
            if (match != null)
            {
                // Build the transformed function call
                var functionCall = BuildFunctionCall(pattern, match);

                // Create an expression statement
                var statement = new ExpressionStatement
                {
                    StartToken = token,
                    Expression = functionCall
                };

                context.CurrentScope.Statements.Add(statement);

                return new TokenProcessingResult
                {
                    NextToken = match.NextToken,
                    ShouldValidateExpectations = false
                };
            }
        }

        // No pattern matched - shouldn't happen if CanProcess returned true
        throw new InvalidOperationException($"Pattern syntax processor failed to match pattern starting with '{firstWord}'");
    }

    /// <summary>
    /// Attempts to match a token stream against a pattern.
    /// Returns variable bindings if successful, null otherwise.
    /// </summary>
    private PatternMatch? TryMatchPattern(Token startToken, SyntaxTransformation pattern)
    {
        // Console.WriteLine($"[TryMatchPattern] Starting match for pattern: {pattern.Pattern}");

        // Parse the pattern template
        var patternParts = ParsePattern(pattern.Pattern);

        // Console.WriteLine($"[TryMatchPattern] Pattern has {patternParts.Count} parts");
        foreach (var part in patternParts)
        {
            // Console.WriteLine($"[TryMatchPattern]   Part: '{part.Text}' IsVariable={part.IsVariable} IsOptional={part.IsOptional}");
        }

        Token? currentToken = startToken;
        var bindings = new Dictionary<string, List<Token>>();

        for (int i = 0; i < patternParts.Count; i++)
        {
            var part = patternParts[i];
            // Console.WriteLine($"[TryMatchPattern] Processing part {i}: '{part.Text}' (Optional: {part.IsOptional}), current token: {currentToken?.GetType().Name}");

            if (part.IsVariable)
            {
                // This is a variable like $array or $condition
                // Capture tokens until we hit the next keyword or end
                var capturedTokens = new List<Token>();

                // Determine where this variable ends
                Token? endMarker = null;
                if (i + 1 < patternParts.Count)
                {
                    // Find the next non-optional keyword to use as end marker
                    Token? nextKeywordToken = null;
                    for (int j = i + 1; j < patternParts.Count; j++)
                    {
                        var nextPart = patternParts[j];
                        if (!nextPart.IsVariable)
                        {
                            nextKeywordToken = FindNextKeyword(currentToken, nextPart.Text);
                            if (nextKeywordToken != null)
                            {
                                endMarker = nextKeywordToken;
                                // Console.WriteLine($"[TryMatchPattern] Variable '{part.Text}' ends at keyword '{nextPart.Text}', endMarker: {endMarker?.GetType().Name}");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    // Console.WriteLine($"[TryMatchPattern] Variable '{part.Text}' is last part - will capture remaining tokens");
                }

                // Capture tokens for this variable
                while (currentToken != null && currentToken != endMarker)
                {
                    // If there's no endMarker (last variable), stop at statement boundaries
                    if (endMarker == null && ShouldStopCapture(currentToken, capturedTokens.Count))
                    {
                        // Console.WriteLine($"[TryMatchPattern]   Stopping capture at statement boundary: {currentToken.GetType().Name}");
                        break;
                    }

                    string tokenValue = currentToken switch
                    {
                        IdentifierToken it => it.Value,
                        ValueToken vt => vt.ToString(),
                        _ => currentToken?.GetType().Name ?? "Unknown"
                    } ?? "Unknown";
                    // Console.WriteLine($"[TryMatchPattern]   Capturing token: {currentToken?.GetType().Name} = {tokenValue}");
                    if (currentToken != null)
                    {
                        capturedTokens.Add(currentToken);
                        currentToken = currentToken.Next;
                    }
                }

                if (capturedTokens.Count == 0)
                {
                    if (part.IsOptional)
                    {
                        // Optional variable with no tokens - skip it
                        // Console.WriteLine($"[TryMatchPattern] Optional variable '{part.Text}' captured nothing - skipping");
                        continue;
                    }
                    else
                    {
                        // Required variable didn't capture anything - match failed
                        // Console.WriteLine($"[TryMatchPattern] FAIL: Required variable '{part.Text}' captured nothing");
                        return null;
                    }
                }

                // Validate type constraints if present
                if (pattern.TypeConstraints.ContainsKey(part.Text))
                {
                    string expectedType = pattern.TypeConstraints[part.Text];
                    if (!ValidateTypeConstraint(capturedTokens, expectedType, part.Text))
                    {
                        if (part.IsOptional)
                        {
                            // Optional variable with wrong type - skip it
                            // Console.WriteLine($"[TryMatchPattern] Optional variable '{part.Text}' failed type constraint '{expectedType}' - skipping");
                            continue;
                        }
                        else
                        {
                            // Console.WriteLine($"[TryMatchPattern] FAIL: Required variable '{part.Text}' does not match type constraint '{expectedType}'");
                            return null;
                        }
                    }
                    // Console.WriteLine($"[TryMatchPattern] Variable '{part.Text}' passed type constraint '{expectedType}'");
                }

                // Console.WriteLine($"[TryMatchPattern] Variable '{part.Text}' captured {capturedTokens.Count} tokens");
                bindings[part.Text] = capturedTokens;
            }
            else
            {
                // This is a keyword - must match exactly
                // Check both IdentifierToken and the raw token text
                bool matches = false;
                if (currentToken is IdentifierToken keywordToken)
                {
                    matches = keywordToken.Value.ToUpperInvariant() == part.Text.ToUpperInvariant();
                }
                else if (currentToken?.RawToken?.Text?.ToUpperInvariant() == part.Text.ToUpperInvariant())
                {
                    matches = true;
                }

                if (!matches)
                {
                    if (part.IsOptional)
                    {
                        // Optional keyword doesn't match - skip it
                        // Console.WriteLine($"[TryMatchPattern] Optional keyword '{part.Text}' not found - skipping");
                        continue;
                    }
                    else
                    {
                        // Required keyword doesn't match - pattern failed
                        // Console.WriteLine($"[TryMatchPattern] FAIL: Expected required keyword '{part.Text}' but got {currentToken?.GetType().Name}");
                        return null;
                    }
                }

                // Console.WriteLine($"[TryMatchPattern] Matched keyword '{part.Text}'");
                currentToken = currentToken?.Next;
            }
        }

        return new PatternMatch
        {
            Bindings = bindings,
            NextToken = currentToken!
        };
    }

    /// <summary>
    /// Finds the next occurrence of a keyword in the token stream.
    /// </summary>
    private Token? FindNextKeyword(Token? startToken, string keyword)
    {
        Token? current = startToken;
        string keywordUpper = keyword.ToUpperInvariant();

        while (current != null)
        {
            // Check if this is an identifier with the keyword text
            if (current is IdentifierToken identToken &&
                identToken.Value.ToUpperInvariant() == keywordUpper)
            {
                return current;
            }

            // Also check the raw token text for keyword tokens
            if (current.RawToken?.Text?.ToUpperInvariant() == keywordUpper)
            {
                return current;
            }

            current = current.Next;
        }
        return null;
    }

    /// <summary>
    /// Determines if variable capture should stop at the current token.
    /// Used when capturing the last variable in a pattern (no explicit end marker).
    /// </summary>
    private bool ShouldStopCapture(Token currentToken, int capturedCount)
    {
        // If we've already captured at least one token, stop at statement boundaries
        if (capturedCount > 0)
        {
            // Stop at scope boundaries
            if (currentToken is ScopeEndToken)
                return true;

            // Stop at statement keywords (these start new statements)
            if (currentToken is IdentifierToken identToken)
            {
                string upper = identToken.Value.ToUpperInvariant();
                if (upper == "PRINT" || upper == "IF" || upper == "CYCLE" ||
                    upper == "RETURN" || upper == "FLEX" || upper == "FUNCTION" ||
                    upper == "WHILE" || upper == "FOR")
                {
                    return true;
                }
            }

            // Stop at pattern keywords (might be start of another pattern)
            if (currentToken is CustomKeywordToken)
                return true;
        }

        // For the first token, capture at least one value/identifier
        // But stop after capturing a complete simple expression
        if (capturedCount == 1)
        {
            // If next token is an operator, continue (it's part of the expression)
            if (currentToken is PlusToken || currentToken is MinusToken ||
                currentToken is MultiplyToken || currentToken is DivideToken ||
                currentToken is ModuloToken)
            {
                return false; // Continue capturing the expression
            }
        }

        return false; // Continue capturing
    }

    /// <summary>
    /// Builds a function call expression from the matched pattern and transformation.
    /// </summary>
    private FunctionCallExpression BuildFunctionCall(SyntaxTransformation pattern, PatternMatch match)
    {
        // Parse the transformation template (e.g., "ARRAY_FILTER($array, $condition)")
        var transformParts = ParseTransformation(pattern.Transformation);

        // Extract function name (everything before the first '(')
        string functionName = transformParts.FunctionName;

        // Build arguments by replacing variables with their bound tokens
        var arguments = new List<Expression>();

        foreach (var argTemplate in transformParts.Arguments)
        {
            if (argTemplate.StartsWith("$"))
            {
                // This is a variable reference
                if (match.Bindings.TryGetValue(argTemplate, out var tokens))
                {
                    // Parse the captured tokens as an expression
                    var expr = ParseTokensAsExpression(tokens);
                    arguments.Add(expr);
                }
                else
                {
                    // Variable not found in bindings - could be optional
                    // Check if this variable was optional in the pattern
                    bool isOptionalVariable = IsVariableOptionalInPattern(pattern, argTemplate);
                    if (isOptionalVariable)
                    {
                        // For optional variables, provide a default null value
                        var nullRawToken = RawToken.Create("null");
                        var nullToken = new ValueToken(nullRawToken);
                        arguments.Add(new LiteralExpression(nullToken));
                        // Console.WriteLine($"[BuildFunctionCall] Optional variable '{argTemplate}' not bound - using null default");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Required variable '{argTemplate}' not found in pattern bindings");
                    }
                }
            }
            else
            {
                // This is a literal value - create a ValueToken and wrap in LiteralExpression
                var valueRawToken = RawToken.Create(argTemplate);
                var valueToken = new ValueToken(valueRawToken);
                arguments.Add(new LiteralExpression(valueToken));
            }
        }

        // Create a synthetic identifier token for the function name
        var functionNameRawToken = RawToken.Create(functionName);
        var functionNameToken = new IdentifierToken(functionNameRawToken);

        var functionCall = new FunctionCallExpression
        {
            FunctionName = functionNameToken
        };
        functionCall.Arguments.AddRange(arguments);

        return functionCall;
    }

    /// <summary>
    /// Parses captured tokens into an expression using ExpressionParser.
    /// </summary>
    private Expression ParseTokensAsExpression(List<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Cannot parse empty token list as expression");
        }

        // Link tokens together
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            tokens[i].Next = tokens[i + 1];
        }

        // Use ExpressionParser to parse the tokens
        var parser = new ExpressionParser();
        Token currentToken = tokens[0];
        return parser.Parse(ref currentToken);
    }

    /// <summary>
    /// Parses a pattern string into parts (keywords and variables).
    /// Example: "FILTER $array WHERE? $condition" => [FILTER, $array, WHERE?, $condition]
    /// Supports optional elements with ? suffix: "SORT $array ASCENDING?"
    /// </summary>
    private List<PatternPart> ParsePattern(string pattern)
    {
        var parts = new List<PatternPart>();
        var words = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            bool isOptional = word.EndsWith("?");
            string cleanWord = isOptional ? word.TrimEnd('?') : word;

            if (cleanWord.StartsWith("$"))
            {
                // Variable with possible type constraint: $variable:TYPE
                // Extract just the variable name for matching
                var colonIndex = cleanWord.IndexOf(':');
                var variableName = colonIndex > 0 ? cleanWord.Substring(0, colonIndex) : cleanWord;

                parts.Add(new PatternPart
                {
                    Text = variableName, // Store just $variable, not $variable:TYPE
                    IsVariable = true,
                    IsOptional = isOptional
                });
            }
            else
            {
                parts.Add(new PatternPart
                {
                    Text = cleanWord,
                    IsVariable = false,
                    IsOptional = isOptional
                });
            }
        }

        return parts;
    }

    /// <summary>
    /// Parses a transformation string to extract function name and arguments.
    /// Example: "ARRAY_FILTER($array, $condition)" => { FunctionName: "ARRAY_FILTER", Arguments: ["$array", "$condition"] }
    /// </summary>
    private TransformationParts ParseTransformation(string transformation)
    {
        // Find the function name (everything before '(')
        int openParenIndex = transformation.IndexOf('(');
        if (openParenIndex < 0)
        {
            throw new InvalidOperationException($"Invalid transformation format: {transformation}");
        }

        string functionName = transformation.Substring(0, openParenIndex).Trim();

        // Extract arguments (everything between '(' and ')')
        int closeParenIndex = transformation.LastIndexOf(')');
        if (closeParenIndex < 0)
        {
            throw new InvalidOperationException($"Invalid transformation format: {transformation}");
        }

        string argString = transformation.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1).Trim();

        var arguments = new List<string>();
        if (!string.IsNullOrEmpty(argString))
        {
            // Split by comma, accounting for potential nested parentheses
            arguments = SplitArguments(argString);
        }

        return new TransformationParts
        {
            FunctionName = functionName,
            Arguments = arguments
        };
    }

    /// <summary>
    /// Splits arguments by comma, respecting nested parentheses.
    /// </summary>
    private List<string> SplitArguments(string argString)
    {
        var arguments = new List<string>();
        var currentArg = new System.Text.StringBuilder();
        int parenDepth = 0;

        foreach (char c in argString)
        {
            if (c == '(')
            {
                parenDepth++;
                currentArg.Append(c);
            }
            else if (c == ')')
            {
                parenDepth--;
                currentArg.Append(c);
            }
            else if (c == ',' && parenDepth == 0)
            {
                // This comma separates arguments
                arguments.Add(currentArg.ToString().Trim());
                currentArg.Clear();
            }
            else
            {
                currentArg.Append(c);
            }
        }

        // Add the last argument
        if (currentArg.Length > 0)
        {
            arguments.Add(currentArg.ToString().Trim());
        }

        return arguments;
    }

    /// <summary>
    /// Extracts the first word from a pattern.
    /// </summary>
    private string ExtractFirstWord(string pattern)
    {
        int spaceIndex = pattern.IndexOf(' ');
        if (spaceIndex < 0)
        {
            return pattern.ToUpperInvariant();
        }
        return pattern.Substring(0, spaceIndex).ToUpperInvariant();
    }

    /// <summary>
    /// Checks if a variable was marked as optional in the original pattern.
    /// </summary>
    private bool IsVariableOptionalInPattern(SyntaxTransformation pattern, string variableName)
    {
        // Parse the original pattern to check if this variable was optional
        var patternParts = ParsePattern(pattern.Pattern);

        foreach (var part in patternParts)
        {
            if (part.IsVariable && part.Text == variableName)
            {
                return part.IsOptional;
            }
        }

        return false; // Variable not found or not optional
    }

    /// <summary>
    /// Validates that captured tokens match the expected type constraint.
    /// </summary>
    private bool ValidateTypeConstraint(List<Token> capturedTokens, string expectedType, string variableName)
    {
        if (capturedTokens.Count == 0)
        {
            // Console.WriteLine($"[ValidateTypeConstraint] No tokens captured for variable '{variableName}'");
            return false;
        }

        // For single token validation, check the first token
        var firstToken = capturedTokens[0];
        string actualType = InferTokenType(firstToken);

        // Console.WriteLine($"[ValidateTypeConstraint] Variable '{variableName}': expected '{expectedType}', got '{actualType}'");

        // Check if types match (case-insensitive)
        return actualType.ToUpperInvariant() == expectedType.ToUpperInvariant();
    }

    /// <summary>
    /// Infers the type of a token for type constraint validation.
    /// </summary>
    private string InferTokenType(Token token)
    {
        return token switch
        {
            ValueToken vt when int.TryParse(vt.RawToken?.Text ?? "", out _) => "INT",
            ValueToken vt when double.TryParse(vt.RawToken?.Text ?? "", out _) => "FLOAT",
            DecimalToken => "FLOAT",
            StringLiteralToken => "STRING",
            ValueToken vt when (vt.RawToken?.Text ?? "").StartsWith("\"") && (vt.RawToken?.Text ?? "").EndsWith("\"") => "STRING",
            ValueToken vt when (vt.RawToken?.Text ?? "").StartsWith("[") && (vt.RawToken?.Text ?? "").EndsWith("]") => "ARRAY",
            ValueToken vt when (vt.RawToken?.Text ?? "").ToUpperInvariant() == "TRUE" || (vt.RawToken?.Text ?? "").ToUpperInvariant() == "FALSE" => "BOOL",
            IdentifierToken => "IDENTIFIER", // Variables, function names, etc.
            _ => "UNKNOWN"
        };
    }

    // Helper classes
    private class PatternPart
    {
        public string Text { get; set; } = "";
        public bool IsVariable { get; set; }
        public bool IsOptional { get; set; }
    }

    private class PatternMatch
    {
        public Dictionary<string, List<Token>> Bindings { get; set; } = new();
        public Token NextToken { get; set; } = null!;
    }

    private class TransformationParts
    {
        public string FunctionName { get; set; } = "";
        public List<string> Arguments { get; set; } = new();
    }
}
