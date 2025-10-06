using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Builders
{
    internal class ExpectationValidator
    {
        public void ValidateNext(Token currentToken)
        {
            if (currentToken == null || currentToken.Expectations.Length == 0)
                return;

            var nextToken = currentToken.Next;
            if (nextToken == null)
            {
                // End of token stream - validate if expectations allow this
                return;
            }

            bool isValid = currentToken.Expectations.Any(expectedType =>
            {
                // Check if nextToken is of the expected type or implements the expected interface
                return expectedType.IsAssignableFrom(nextToken.GetType());
            });

            if (!isValid)
            {
                throw new UnexpectedTokenException(
                    nextToken,
                    $"Token '{currentToken.GetType().Name}' has specific expectations",
                    currentToken.Expectations);
            }
        }

        public bool IsExpected(Token token, Type[] expectedTypes)
        {
            if (expectedTypes.Length == 0)
                return true;

            return expectedTypes.Any(expectedType =>
                expectedType.IsAssignableFrom(token.GetType()));
        }
    }
}
