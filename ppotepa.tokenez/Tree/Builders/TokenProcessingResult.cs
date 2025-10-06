using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Builders
{
    internal class TokenProcessingResult
    {
        public Token NextToken { get; init; }
        public bool ShouldValidateExpectations { get; init; } = true;
        public Scope ModifiedScope { get; init; }

        public static TokenProcessingResult Continue(Token nextToken, bool validateExpectations = true)
        {
            return new TokenProcessingResult
            {
                NextToken = nextToken,
                ShouldValidateExpectations = validateExpectations
            };
        }

        public static TokenProcessingResult ContinueWithScope(Token nextToken, Scope scope, bool validateExpectations = true)
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
