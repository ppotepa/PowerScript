using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Exceptions
{
    /// <summary>
    ///     Exception thrown when a token doesn't match expected types during parsing.
    ///     Provides detailed error messages with code context visualization,
    ///     showing the problematic token in context with surrounding tokens.
    /// </summary>
    public class UnexpectedTokenException : Exception
    {
        /// <summary>
        ///     Creates exception with automatic error message including code view.
        /// </summary>
        public UnexpectedTokenException(Token token, params Type[] expectedTypes)
            : base(BuildMessage(token, expectedTypes))
        {
            Token = token;
            ExpectedTypes = expectedTypes;
        }

        /// <summary>
        ///     Creates exception with custom message prefix.
        /// </summary>
        public UnexpectedTokenException(Token token, string customMessage, params Type[] expectedTypes)
            : base($"{customMessage}\n{BuildMessage(token, expectedTypes)}")
        {
            Token = token;
            ExpectedTypes = expectedTypes;
        }

        /// <summary>The token that caused the exception</summary>
        public Token Token { get; }

        /// <summary>Array of token types that were expected at this position</summary>
        public Type[] ExpectedTypes { get; }

        /// <summary>
        ///     Builds the complete error message with code visualization.
        ///     Format: [code line with context] \n [underline] \n [expected types]
        /// </summary>
        private static string BuildMessage(Token token, Type[] expectedTypes)
        {
            var codeView = BuildCodeView(token);
            var expected = expectedTypes.Length > 0
                ? $"expected {string.Join(" or ", expectedTypes.Select(t => FormatTypeName(t.Name)))}"
                : "expected: unknown";

            return $"\n{codeView}\n{expected}";
        }

        /// <summary>
        ///     Formats token type name for display (removes "Token" suffix, lowercases).
        ///     Example: "ReturnKeywordToken" -> "returnkeyword"
        /// </summary>
        private static string FormatTypeName(string name)
        {
            // Remove "Token" suffix for cleaner output
            return name.Replace("Token", "").ToLower();
        }

        private static string BuildCodeView(Token token)
        {
            if (token == null) return "error: null token";

            var codeLine = BuildCodeLine(token);
            var underline = BuildUnderline(token, codeLine);

            var result = $"{codeLine}\n{underline}";

            return result;
        }

        /// <summary>
        ///     Builds a single line of code showing the error token with surrounding context.
        ///     Collects 2 tokens before + error token + 2 tokens after.
        /// </summary>
        private static string BuildCodeLine(Token token)
        {
            List<Token> tokens = [];

            // Collect previous tokens (2 before current)
            var previousTokens = token.GetPrevious(2);
            tokens.AddRange(previousTokens.Reverse());

            // Add current token
            tokens.Add(token);

            // Collect next tokens (2 after current)
            var nextTokens = token.GetNext(2);
            tokens.AddRange(nextTokens);

            // Build code line with proper spacing
            return string.Join(" ", tokens.Select(t => t.RawToken?.Text ?? "?"));
        }

        /// <summary>
        ///     Builds an underline (^^^^) pointing to the error token.
        ///     Calculates position based on preceding tokens' lengths.
        /// </summary>
        private static string BuildUnderline(Token token, string codeLine)
        {
            // Calculate position of error token in the code line
            var position = 0;

            // Get previous tokens and calculate their total length
            var previousTokens = token.GetPrevious(2);
            foreach (var prevToken in
                     previousTokens.Reverse()) position += (prevToken.RawToken?.Text?.Length ?? 0) + 1; // +1 for space

            // Create underline with ^^^^^ matching the error token length
            var tokenLength = token.RawToken?.Text?.Length ?? 1;
            string spaces = new(' ', position);
            string underline = new('^', tokenLength);

            return $"{spaces}{underline}";
        }
    }
}