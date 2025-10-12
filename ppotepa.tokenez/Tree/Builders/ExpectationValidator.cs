using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Validates that tokens follow their declared expectations.
    ///     Each token can declare what types of tokens are valid to follow it.
    ///     This enforces language syntax rules at the token level.
    /// </summary>
    internal class ExpectationValidator
    {
        /// <summary>
        ///     Validates that the next token matches one of the current token's expectations.
        ///     Throws UnexpectedTokenException if validation fails.
        /// </summary>
        public void ValidateNext(Token currentToken)
        {
            // Tokens without expectations accept any following token
            if (currentToken == null || currentToken.Expectations.Length == 0) return;

            var nextToken = currentToken.Next;
            if (nextToken == null)
                // End of token stream - this is acceptable
                return;

            // Check if nextToken is one of the expected types
            var isValid = currentToken.Expectations.Any(expectedType =>
            {
                // Use IsAssignableFrom to handle both exact types and interface implementations
                return expectedType.IsAssignableFrom(nextToken.Type);
            });

            if (!isValid)
                throw new UnexpectedTokenException(
                    nextToken,
                    $"Token '{currentToken.GetType().Name}' has specific expectations",
                    currentToken.Expectations);
        }

        /// <summary>
        ///     Checks if a token matches any of the expected types without throwing.
        ///     Used for conditional logic where multiple paths are valid.
        /// </summary>
        public bool IsExpected(Token token, Type[] expectedTypes)
        {
            // Empty expectations mean any token is valid
            return expectedTypes.Length == 0
                ? true
                : expectedTypes.Any(expectedType =>
                    expectedType.IsAssignableFrom(token.GetType()));
        }
    }
}