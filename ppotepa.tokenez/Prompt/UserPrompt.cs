using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Prompt
{
    public class UserPrompt
    {
        private RawToken[] _rawTokens = default;

        public UserPrompt(string prompt)
        {
            Prompt = prompt;
        }   

        public string Prompt { get; }
        public RawToken[] RawTokens
        {
            get
            {
                _rawTokens ??= [.. Prompt.Split(" ").Select(RawToken.Create)];
                return _rawTokens;
            }
        }
    }
}
