using PowerScript.Common.Logging;
using PowerScript.Core.AST;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes static type declarations: INT name = value, STRING name = value, NUMBER name = value
///     Also supports bit-width specification: INT[8] small = 100, NUMBER[16] medium = 1000
///     These are strongly-typed variables that cannot change type.
/// </summary>
public class StaticTypeVariableProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is IBaseTypeToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        Token typeToken = token;
        string typeName = typeToken.RawToken?.Text ?? "";

        LoggerService.Logger.Debug(
            $"StaticTypeVariableProcessor: Processing {typeName} variable declaration in scope '{context.CurrentScope.ScopeName}'");

        Token currentToken = typeToken.Next!;
        int? bitWidth = null;

        // Check for optional bit-width specification: TYPE[bits]
        if (currentToken is BracketOpen)
        {
            currentToken = currentToken.Next!; // Move past '['

            if (currentToken is not ValueToken bitWidthToken)
            {
                throw new InvalidOperationException(
                    $"Expected bit-width value after '[' in type declaration, found {currentToken.GetType().Name}");
            }

            string bitWidthText = bitWidthToken.RawToken?.Text ?? "";
            if (!int.TryParse(bitWidthText, out int parsedBitWidth))
            {
                throw new InvalidOperationException($"Invalid bit-width value: {bitWidthText}");
            }

            bitWidth = parsedBitWidth;
            LoggerService.Logger.Debug($"[StaticTypeVariableProcessor] Bit-width: {bitWidth}");

            currentToken = bitWidthToken.Next!; // Move past bit width value

            if (currentToken is not BracketClosed)
            {
                throw new InvalidOperationException(
                    $"Expected ']' after bit-width value, found {currentToken.GetType().Name}");
            }

            currentToken = currentToken.Next!; // Move past ']'
        }

        // Next token must be an identifier (variable name)
        if (currentToken is not IdentifierToken)
        {
            throw new InvalidOperationException(
                $"Expected identifier after {typeName}{(bitWidth.HasValue ? $"[{bitWidth}]" : "")}, found {currentToken.GetType().Name}");
        }

        IdentifierToken identifierToken = (IdentifierToken)currentToken;
        string variableName = identifierToken.RawToken?.Text ?? "";

        string typeSpec = bitWidth.HasValue ? $"{typeName}[{bitWidth}]" : typeName;
        LoggerService.Logger.Debug($"[StaticTypeVariableProcessor] Variable name: {variableName}, Type: {typeSpec}");

        // Check for duplicate declarations in current scope
        if (context.CurrentScope.Decarations.ContainsKey(variableName))
        {
            throw new InvalidOperationException(
                $"Variable '{variableName}' is already declared in scope '{context.CurrentScope.ScopeName}'");
        }

        currentToken = identifierToken.Next!;

        // Expect equals sign
        if (currentToken is not EqualsToken)
        {
            throw new InvalidOperationException(
                $"Expected '=' after variable name '{variableName}', found {currentToken.GetType().Name}");
        }

        currentToken = currentToken.Next!;

        // Parse the initial value expression
        Expression initialValue;

        if (currentToken is ChainToken chainToken)
        {
            // Handle CHAIN syntax for arrays
            // Two forms:
            // 1. CHAIN OF [value1, value2, ...]  (array literal)
            // 2. CHAIN size WITH value1, value2, ...  (sized array with initial values)

            currentToken = chainToken.Next!;

            if (currentToken is OfKeywordToken)
            {
                // CHAIN OF [...]  - array literal
                currentToken = currentToken.Next!;

                if (currentToken is not BracketOpen)
                {
                    throw new InvalidOperationException(
                        $"Expected '[' after 'CHAIN OF', found {currentToken.GetType().Name}");
                }

                currentToken = currentToken.Next!; // Move past '['

                List<Expression> elements = new();

                // Parse comma-separated values
                while (currentToken != null && currentToken is not BracketClosed)
                {
                    if (currentToken is ValueToken vt)
                    {
                        elements.Add(new LiteralExpression(vt));
                        currentToken = vt.Next!;
                    }
                    else if (currentToken is IdentifierToken it)
                    {
                        elements.Add(new IdentifierExpression(it));
                        currentToken = it.Next!;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Expected value or identifier in array literal, found {currentToken.GetType().Name}");
                    }

                    // Check for comma or end of array
                    if (currentToken is CommaToken)
                    {
                        currentToken = currentToken.Next!; // Skip comma
                    }
                    else if (currentToken is not BracketClosed)
                    {
                        throw new InvalidOperationException(
                            $"Expected ',' or ']' in array literal, found {currentToken.GetType().Name}");
                    }
                }

                if (currentToken is not BracketClosed)
                {
                    throw new InvalidOperationException("Expected ']' to close array literal");
                }

                currentToken = currentToken.Next!; // Move past ']'

                initialValue = new ArrayLiteralExpression(elements);
            }
            else if (currentToken is ValueToken sizeToken)
            {
                // Could be CHAIN size or CHAIN size WITH values
                Expression sizeExpression = new LiteralExpression(sizeToken);
                currentToken = sizeToken.Next!;

                if (currentToken is WithKeywordToken)
                {
                    // CHAIN size WITH value1, value2, ...
                    currentToken = currentToken.Next!; // Move past WITH

                    List<Expression> initialValues = new();

                    // Parse comma-separated values
                    while (currentToken != null)
                    {
                        if (currentToken is ValueToken vt)
                        {
                            initialValues.Add(new LiteralExpression(vt));
                            currentToken = vt.Next!;
                        }
                        else if (currentToken is IdentifierToken it)
                        {
                            initialValues.Add(new IdentifierExpression(it));
                            currentToken = it.Next!;
                        }
                        else
                        {
                            // End of value list
                            break;
                        }

                        // Check for comma to continue
                        if (currentToken is CommaToken)
                        {
                            currentToken = currentToken.Next!; // Skip comma
                        }
                        else
                        {
                            // No more values
                            break;
                        }
                    }

                    initialValue = new ArrayLiteralExpression(initialValues);
                }
                else
                {
                    // CHAIN size (create empty array of given size)
                    initialValue = new ArrayCreationExpression(sizeExpression);
                }
            }
            else if (currentToken is IdentifierToken sizeIdentifier)
            {
                // CHAIN variableName - create array with size from variable
                Expression sizeExpression = new IdentifierExpression(sizeIdentifier);
                currentToken = sizeIdentifier.Next!;

                if (currentToken is WithKeywordToken)
                {
                    // CHAIN size WITH value1, value2, ...
                    currentToken = currentToken.Next!; // Move past WITH

                    List<Expression> initialValues = new();

                    // Parse comma-separated values
                    while (currentToken != null)
                    {
                        if (currentToken is ValueToken vt)
                        {
                            initialValues.Add(new LiteralExpression(vt));
                            currentToken = vt.Next!;
                        }
                        else if (currentToken is IdentifierToken it)
                        {
                            initialValues.Add(new IdentifierExpression(it));
                            currentToken = it.Next!;
                        }
                        else
                        {
                            // End of value list
                            break;
                        }

                        // Check for comma to continue
                        if (currentToken is CommaToken)
                        {
                            currentToken = currentToken.Next!; // Skip comma
                        }
                        else
                        {
                            // No more values
                            break;
                        }
                    }

                    initialValue = new ArrayLiteralExpression(initialValues);
                }
                else
                {
                    initialValue = new ArrayCreationExpression(sizeExpression);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Expected 'OF', size value, or size identifier after 'CHAIN', found {currentToken.GetType().Name}");
            }
        }
        else
        {
            // For any other token type, use ExpressionParser to handle the complete expression
            // This includes: simple values, identifiers, binary operations, function calls, etc.
            var parser = new ExpressionParser();
            initialValue = parser.Parse(ref currentToken);
        }

        // Create the variable declaration with explicit type and optional bit width
        VariableDeclaration declaration = new VariableDeclaration(typeToken, identifierToken, bitWidth);
        declaration.InitialValue = initialValue;

        // Register the declaration in the scope
        context.CurrentScope.Decarations.Add(variableName, declaration);

        // Create and register the statement
        VariableDeclarationStatement statement = new(declaration, initialValue)
        {
            StartToken = typeToken
        };

        context.CurrentScope.Statements.Add(statement);

        LoggerService.Logger.Success(
            $"[StaticTypeVariableProcessor] Registered {typeSpec} variable '{variableName}' in scope '{context.CurrentScope.ScopeName}'");

        if (currentToken == null)
        {
            throw new InvalidOperationException($"Unexpected end of tokens after variable declaration '{variableName}'");
        }

        return TokenProcessingResult.Continue(currentToken);
    }

    /// <summary>
    ///     Parses function call arguments from opening parenthesis to closing parenthesis.
    ///     Returns the list of argument expressions and the token after the closing parenthesis.
    /// </summary>
    private static (List<Expression> Arguments, Token? NextToken) ParseFunctionArguments(Token openParenToken)
    {
        List<Expression> arguments = new();
        Token? currentToken = openParenToken.Next; // Start after '('

        // Handle empty argument list
        if (currentToken is ParenthesisClosed)
        {
            return (arguments, currentToken.Next);
        }

        // Parse comma-separated arguments
        while (currentToken != null && currentToken is not ParenthesisClosed)
        {
            // Collect tokens for this argument until we hit a comma or closing paren
            List<Token> argumentTokens = new();
            int parenDepth = 0;

            while (currentToken != null)
            {
                // Track parenthesis depth for nested expressions
                if (currentToken is ParenthesisOpen)
                {
                    parenDepth++;
                    argumentTokens.Add(currentToken);
                    currentToken = currentToken.Next;
                    continue;
                }

                if (currentToken is ParenthesisClosed)
                {
                    if (parenDepth == 0)
                    {
                        // End of argument list
                        break;
                    }
                    parenDepth--;
                    argumentTokens.Add(currentToken);
                    currentToken = currentToken.Next;
                    continue;
                }

                // Comma at depth 0 means end of this argument
                if (currentToken is CommaToken && parenDepth == 0)
                {
                    currentToken = currentToken.Next; // Skip comma
                    break;
                }

                argumentTokens.Add(currentToken);
                currentToken = currentToken.Next;
            }

            // Build expression from collected tokens
            if (argumentTokens.Count > 0)
            {
                Expression argExpr = BuildSimpleExpression(argumentTokens);
                arguments.Add(argExpr);
            }
        }

        // CurrentToken should now be at ParenthesisClosed
        Token? nextToken = currentToken is ParenthesisClosed ? currentToken.Next : currentToken;
        return (arguments, nextToken);
    }

    /// <summary>
    ///     Builds a simple expression from a list of tokens.
    ///     Supports literals, identifiers, and binary operations.
    /// </summary>
    private static Expression BuildSimpleExpression(List<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Cannot build expression from empty token list");
        }

        // Ensure tokens are properly linked
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            if (tokens[i].Next != tokens[i + 1])
            {
                tokens[i].Next = tokens[i + 1];
            }
        }

        // Use the ExpressionParser to handle all expression parsing including nested function calls
        var parser = new ExpressionParser();
        Token currentToken = tokens[0];
        var expression = parser.Parse(ref currentToken);
        return expression;
    }

    private static FunctionCallExpression ParseFunctionCallExpression(IdentifierToken functionNameToken)
    {
        Token openParen = functionNameToken.Next;
        var (arguments, _) = ParseFunctionArguments(openParen);

        FunctionCallExpression funcCall = new FunctionCallExpression
        {
            FunctionName = functionNameToken
        };
        funcCall.Arguments.AddRange(arguments);

        return funcCall;
    }
}
