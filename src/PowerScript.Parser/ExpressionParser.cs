using PowerScript.Core.AST.Expressions;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Delimiters;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Keywords.Types;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Scoping;
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

            // Handle .NET interop: #identifier -> method() or #Type -> StaticMethod()
            if (currentToken is NetKeywordToken)
            {
                currentToken = currentToken.Next; // Move past #

                if (currentToken is not IdentifierToken netIdentToken)
                {
                    throw new InvalidOperationException($"Expected identifier after '#', but found {currentToken?.GetType().Name}");
                }

                // Check for arrow operator: #identifier->Member or #identifier->Method()
                if (netIdentToken.Next is ArrowToken)
                {
                    Expression baseExpr = new IdentifierExpression(netIdentToken);
                    currentToken = netIdentToken.Next; // Move to ->
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

                // If no arrow, just treat as identifier (shouldn't happen, but fallback)
                currentToken = netIdentToken.Next;
                return new IdentifierExpression(netIdentToken);
            }

            // Handle identifiers (check for function calls or member access first)
            if (currentToken is IdentifierToken identToken)
            {
                // Check for arrow operator WITHOUT #: this should NOT be allowed for .NET calls
                // But we keep it for backwards compatibility or non-.NET member access if any
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

                // Check for property access: identifier.property
                if (identToken.Next is DotToken)
                {
                    Expression target = new IdentifierExpression(identToken);
                    currentToken = identToken.Next; // Move to .

                    // Chain property accesses: obj.prop1.prop2.prop3
                    while (currentToken is DotToken)
                    {
                        currentToken = currentToken.Next; // Move past .

                        if (currentToken is not IdentifierToken propertyToken)
                        {
                            throw new InvalidOperationException($"Expected property name after '.' but found {currentToken?.GetType().Name}");
                        }

                        string propertyName = propertyToken.Value;
                        target = new PropertyAccessExpression(target, propertyName);
                        currentToken = currentToken.Next; // Move past property name
                    }

                    return target;
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

            // Handle object literals: {prop = val, ...}
            if (currentToken is ScopeStartToken)
            {
                return ParseObjectLiteral(ref currentToken);
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

        /// <summary>
        /// Parses object literal expressions: {prop = val, ...}
        /// Supports type annotation: {name = "John"} as Person
        /// Supports strict typing: {x = 1} as Point!
        /// </summary>
        private Expression ParseObjectLiteral(ref Token currentToken)
        {
            if (currentToken is not ScopeStartToken)
                throw new InvalidOperationException($"Expected '{{' to start object literal but found {currentToken?.GetType().Name}");

            currentToken = currentToken.Next; // Move past {

            var properties = new Dictionary<string, Expression>();

            // Parse properties until we hit the closing brace
            while (currentToken != null && currentToken is not ScopeEndToken)
            {
                // Parse property name
                if (currentToken is not IdentifierToken propNameToken)
                {
                    throw new InvalidOperationException($"Expected property name in object literal but found {currentToken?.GetType().Name}");
                }

                string propName = propNameToken.Value;
                currentToken = currentToken.Next;

                // Parse = sign
                if (currentToken is not EqualsToken)
                {
                    throw new InvalidOperationException($"Expected '=' after property name '{propName}' but found {currentToken?.GetType().Name}");
                }

                currentToken = currentToken.Next;

                // Parse property value
                Expression valueExpression = ParseBinaryExpression(ref currentToken, 0);
                properties.Add(propName, valueExpression);

                // Skip comma if present
                if (currentToken is CommaToken)
                {
                    currentToken = currentToken.Next;
                }
            }

            if (currentToken is not ScopeEndToken)
            {
                throw new InvalidOperationException("Unterminated object literal - expected '}'");
            }

            currentToken = currentToken.Next; // Move past }

            // Check for "as Type" or "as Type!" syntax
            string? typeName = null;
            bool isStrict = false;

            if (currentToken is AsKeywordToken)
            {
                currentToken = currentToken.Next;

                if (currentToken is IdentifierToken typeToken)
                {
                    typeName = typeToken.Value;
                    currentToken = currentToken.Next;

                    // Check for ! suffix
                    if (currentToken is ExclamationToken)
                    {
                        isStrict = true;
                        currentToken = currentToken.Next;
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Expected type name after 'as' but found {currentToken?.GetType().Name}");
                }
            }

            return new ObjectLiteralExpression(properties, typeName, isStrict);
        }
    }
}
