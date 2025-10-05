using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Exceptions
{
    public class UnexpectedTokenException : Exception
    {
        public Token Token { get; }
        public Type[] ExpectedTypes { get; }

        public UnexpectedTokenException(Token token, params Type[] expectedTypes)
            : base(BuildMessage(token, expectedTypes))
        {
            Token = token;
            ExpectedTypes = expectedTypes;
        }

        public UnexpectedTokenException(Token token, string customMessage, params Type[] expectedTypes)
            : base($"{customMessage}\n{BuildMessage(token, expectedTypes)}")
        {
            Token = token;
            ExpectedTypes = expectedTypes;
        }

        private static string BuildMessage(Token token, Type[] expectedTypes)
        {
            var codeView = BuildCodeView(token);
            var expected = expectedTypes.Length > 0
                ? $"expected {string.Join(" or ", expectedTypes.Select(t => FormatTypeName(t.Name)))}"
                : "expected: unknown";

            return $"\n{codeView}\n{expected}";
        }

        private static string FormatTypeName(string name)
        {
            // Remove "Token" suffix for cleaner output
            return name.Replace("Token", "").ToLower();
        }

        private static string BuildCodeView(Token token)
        {
            if (token == null)
                return "error: null token";

            var codeLine = BuildCodeLine(token);
            var underline = BuildUnderline(token, codeLine);

            // Save current color and set to red for error underline
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            var result = $"{codeLine}\n{underline}";
            Console.ForegroundColor = originalColor;

            return result;
        }

        private static string BuildCodeLine(Token token)
        {
            var tokens = new List<Token>();

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

        private static string BuildUnderline(Token token, string codeLine)
        {
            // Calculate position of error token in the code line
            int position = 0;

            // Get previous tokens and calculate their total length
            var previousTokens = token.GetPrevious(2);
            foreach (var prevToken in previousTokens.Reverse())
            {
                position += (prevToken.RawToken?.Text?.Length ?? 0) + 1; // +1 for space
            }

            // Create underline with ^^^^^ matching the error token length
            int tokenLength = token.RawToken?.Text?.Length ?? 1;
            var spaces = new string(' ', position);
            var underline = new string('^', tokenLength);

            return $"{spaces}{underline}";
        }
    }
}
