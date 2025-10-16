using PowerScript.Core.AST.Expressions;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;
using System;
using System.Collections.Generic;

namespace PowerScript.Parser
{
    /// <summary>
    /// Parses expressions using recursive descent, properly handling nested function calls
    /// and operator precedence. Works directly with Token's linked list structure (.Next).
    /// </summary>
    public class ExpressionParser
    {
        /// <summary>
        /// Parses a complete expression starting from the current token.
        /// Updates currentToken to point after the parsed expression.
        /// </summary>
        public Expression Parse(ref Token currentToken)
        {
            return ParseBinaryExpression(ref currentToken, 0);
        }

        /// <summary>
        /// Parses binary expressions with operator precedence.
        /// Higher precedence operators bind tighter (e.g., * before +).
        /// </summary>
        private Expression ParseBinaryExpression(ref Token currentToken, int minPrecedence)
        {
            var left = ParsePrimaryExpression(ref currentToken);

            while (currentToken != null && IsOperator(currentToken))
            {
                var op = currentToken;
                var precedence = GetPrecedence(op);

                if (precedence < minPrecedence)
                    break;

                currentToken = currentToken.Next; // Move past operator
                var right = ParseBinaryExpression(ref currentToken, precedence + 1);
                left = new BinaryExpression(left, op, right);
            }

            return left;
        }

