using PowerScript.Common.Logging;
using PowerScript.Core.AST;
using PowerScript.Core.AST.Expressions;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Scoping;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.ControlFlow;

/// <summary>
///     Processes IF keyword for conditional statements.
///     SQL-style syntax: IF condition { ... } or IF condition { ... } ELSE { ... }
///     Supports: >, <, >=, <=, ==, !=, AND, OR
/// </summary>
public class IfStatementProcessor(IScopeBuilder scopeBuilder) : ITokenProcessor
{
    private readonly IScopeBuilder _scopeBuilder = scopeBuilder;

    public bool CanProcess(Token token)
    {
        return token is IfKeywordToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Debug(
            $"IfStatementProcessor: Processing IF statement in scope '{context.CurrentScope.ScopeName}'");

        IfKeywordToken? ifToken = token as IfKeywordToken;
        Token currentToken = ifToken!.Next;

        // Parse the condition expression
        Expression condition = ParseConditionExpression(ref currentToken);

        LoggerService.Logger.Debug($"IfStatementProcessor: Condition: {condition}");

        // Expect opening brace for THEN block
        if (currentToken is not ScopeStartToken)
        {
            throw new InvalidOperationException($"Expected {{ after IF condition, got {currentToken?.GetType().Name}");
        }

        // Create THEN scope
        string thenScopeName = $"IF_THEN_{context.CurrentScope.Statements.Count}";
        Scope thenScope = new(thenScopeName)
        {
            Type = ScopeType.Block,
            OuterScope = context.CurrentScope
        };

        LoggerService.Logger.Debug($"IfStatementProcessor: Created THEN scope: {thenScopeName}");

        // Build the THEN scope body - use context.Clone() to preserve function context
        ProcessingContext thenContext = context.Clone();
        thenContext.CurrentScope = thenScope;
        thenContext.Depth = context.Depth + 1;
        _scopeBuilder.BuildScope(currentToken, thenScope, thenContext);

        // Find the closing brace of THEN scope
        Token? nextToken = currentToken;
        int braceDepth = 0;

        // Skip past the THEN scope
        while (nextToken != null)
        {
            if (nextToken is ScopeStartToken)
            {
                braceDepth++;
            }

            if (nextToken is ScopeEndToken)
            {
                braceDepth--;
                if (braceDepth == 0)
                {
                    nextToken = nextToken.Next;
                    break;
                }
            }

            nextToken = nextToken.Next;
        }

        Scope? elseScope = null;

        // Check for optional ELSE keyword AFTER finding end of THEN block
        if (nextToken is ElseKeywordToken)
        {
            nextToken = nextToken.Next; // Skip ELSE keyword

            // Expect opening brace for ELSE block
            if (nextToken is not ScopeStartToken)
            {
                throw new InvalidOperationException(
                    $"Expected {{ after ELSE keyword, got {nextToken?.GetType().Name}");
            }

            // Create ELSE scope
            string elseScopeName = $"IF_ELSE_{context.CurrentScope.Statements.Count}";
            elseScope = new Scope(elseScopeName)
            {
                Type = ScopeType.Block,
                OuterScope = context.CurrentScope
            };

            LoggerService.Logger.Debug($"IfStatementProcessor: Created ELSE scope: {elseScopeName}");

            // Build the ELSE scope body - use context.Clone() to preserve function context
            ProcessingContext elseContext = context.Clone();
            elseContext.CurrentScope = elseScope;
            elseContext.Depth = context.Depth + 1;
            _scopeBuilder.BuildScope(nextToken, elseScope, elseContext);

            // Skip past the ELSE scope
            braceDepth = 0;
            while (nextToken != null)
            {
                if (nextToken is ScopeStartToken)
                {
                    braceDepth++;
                }

                if (nextToken is ScopeEndToken)
                {
                    braceDepth--;
                    if (braceDepth == 0)
                    {
                        nextToken = nextToken.Next;
                        break;
                    }
                }

                nextToken = nextToken.Next;
            }
        }

        // Create the IF statement
        IfStatement ifStatement = new(condition, thenScope, elseScope)
        {
            StartToken = token
        };
        context.CurrentScope.Statements.Add(ifStatement);

        LoggerService.Logger.Success("IfStatementProcessor: IF statement created successfully");

        return TokenProcessingResult.Continue(nextToken!);
    }

