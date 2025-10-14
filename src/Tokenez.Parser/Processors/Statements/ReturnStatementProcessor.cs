using Tokenez.Common.Logging;
using Tokenez.Parser.Processors.Base;
using Tokenez.Core.Exceptions;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.AST.Statements;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Delimiters;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Operators;
using Tokenez.Core.Syntax.Tokens.Scoping;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Parser.Processors
{
    /// <summary>
    ///     Processes RETURN keyword tokens.
    ///     Responsible for:
    ///     - Validating RETURN is inside a function
    ///     - Parsing the return expression (or null for void returns)
    ///     - Marking the scope as having a valid RETURN statement
    ///     Supports both value returns (RETURN expr) and void returns (RETURN)
    /// </summary>
    public class ReturnStatementProcessor : ITokenProcessor
    {

        public bool CanProcess(Token token)
        {
            return token is ReturnKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            LoggerService.Logger.Debug(
                $"ReturnStatementProcessor: Processing RETURN token '{token.RawToken?.Text}' in scope '{context.CurrentScope.ScopeName}'");

            // Parse the expression after RETURN keyword
            Token? currentToken = token.Next;
            var expression = currentToken is null or ScopeEndToken ? null : ParseFullExpression(ref currentToken);

            // Enforce language rule: RETURN with a value can only appear inside functions
            // Void RETURN is allowed anywhere (acts as early exit/break)
            if (!context.IsInsideFunction && expression != null)
            {
                throw new InvalidReturnStatementException(token);
            }

            // Create and register the RETURN statement
            ReturnStatement returnStatement = new(token, expression)
            {
                StartToken = token
            };
            context.CurrentScope.Statements.Add(returnStatement);
            context.CurrentScope.HasReturn = true; // Mark scope as having RETURN

            LoggerService.Logger.Debug($"Registered RETURN statement in scope '{context.CurrentScope.ScopeName}'");

            // currentToken now points to the token after the expression
            LoggerService.Logger.Debug(
                $"ReturnStatementProcessor: Next token after RETURN is {currentToken?.GetType().Name} '{currentToken?.RawToken?.Text}'");
            return TokenProcessingResult.Continue(currentToken!);
        }

        /// <summary>
        ///     Parses an expression following the RETURN keyword.
        ///     Supports:
        ///     - Void return (e.g., RETURN) - returns null expression
        ///     - Arithmetic expressions with operators: +, -, *, /, %
        ///     - Parenthesized expressions
        ///     - Comparisons: ==, !=, <, >, <=, >=
        /// </summary>
        private Expression? ParseExpression(Token? startToken)
        {
            // Allow void return: RETURN with no expression (followed by scope end)
            if (startToken is null or ScopeEndToken) return null;

            Token? current = startToken;
            return ParseFullExpression(ref current);
        }

        /// <summary>
        ///     Parse a full expression including comparisons and logical operators
        /// </summary>
        private Expression ParseFullExpression(ref Token? token)
        {
            Expression left = ParseArithmeticExpression(ref token);

            // Handle comparison operators
            if (token is GreaterThanToken or LessThanToken or GreaterThanOrEqualToken or
                LessThanOrEqualToken or EqualsEqualsToken or NotEqualsToken)
            {
                Token comparisonOp = token;
                token = token.Next;
                Expression right = ParseArithmeticExpression(ref token);
                return new BinaryExpression(left, comparisonOp, right);
            }

            // Handle == as two EqualsToken (tokenizer fallback)
            if (token is EqualsToken && token.Next is EqualsToken)
            {
                Token equalsOp = token;
                token = token.Next.Next; // Skip both = tokens
                Expression right = ParseArithmeticExpression(ref token);
                return new BinaryExpression(left, equalsOp, right);
            }

            return left;
        }

        /// <summary>
        ///     Parse arithmetic expression with support for +, -, *, /, % operators
        /// </summary>
        private Expression ParseArithmeticExpression(ref Token? token)
        {
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
        ///     Parses multiplication, division, and modulo (left-to-right)
        /// </summary>
        private Expression ParseMultiplicativeExpression(ref Token? token)
        {
            Expression left = ParsePrimaryExpression(ref token);

            while (token is MultiplyToken or DivideToken or ModuloToken)
            {
                Token operatorToken = token;
                token = token.Next;

                Expression right = ParsePrimaryExpression(ref token);

                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        /// <summary>
        ///     Parses primary expressions: literals, identifiers, parentheses
        /// </summary>
        private Expression ParsePrimaryExpression(ref Token? token)
        {
            // Handle parentheses for precedence override
            if (token is ParenthesisOpen)
            {
                token = token.Next; // Skip '('

                Expression innerExpr = ParseFullExpression(ref token);

                if (token is not ParenthesisClosed)
                {
                    throw new UnexpectedTokenException(token!, typeof(ParenthesisClosed));
                }

                token = token.Next; // Skip ')'
                return innerExpr;
            }

            // Handle identifiers (including function calls)
            if (token is IdentifierToken identToken)
            {
                // Check for function call: identifier(...)
                if (identToken.Next is ParenthesisOpen)
                {
                    FunctionCallExpression funcCall = new()
                    {
                        FunctionName = identToken
                    };

                    token = identToken.Next; // Move to '('
                    token = token.Next; // Move past '('

                    // Parse function arguments - for now, skip to closing paren
                    // TODO: Properly parse argument expressions
                    int parenDepth = 1; // Track nested parentheses
                    while (token is not null && parenDepth > 0)
                    {
                        if (token is ParenthesisOpen)
                            parenDepth++;
                        else if (token is ParenthesisClosed)
                            parenDepth--;

                        if (parenDepth > 0)
                            token = token.Next;
                    }

                    if (token is not ParenthesisClosed)
                    {
                        throw new UnexpectedTokenException(token!, typeof(ParenthesisClosed));
                    }

                    token = token.Next; // Move past ')'
                    return funcCall;
                }

                token = token.Next;
                return new IdentifierExpression(identToken);
            }

            // Handle literal values
            if (token is ValueToken valueToken)
            {
                token = token.Next;
                return new LiteralExpression(valueToken);
            }

            throw new UnexpectedTokenException(token!, typeof(IdentifierToken), typeof(ValueToken),
                typeof(ParenthesisOpen));
        }
    }
}
