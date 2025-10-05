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
            var context = BuildTokenContext(token);
            var expected = expectedTypes.Length > 0
                ? $"Expected: [{string.Join(", ", expectedTypes.Select(t => t.Name))}]"
                : "Expected: [unknown]";

            return $"Unexpected token: {token?.GetType().Name ?? "null"} '{token?.RawToken?.Text ?? "null"}'\n" +
                   $"{expected}\n" +
                   $"Context: {context}";
        }

        private static string BuildTokenContext(Token token)
        {
            var parts = new List<string>();

            // token?.prev?.prev
            if (token?.Prev?.Prev != null)
                parts.Add($"{token.Prev.Prev.GetType().Name}('{token.Prev.Prev.RawToken?.Text}')");
            else
                parts.Add("_");

            // token?.prev
            if (token?.Prev != null)
                parts.Add($"{token.Prev.GetType().Name}('{token.Prev.RawToken?.Text}')");
            else
                parts.Add("_");

            // [token]
            if (token != null)
                parts.Add($"[{token.GetType().Name}('{token.RawToken?.Text}')]");
            else
                parts.Add("[null]");

            // token?.next
            if (token?.Next != null)
                parts.Add($"{token.Next.GetType().Name}('{token.Next.RawToken?.Text}')");
            else
                parts.Add("_");

            // token?.next?.next
            if (token?.Next?.Next != null)
                parts.Add($"{token.Next.Next.GetType().Name}('{token.Next.Next.RawToken?.Text}')");
            else
                parts.Add("_");

            return string.Join(", ", parts);
        }
    }
}
