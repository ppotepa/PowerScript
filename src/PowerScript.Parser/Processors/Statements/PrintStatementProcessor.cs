using PowerScript.Common.Logging;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;
using PowerScript.Core.Syntax.Tokens.Operators;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes PRINT keyword tokens.
///     Responsible for:
///     - Parsing the expression to print (string literal, function call, etc.)
///     - Creating PrintStatement objects
///     - Registering print statements in the current scope
/// </summary>
public class PrintStatementProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is PrintKeywordToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Debug(
            $"PrintStatementProcessor: Processing PRINT token '{token.RawToken?.Text}' in scope '{context.CurrentScope.ScopeName}'");

        PrintKeywordToken? printToken = token as PrintKeywordToken;
        Token? nextToken = printToken!.Next;

        // Parse the expression to print
        Expression? expression = null;

        if (nextToken is TemplateStringToken templateToken)
        {
            // Template string with variable interpolation
            expression = new TemplateStringExpression(templateToken);
            nextToken = templateToken.Next;
        }
        else if (nextToken is StringLiteralToken stringToken)
        {
            // Simple string literal
            expression = new StringLiteralExpression(stringToken);
            nextToken = stringToken.Next;
        }
        else if (nextToken is IdentifierToken identifierToken)
        {
            // Check if it's a function call
            if (identifierToken.Next is ParenthesisOpen)
            {
                // Function call expression: PRINT function(args)
                Token openParen = identifierToken.Next;

                // Parse arguments
                var (arguments, tokenAfterArgs) = ParseFunctionArguments(openParen);

                FunctionCallExpression funcCall = new FunctionCallExpression
                {
                    FunctionName = identifierToken
                };
                funcCall.Arguments.AddRange(arguments);
                expression = funcCall;

                nextToken = tokenAfterArgs;
            }
            // Check if it's an array index access (supports chaining: arr[0][1])
            else if (identifierToken.Next is BracketOpen)
            {
                Expression currentExpr = new IdentifierExpression(identifierToken);
                Token currentToken = identifierToken.Next; // Move to '['

                // Support chained indexing
                while (currentToken is BracketOpen)
                {
                    currentToken = currentToken.Next; // Move past '['
                    Expression indexExpr = ParseSimpleExpression(ref currentToken);

                    // Expect closing bracket
                    if (currentToken is not BracketClosed)
                    {
                        throw new InvalidOperationException(
                            $"Expected ']' after array index but found {currentToken?.GetType().Name}");
                    }

                    currentExpr = new IndexExpression
                    {
                        ArrayExpression = currentExpr,
                        Index = indexExpr
                    };
                    currentToken = currentToken.Next; // Move past ']'
                }

                expression = currentExpr;
                nextToken = currentToken;
            }
            else
            {
                // Simple variable or function call
                expression = new IdentifierExpression(identifierToken);
                nextToken = identifierToken.Next;
            }
        }
        else if (nextToken is ValueToken valueToken)
        {
            // Numeric literal
            expression = new LiteralExpression(valueToken);
            nextToken = valueToken.Next;
        }
        else
        {
            throw new NotImplementedException(
                $"PRINT does not yet support expression type: {nextToken?.GetType().Name}");
        }

        // Create and register the print statement
        PrintStatement printStatement = new(expression)
        {
            StartToken = printToken
        };

        context.CurrentScope.Statements.Add(printStatement);

        LoggerService.Logger.Debug($"Registered PRINT statement in scope '{context.CurrentScope.ScopeName}'");

        // Don't validate expectations since we already handled the token sequence
        // If nextToken is null, we've reached the end of the token stream
        return new TokenProcessingResult
        {
            NextToken = nextToken,  // Changed: Don't fallback to printToken, return null if at end
            ShouldValidateExpectations = false
        };
    }

    /// <summary>
    ///     Parses a simple expression (literal, identifier, or value).
    /// </summary>
    private static Expression ParseSimpleExpression(ref Token currentToken)
    {
        if (currentToken is ValueToken valueToken)
        {
            LiteralExpression expr = new(valueToken);
            currentToken = currentToken.Next;
            return expr;
        }

        if (currentToken is IdentifierToken identifierToken)
        {
            IdentifierExpression expr = new(identifierToken);
            currentToken = currentToken.Next;
            return expr;
        }

        throw new InvalidOperationException($"Expected expression but found {currentToken?.GetType().Name}");
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

        if (tokens.Count == 1)
        {
            Token token = tokens[0];
            return token switch
            {
                ValueToken valueToken => new LiteralExpression(valueToken),
                IdentifierToken identifierToken => new IdentifierExpression(identifierToken),
                _ => throw new InvalidOperationException($"Unexpected single token type: {token.GetType().Name}")
            };
        }

        // For multi-token expressions, create a binary expression
        // Simple left-to-right evaluation (no operator precedence for now)
        Expression left = tokens[0] switch
        {
            ValueToken vt => new LiteralExpression(vt),
            IdentifierToken it => new IdentifierExpression(it),
            _ => throw new InvalidOperationException($"Unexpected token type: {tokens[0].GetType().Name}")
        };

        for (int i = 1; i < tokens.Count; i += 2)
        {
            if (i + 1 >= tokens.Count)
            {
                throw new InvalidOperationException("Expected value after operator");
            }

            Token operatorToken = tokens[i];
            Token rightToken = tokens[i + 1];

            Expression right = rightToken switch
            {
                ValueToken vt => new LiteralExpression(vt),
                IdentifierToken it => new IdentifierExpression(it),
                _ => throw new InvalidOperationException($"Unexpected token type: {rightToken.GetType().Name}")
            };

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }
}
