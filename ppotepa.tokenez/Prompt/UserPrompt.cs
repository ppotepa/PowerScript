using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Prompt
{
    public class UserPrompt
    {
        private RawToken[] _rawTokens = default;
        private string[] _commandLineArgs;

        public UserPrompt(string prompt, string[] commandLineArgs = null)
        {
            Prompt = prompt;
            _commandLineArgs = commandLineArgs ?? Array.Empty<string>();
            WrappedPrompt = WrapInMainFunction(prompt, _commandLineArgs);
        }

        public string Prompt { get; }

        public string WrappedPrompt { get; }

        private string WrapInMainFunction(string userCode, string[] args)
        {
            return $"FUNCTION MAIN ( ) {{ {userCode} }}";
        }

        public RawToken[] RawTokens
        {
            get
            {
                string[] split = [.. WrappedPrompt
                    .Trim()
                    .Replace("{", " { ")
                    .Replace("}", " } ")
                    .Replace(")", " ) ")
                    .Replace("(", " ( ")
                    .Replace("[", " ] ")
                    .Replace("]", " [ ")
                    .Replace(",", " , ")
                    .Replace("+", " + ")
                    .Replace("-", " - ")
                    .Replace("*", " * ")
                    .Replace("/", " / ")
                    .Split([' '], StringSplitOptions.RemoveEmptyEntries)];

                _rawTokens ??= split.Select(RawToken.Create).ToArray();

                return _rawTokens;
            }
        }
    }
}