    /// <summary>
    ///     Parse a condition expression with support for comparisons and logical operators.
    ///     Examples: a > b, x == y AND z < w, m != n OR p>= q
    /// </summary>
    private static Expression ParseConditionExpression(ref Token currentToken)
    {
        // Parse the left side of the condition
        Expression left = ParseComparisonExpression(ref currentToken);

        // Check for logical operators (AND, OR)
        while (currentToken is AndKeywordToken or OrKeywordToken)
        {
            Token logicalOp = currentToken;
            currentToken = currentToken.Next;

            Expression right = ParseComparisonExpression(ref currentToken);
            left = new LogicalExpression(left, logicalOp, right);
        }

        return left;
    }

    /// <summary>
    ///     Parse a comparison expression: leftValue operator rightValue
    ///     Operators: >, <, >=, <=, ==, !=
    ///     Handles == as two consecutive = tokens if needed
    ///     If no comparison operator is found, returns the left expression as-is
    /// </summary>
    private static Expression ParseComparisonExpression(ref Token currentToken)
    {
        // Parse left operand as a full arithmetic expression
        Expression left = ParseArithmeticExpression(ref currentToken);

        // Check if there's a comparison operator
        if (!IsComparisonOperator(currentToken))
        {
            // No comparison operator - just return the left expression
            return left;
        }

        Token comparisonOp = currentToken;

        // Handle == as two EqualsToken (tokenizer fallback)
        if (currentToken is EqualsToken && currentToken.Next is EqualsToken)
        {
            currentToken = currentToken.Next; // Skip first =
            currentToken = currentToken.Next; // Skip second =, now at right operand
        }
        else
        {
            currentToken = currentToken.Next;
        }

        // Parse right operand
        Expression right = ParseValue(ref currentToken);

        return new BinaryExpression(left, comparisonOp, right);
    }

    /// <summary>
    ///     Parse a value (identifier, array element, number, or string literal)
    /// </summary>
    private static Expression ParseValue(ref Token currentToken)
    {
        if (currentToken is IdentifierToken identifierToken)
        {
            // Check for function call: identifier(...)
            if (identifierToken.Next is ParenthesisOpen)
            {
                currentToken = identifierToken.Next; // Move to '('

                // Parse function arguments
                var (arguments, tokenAfterArgs) = ParseFunctionArguments(currentToken);

                FunctionCallExpression funcCall = new()
                {
                    FunctionName = identifierToken
                };
                funcCall.Arguments.AddRange(arguments);

                currentToken = tokenAfterArgs;
                return funcCall;
            }

            // Start with identifier expression
            Expression currentExpr = new IdentifierExpression(identifierToken);
            currentToken = identifierToken.Next;

            // Check for array indexing (supports chaining: arr[0][1])
            while (currentToken is BracketOpen)
            {
                currentToken = currentToken.Next; // Move past '['

                // Parse the index expression
                Expression indexExpr = ParseSimpleValue(ref currentToken);

                // Expect closing bracket
                if (currentToken is not BracketClosed)
                {
                    throw new InvalidOperationException(
                        $"Expected ']' after array index, got {currentToken?.GetType().Name}");
                }

                currentToken = currentToken.Next; // Move past ']'

                currentExpr = new IndexExpression
                {
                    ArrayExpression = currentExpr,
                    Index = indexExpr
                };
            }

            return currentExpr;
        }

        if (currentToken is ValueToken valueToken)
        {
            LiteralExpression expr = new(valueToken);
            currentToken = currentToken.Next;
            return expr;
        }

        if (currentToken is StringLiteralToken stringToken)
        {
            StringLiteralExpression expr = new(stringToken);
            currentToken = currentToken.Next;
            return expr;
        }

        throw new InvalidOperationException(
            $"Expected identifier, number, or string, got {currentToken?.GetType().Name}");
    }

