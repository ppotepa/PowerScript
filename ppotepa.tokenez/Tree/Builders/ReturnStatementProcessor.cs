using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Scoping;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Processes RETURN keyword tokens.
    /// Responsible for:
    /// - Validating RETURN is inside a function
    /// - Parsing the return expression
    /// - Marking the scope as having a valid RETURN statement
    /// </summary>
    internal class ReturnStatementProcessor : ITokenProcessor
    {
        private readonly ExpectationValidator _validator;

        public ReturnStatementProcessor(ExpectationValidator validator)
        {
            _validator = validator;
        }

        public bool CanProcess(Token token)
        {
            return token is ReturnKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            // Enforce language rule: RETURN can only appear inside functions
            if (!context.IsInsideFunction)
            {
                throw new InvalidReturnStatementException(token);
            }

            // Parse the expression after RETURN keyword
            var expression = ParseExpression(token.Next);

            // Create and register the RETURN statement
            var returnStatement = new ReturnStatement(token, expression);
            context.CurrentScope.Statements.Add(returnStatement);
            context.CurrentScope.HasReturn = true;  // Mark scope as having RETURN

            // Find where the expression ends (at scope end)
            var nextToken = FindNextToken(token.Next);

            return TokenProcessingResult.Continue(nextToken);
        }

        /// <summary>
        /// Parses an expression following the RETURN keyword.
        /// Currently supports:
        /// - Simple identifiers (e.g., RETURN x)
        /// - Literal values (e.g., RETURN 42)
        /// - Binary operations (e.g., RETURN a + b)
        /// </summary>
        private Expression ParseExpression(Token startToken)
        {
            if (startToken == null)
                throw new UnexpectedTokenException(startToken, "Expected expression after RETURN");

            if (startToken is IdentifierToken identToken)
            {
                // Check if followed by an operator (binary expression)
                if (startToken.Next is PlusToken or MinusToken or MultiplyToken or DivideToken)
                {
                    var left = new IdentifierExpression(identToken);
                    var op = startToken.Next;
                    var right = ParseSimpleExpression(op.Next);
                    return new BinaryExpression(left, op, right);
                }

                // Simple identifier
                return new IdentifierExpression(identToken);
            }

            // Check for literal value
            if (startToken is ValueToken valueToken)
            {
                return new LiteralExpression(valueToken);
            }

            throw new UnexpectedTokenException(startToken, typeof(IdentifierToken), typeof(ValueToken));
        }

        /// <summary>
        /// Parses a simple expression (identifier or value without operators).
        /// Used as the right-hand side of binary expressions.
        /// </summary>
        private Expression ParseSimpleExpression(Token token)
        {
            if (token is IdentifierToken identToken)
                return new IdentifierExpression(identToken);

            if (token is ValueToken valueToken)
                return new LiteralExpression(valueToken);

            throw new UnexpectedTokenException(token, typeof(IdentifierToken), typeof(ValueToken));
        }

        /// <summary>
        /// Finds the token after the return expression ends.
        /// Navigates through the expression tokens until reaching scope end.
        /// </summary>
        private Token FindNextToken(Token expressionStart)
        {
            var current = expressionStart;

            // Skip through expression tokens
            while (current != null && current is not ScopeEndToken)
            {
                if (current is IdentifierToken or ValueToken)
                {
                    // Check if this is part of a binary expression
                    if (current.Next is PlusToken or MinusToken or MultiplyToken or DivideToken)
                    {
                        // Skip the operator and right operand
                        current = current.Next?.Next;
                    }
                    else
                    {
                        // Simple expression, move past it
                        current = current.Next;
                    }
                }
                else
                {
                    current = current.Next;
                }

                // Stop at scope end
                if (current is ScopeEndToken)
                    return current;
            }

            return current;
        }
    }
}