        /// <summary>
        /// Parses primary expressions: literals, identifiers, function calls, parenthesized expressions.
        /// This is where nested function calls are properly handled!
        /// </summary>
        private Expression ParsePrimaryExpression(ref Token currentToken)
        {
            if (currentToken == null)
                throw new InvalidOperationException("Unexpected end of tokens in expression");

            // Handle identifiers (check for function calls or member access first)
            if (currentToken is IdentifierToken identToken)
            {
                // Check for arrow operator: identifier->Member or identifier->Method()
                if (identToken.Next is ArrowToken)
                {
                    Expression baseExpr = new IdentifierExpression(identToken);
                    currentToken = identToken.Next; // Move to ->
                    currentToken = currentToken.Next; // Move past ->

                    // Get the member name
                    if (currentToken is not IdentifierToken memberToken)
                    {
                        throw new InvalidOperationException($"Expected identifier after arrow operator, but found {currentToken?.GetType().Name}");
                    }

                    string memberName = memberToken.RawToken?.OriginalText ?? memberToken.RawToken?.Text ?? "";
                    currentToken = memberToken.Next; // Move past member name

                    // Check if it's a method call (has parentheses)
                    if (currentToken is ParenthesisOpen)
                    {
                        currentToken = currentToken.Next; // Move past '('

                        // Parse method arguments
                        List<Expression> arguments = new();
                        if (currentToken is not ParenthesisClosed)
                        {
                            while (true)
                            {
                                arguments.Add(ParseBinaryExpression(ref currentToken, 0));

                                if (currentToken is CommaToken)
                                {
                                    currentToken = currentToken.Next; // Skip comma
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        if (currentToken is not ParenthesisClosed)
                        {
                            throw new InvalidOperationException($"Expected ')' but found {currentToken?.GetType().Name}");
                        }

                        currentToken = currentToken.Next; // Move past ')'

                        return new NetMemberAccessExpression(baseExpr, memberName, arguments);
                    }
                    else
                    {
                        // Property access
                        return new NetMemberAccessExpression(baseExpr, memberName);
                    }
                }

                // Check for function call: identifier(...)
                if (identToken.Next is ParenthesisOpen)
                {
                    return ParseFunctionCall(ref currentToken);
                }

                // Check for array indexing: identifier[...]
                if (identToken.Next is BracketOpen)
                {
                    currentToken = identToken.Next.Next; // Skip identifier and [
                    var indexExpr = ParseBinaryExpression(ref currentToken, 0);

                    if (!(currentToken is BracketClosed))
                        throw new InvalidOperationException($"Expected ']' in array indexing but found {currentToken?.GetType().Name}");

                    currentToken = currentToken.Next; // Skip ]
                    return new IndexExpression
                    {
                        ArrayExpression = new IdentifierExpression(identToken),
                        Index = indexExpr
                    };
                }

                // Plain identifier
                currentToken = currentToken.Next;
                return new IdentifierExpression(identToken);
            }

            // Handle integer literals
            if (currentToken is ValueToken valueToken)
            {
                currentToken = currentToken.Next;
                return new LiteralExpression(valueToken);
            }

            // Handle decimal literals
            if (currentToken is DecimalToken decimalToken)
            {
                currentToken = currentToken.Next;
                return new LiteralExpression(decimalToken);
            }

            // Handle string literals
            if (currentToken is StringToken stringToken)
            {
                currentToken = currentToken.Next;
                return new StringLiteralExpression(stringToken);
            }

            if (currentToken is StringLiteralToken stringLiteralToken)
            {
                currentToken = currentToken.Next;
                return new StringLiteralExpression(stringLiteralToken);
            }

            // Handle parenthesized expressions
            if (currentToken is ParenthesisOpen)
            {
                currentToken = currentToken.Next; // Skip (
                var expr = ParseBinaryExpression(ref currentToken, 0);

                if (!(currentToken is ParenthesisClosed))
                    throw new InvalidOperationException($"Expected ')' but found {currentToken?.GetType().Name}");

                currentToken = currentToken.Next; // Skip )
                return expr;
            }

            // Handle unary minus (negative numbers - integer)
            if (currentToken is MinusToken && currentToken.Next is ValueToken)
            {
                currentToken = currentToken.Next; // Move to value
                var negValue = currentToken as ValueToken;
                currentToken = currentToken.Next;
                // Create a binary expression: 0 - value
                var zeroRawToken = RawToken.Create("0");
                var zeroToken = new ValueToken(zeroRawToken);
                return new BinaryExpression(
                    new LiteralExpression(zeroToken),
                    new MinusToken(),
                    new LiteralExpression(negValue!)
                );
            }

            // Handle unary minus (negative numbers - decimal)
            if (currentToken is MinusToken && currentToken.Next is DecimalToken)
            {
                currentToken = currentToken.Next; // Move to decimal
                var negDecimal = currentToken as DecimalToken;
                currentToken = currentToken.Next;
                // Create a binary expression: 0.0 - value
                var zeroRawToken = RawToken.Create("0.0");
                var zeroToken = new DecimalToken(zeroRawToken, 0.0);
                return new BinaryExpression(
                    new LiteralExpression(zeroToken),
                    new MinusToken(),
                    new LiteralExpression(negDecimal!)
                );
            }

            throw new InvalidOperationException($"Unexpected token in expression: {currentToken.GetType().Name}");
        }

        /// <summary>
        /// Parses function calls with support for nested function calls in arguments.
        /// Example: OUTER(INNER(42), OTHER(x, y))
        /// </summary>
        private FunctionCallExpression ParseFunctionCall(ref Token currentToken)
        {
            var nameToken = currentToken as IdentifierToken;

            currentToken = currentToken.Next; // Move to (
            if (!(currentToken is ParenthesisOpen))
                throw new InvalidOperationException($"Expected '(' after function name");

            currentToken = currentToken.Next; // Move past (

            var arguments = new List<Expression>();

            // Parse comma-separated arguments
            while (currentToken != null && !(currentToken is ParenthesisClosed))
            {
                // Recursively parse each argument - this handles nested function calls!
                arguments.Add(ParseBinaryExpression(ref currentToken, 0));

                if (currentToken is CommaToken)
                {
                    currentToken = currentToken.Next; // Skip comma
                }
                else if (!(currentToken is ParenthesisClosed))
                {
                    throw new InvalidOperationException($"Expected ',' or ')' in function call, but found {currentToken?.GetType().Name}");
                }
            }

            if (!(currentToken is ParenthesisClosed))
                throw new InvalidOperationException($"Expected ')' to close function call");

            currentToken = currentToken.Next; // Move past )

            FunctionCallExpression funcCall = new FunctionCallExpression
            {
                FunctionName = nameToken
            };
            funcCall.Arguments.AddRange(arguments);

            return funcCall;
        }

        /// <summary>
        /// Checks if a token is a binary operator
        /// </summary>
        private bool IsOperator(Token token)
        {
            return token is PlusToken ||
                   token is MinusToken ||
                   token is MultiplyToken ||
                   token is DivideToken ||
                   token is ModuloToken ||
                   token is EqualsEqualsToken ||
                   token is NotEqualsToken ||
                   token is GreaterThanToken ||
                   token is LessThanToken ||
                   token is GreaterThanOrEqualToken ||
                   token is LessThanOrEqualToken;
        }

        /// <summary>
        /// Returns operator precedence (higher number = higher precedence).
        /// Ensures correct evaluation order (e.g., * before +).
        /// </summary>
        private int GetPrecedence(Token op)
        {
            return op switch
            {
                EqualsEqualsToken _ => 3,
                NotEqualsToken _ => 3,
                GreaterThanToken _ => 4,
                LessThanToken _ => 4,
                GreaterThanOrEqualToken _ => 4,
                LessThanOrEqualToken _ => 4,
                PlusToken _ => 5,
                MinusToken _ => 5,
                MultiplyToken _ => 6,
                DivideToken _ => 6,
                ModuloToken _ => 6,
                _ => 0
            };
        }
    }
}
