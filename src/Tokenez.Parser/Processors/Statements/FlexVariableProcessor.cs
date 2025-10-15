using Tokenez.Common.Logging;
using Tokenez.Core.AST;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.AST.Statements;
using Tokenez.Core.Exceptions;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Delimiters;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Keywords.Types;
using Tokenez.Core.Syntax.Tokens.Operators;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;
using Tokenez.Parser.Processors.Base;

namespace Tokenez.Parser.Processors.Statements;

/// <summary>
///     Processes FLEX keyword for dynamic variable declarations.
///     Syntax: FLEX variableName = expression
/// </summary>
public class FlexVariableProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is FlexKeywordToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Info(
            $"[FlexVariableProcessor] Processing FLEX variable declaration in scope '{context.CurrentScope.ScopeName}'");

        FlexKeywordToken? flexToken = token as FlexKeywordToken;
        Token? currentToken = flexToken!.Next;

        // Expect identifier (variable name)
        if (currentToken is not IdentifierToken identifierToken)
        {
            throw new UnexpectedTokenException(currentToken!, typeof(IdentifierToken));
        }

        string variableName = identifierToken.RawToken!.Text;
        LoggerService.Logger.Debug($"[FlexVariableProcessor] Variable name: {variableName}");

        currentToken = currentToken.Next;

        // Check if this is an array element assignment (FLEX arr[0] = value or FLEX arr[0][1] = value)
        if (currentToken is BracketOpen)
        {
            // Build IndexExpression (supports chaining for 2D arrays)
            Expression currentExpr = new IdentifierExpression(identifierToken);

            while (currentToken is BracketOpen)
            {
                currentToken = currentToken.Next; // Move past '['
                Expression indexExpr = ParseExpression(ref currentToken);

                // Expect closing bracket
                if (currentToken is not BracketClosed)
                {
                    throw new UnexpectedTokenException(currentToken!, typeof(BracketClosed));
                }

                currentToken = currentToken.Next; // Skip ']'

                currentExpr = new IndexExpression
                {
                    ArrayExpression = currentExpr,
                    Index = indexExpr
                };
            }

            // Expect assignment operator
            if (currentToken is not EqualsToken)
            {
                throw new UnexpectedTokenException(currentToken!, typeof(EqualsToken));
            }

            currentToken = currentToken.Next;

            // Parse the value expression
            Expression valueExpr = ParseExpression(ref currentToken);

            // Create an ArrayAssignmentStatement
            ArrayAssignmentStatement arrayAssignStmt = new((IndexExpression)currentExpr, valueExpr)
            {
                StartToken = flexToken
            };

            context.CurrentScope.Statements.Add(arrayAssignStmt);

            LoggerService.Logger.Success(
                $"[FlexVariableProcessor] Registered array assignment '{variableName}[...] = ...' in scope '{context.CurrentScope.ScopeName}'");

            return new TokenProcessingResult
            {
                NextToken = currentToken!,
                ShouldValidateExpectations = false
            };
        }

        // Expect assignment operator
        if (currentToken is not EqualsToken)
        {
            throw new UnexpectedTokenException(currentToken!, typeof(EqualsToken));
        }

        currentToken = currentToken.Next;

        // Parse the initialization expression
        Expression initExpression = ParseExpression(ref currentToken);

        // Create variable declaration
        VariableDeclaration variableDecl = new(identifierToken); // No type token for FLEX variables

        // Create variable declaration statement
        VariableDeclarationStatement statement = new(variableDecl, initExpression, true)
        {
            StartToken = flexToken
        };

        // Add to current scope
        context.CurrentScope.Statements.Add(statement);
        context.CurrentScope.AddDynamicVariable(variableName);

        LoggerService.Logger.Success(
            $"[FlexVariableProcessor] Registered FLEX variable '{variableName}' in scope '{context.CurrentScope.ScopeName}'");

        return new TokenProcessingResult
        {
            NextToken = currentToken!,
            ShouldValidateExpectations = false
        };
    }


    /// <summary>
    ///     Parses an expression with support for binary operations.
    ///     Handles operator precedence: * / before + -
    /// </summary>
    private Expression ParseExpression(ref Token? token)
    {
        // Parse additive expression (lowest precedence)
        return ParseAdditiveExpression(ref token);
    }

    /// <summary>
    ///     Parses addition and subtraction (left-to-right)
    /// </summary>
    private Expression ParseAdditiveExpression(ref Token? token)
    {
        Expression left = ParseMultiplicativeExpression(ref token);

        while (token is PlusToken or MinusToken)
        {
            Token operatorToken = token;
            token = token.Next;

            Expression right = ParseMultiplicativeExpression(ref token);

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    /// <summary>
    ///     Parses multiplication and division (left-to-right)
    /// </summary>
    private Expression ParseMultiplicativeExpression(ref Token? token)
    {
        Expression left = ParsePrimaryExpression(ref token);

        while (token is MultiplyToken or DivideToken)
        {
            Token operatorToken = token;
            token = token.Next;

            Expression right = ParsePrimaryExpression(ref token);

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    /// <summary>
    ///     Parses primary expressions: literals, identifiers, function calls, parentheses
    /// </summary>
    private Expression ParsePrimaryExpression(ref Token? token)
    {
        // Handle parentheses for precedence override
        if (token is ParenthesisOpen)
        {
            token = token.Next; // Skip '('

            // Recursively parse the expression inside parentheses
            Expression innerExpr = ParseExpression(ref token);

            // Expect closing parenthesis
            if (token is not ParenthesisClosed)
            {
                throw new UnexpectedTokenException(token!, typeof(ParenthesisClosed));
            }

            token = token.Next; // Skip ')'
            return innerExpr;
        }

        if (token is BracketOpen)
        {
            // Array literal: [1, 2, 3, 4]
            token = token.Next; // Move past '['

            List<Expression> elements = [];

            // Handle empty array []
            if (token is BracketClosed)
            {
                token = token.Next; // Move past ']'
                return new ArrayLiteralExpression(elements);
            }

            // Parse array elements
            while (token != null)
            {
                // Parse element expression
                Expression elementExpr = ParsePrimaryExpression(ref token);
                elements.Add(elementExpr);

                // Check for comma or closing bracket
                if (token is CommaToken)
                {
                    token = token.Next; // Move past comma

                    // Allow trailing comma: [1, 2, 3,]
                    if (token is BracketClosed)
                    {
                        break;
                    }
                }
                else if (token is BracketClosed)
                {
                    break;
                }
                else
                {
                    throw new UnexpectedTokenException(token!, typeof(CommaToken), typeof(BracketClosed));
                }
            }

            if (token is not BracketClosed)
            {
                throw new UnexpectedTokenException(token!, typeof(BracketClosed));
            }

            token = token.Next; // Move past ']'
            return new ArrayLiteralExpression(elements);
        }

        if (token is ChainToken)
        {
            // CHAIN keyword for array creation: CHAIN <size>
            token = token.Next; // Move past CHAIN

            if (token is not ValueToken sizeToken)
            {
                throw new UnexpectedTokenException(token!, typeof(ValueToken));
            }

            ArrayCreationExpression expr = new(sizeToken);
            token = token.Next;
            return expr;
        }

        if (token is ValueToken valueToken)
        {
            LiteralExpression expr = new(valueToken);
            token = token.Next;
            return expr;
        }

        if (token is StringLiteralToken stringToken)
        {
            StringLiteralExpression expr = new(stringToken);
            token = token.Next;
            return expr;
        }

        if (token is TemplateStringToken templateToken)
        {
            TemplateStringExpression expr = new(templateToken);
            token = token.Next;
            return expr;
        }

        if (token is IdentifierToken identifierToken)
        {
            // Start with identifier expression
            Expression currentExpr = new IdentifierExpression(identifierToken);
            token = identifierToken.Next;

            // Check for array index access (supports chaining: arr[0][1][2])
            while (token is BracketOpen)
            {
                token = token.Next; // Move past '['
                Expression indexExpr = ParseExpression(ref token);

                // Expect closing bracket
                if (token is not BracketClosed)
                {
                    throw new UnexpectedTokenException(token!, typeof(BracketClosed));
                }

                token = token.Next; // Skip ']'

                currentExpr = new IndexExpression
                {
                    ArrayExpression = currentExpr,
                    Index = indexExpr
                };
            }

            // Check if it's a function call (only if no array indexing was done)
            if (currentExpr is IdentifierExpression && token is ParenthesisOpen)
            {
                // Parse function arguments
                var (arguments, tokenAfterArgs) = ParseFunctionArguments(token);

                FunctionCallExpression funcCallExpr = new()
                {
                    FunctionName = identifierToken
                };
                funcCallExpr.Arguments.AddRange(arguments);

                token = tokenAfterArgs;
                return funcCallExpr;
            }

            // Return the expression (could be IdentifierExpression or IndexExpression)
            return currentExpr;
        }

        // Handle unary minus for negative numbers
        if (token is MinusToken minusToken)
        {
            token = token.Next; // Move past minus

            if (token is ValueToken negValueToken)
            {
                // Create negative value token
                token = token.Next;
                RawToken negativeRaw = RawToken.Create($"-{negValueToken.RawToken.OriginalText}");
                ValueToken negativeToken = new(negativeRaw);
                return new LiteralExpression(negativeToken);
            }

            // If it's not followed by a number, it might be a minus expression
            // Put the token back and let the caller handle it
            token = minusToken;
        }

        throw new NotImplementedException(
            $"Expression type {token?.GetType().Name} not yet supported in FLEX variable initialization");
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