    /// <summary>
    ///     Parse a simple value for array index (identifier or number)
    /// </summary>
    private static Expression ParseSimpleValue(ref Token currentToken)
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

        throw new InvalidOperationException(
            $"Expected value or identifier for array index, got {currentToken?.GetType().Name}");
    }

    /// <summary>
    ///     Parse an arithmetic expression with support for +, -, *, /, % operators
    /// </summary>
    private static Expression ParseArithmeticExpression(ref Token currentToken)
    {
        return ParseAdditiveExpression(ref currentToken);
    }

    /// <summary>
    ///     Parses addition and subtraction (left-to-right)
    /// </summary>
    private static Expression ParseAdditiveExpression(ref Token currentToken)
    {
        Expression left = ParseMultiplicativeExpression(ref currentToken);

        while (currentToken is PlusToken or MinusToken)
        {
            Token operatorToken = currentToken;
            currentToken = currentToken.Next;

            Expression right = ParseMultiplicativeExpression(ref currentToken);

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    /// <summary>
    ///     Parses multiplication, division, and modulo (left-to-right)
    /// </summary>
    private static Expression ParseMultiplicativeExpression(ref Token currentToken)
    {
        Expression left = ParsePrimaryExpression(ref currentToken);

        while (currentToken is MultiplyToken or DivideToken or ModuloToken)
        {
            Token operatorToken = currentToken;
            currentToken = currentToken.Next;

            Expression right = ParsePrimaryExpression(ref currentToken);

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    /// <summary>
    ///     Parses primary expressions: literals, identifiers, array access, parentheses
    /// </summary>
    private static Expression ParsePrimaryExpression(ref Token currentToken)
    {
        // Handle parentheses for precedence override
        if (currentToken is ParenthesisOpen)
        {
            currentToken = currentToken.Next; // Skip '('

            // Parse full condition expression to support comparisons in parentheses
            Expression innerExpr = ParseConditionExpression(ref currentToken);

            if (currentToken is not ParenthesisClosed)
            {
                throw new InvalidOperationException(
                    $"Expected ')' after expression, got {currentToken?.GetType().Name}");
            }

            currentToken = currentToken.Next; // Skip ')'
            return innerExpr;
        }

        // Delegate to ParseValue for identifiers, values, strings
        return ParseValue(ref currentToken);
    }

    /// <summary>
    ///     Check if the token is a comparison operator.
    ///     Handles == as two consecutive EqualsToken if needed.
    /// </summary>
    private static bool IsComparisonOperator(Token token)
    {
        if (token is EqualsToken && token.Next is EqualsToken)
        {
            // Handle == as two EqualsToken (tokenizer fallback)
            return true;
        }

        return token is GreaterThanToken
            or LessThanToken
            or GreaterThanOrEqualToken
            or LessThanOrEqualToken
            or EqualsEqualsToken
            or NotEqualsToken;
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
            IdentifierToken it when it.Next is ParenthesisOpen => ParseFunctionCallInExpression(it),
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
                IdentifierToken it when it.Next is ParenthesisOpen => ParseFunctionCallInExpression(it),
                IdentifierToken it => new IdentifierExpression(it),
                _ => throw new InvalidOperationException($"Unexpected token type: {rightToken.GetType().Name}")
            };

            left = new BinaryExpression(left, operatorToken, right);
        }

        return left;
    }

    private static FunctionCallExpression ParseFunctionCallInExpression(IdentifierToken functionNameToken)
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