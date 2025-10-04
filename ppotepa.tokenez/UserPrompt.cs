namespace ppotepa.tokenez
{
    public class UserPrompt
    {       
        private RawTokenCollection _tokesn = default;

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
