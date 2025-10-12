using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Result of processing a token.
    ///     Indicates which token to process next and any side effects (scope changes, validation requirements).
    /// </summary>
    internal class TokenProcessingResult
    {
        /// <summary>The next token to continue processing from</summary>
        public required Token NextToken { get; init; }

        /// <summary>Whether to validate token expectations after processing</summary>
        public bool ShouldValidateExpectations { get; init; } = true;

        /// <summary>If the processor created or modified a scope, this contains the new scope</summary>
        public Scope? ModifiedScope { get; init; }

        /// <summary>
        ///     Creates a result that continues processing from the specified token.
        /// </summary>
        public static TokenProcessingResult Continue(Token nextToken, bool validateExpectations = true)
        {
            return new TokenProcessingResult
            {
                NextToken = nextToken,
                ShouldValidateExpectations = validateExpectations
            };
        }

        /// <summary>
        ///     Creates a result that continues processing with a modified scope.
        ///     Used when entering new scopes (functions, blocks, etc.).
        /// </summary>
        public static TokenProcessingResult ContinueWithScope(Token nextToken, Scope scope,
            bool validateExpectations = true)
        {
            return new TokenProcessingResult
            {
                NextToken = nextToken,
                ModifiedScope = scope,
                ShouldValidateExpectations = validateExpectations
            };
        }
    }
}