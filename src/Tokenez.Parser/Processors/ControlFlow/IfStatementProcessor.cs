using Tokenez.Common.Logging;
using Tokenez.Core.AST;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.AST.Statements;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Delimiters;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Operators;
using Tokenez.Core.Syntax.Tokens.Scoping;
using Tokenez.Core.Syntax.Tokens.Values;
using Tokenez.Parser.Processors.Base;

namespace Tokenez.Parser.Processors.ControlFlow;

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

        Scope? elseScope = null;

        // Check for optional ELSE keyword
        if (currentToken is ElseKeywordToken)
        {
            currentToken = currentToken.Next;

            // Expect opening brace for ELSE block
            if (currentToken is not ScopeStartToken)
            {
                throw new InvalidOperationException(
                    $"Expected {{ after ELSE keyword, got {currentToken?.GetType().Name}");
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
            _scopeBuilder.BuildScope(currentToken, elseScope, elseContext);
        }

        // Create the IF statement
        IfStatement ifStatement = new(condition, thenScope, elseScope)
        {
            StartToken = token
        };
        context.CurrentScope.Statements.Add(ifStatement);

        LoggerService.Logger.Success("IfStatementProcessor: IF statement created successfully");

        // Find the closing brace and continue from there
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

        // If there's an ELSE, skip past that scope too
        if (nextToken is ElseKeywordToken && elseScope != null)
        {
            nextToken = nextToken.Next; // Skip ELSE
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
    /// </summary>
    private static BinaryExpression ParseComparisonExpression(ref Token currentToken)
    {
        // Parse left operand as a full arithmetic expression
        Expression left = ParseArithmeticExpression(ref currentToken);

        // Expect a comparison operator
        if (!IsComparisonOperator(currentToken))
        {
            throw new InvalidOperationException(
                $"Expected comparison operator (>, <, >=, <=, ==, !=), got {currentToken?.GetType().Name}");
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
                FunctionCallExpression funcCall = new()
                {
                    FunctionName = identifierToken
                };

                currentToken = identifierToken.Next; // Move to '('
                currentToken = currentToken.Next; // Move past '('

                // Parse function arguments (simple for now - skip to closing paren)
                // TODO: Implement proper argument parsing
                while (currentToken is not null and not ParenthesisClosed)
                {
                    currentToken = currentToken.Next;
                }

                if (currentToken is not ParenthesisClosed)
                {
                    throw new InvalidOperationException(
                        $"Expected ')' after function call, got {currentToken?.GetType().Name}");
                }

                currentToken = currentToken.Next; // Move past ')'
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

            Expression innerExpr = ParseArithmeticExpression(ref currentToken);

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
}