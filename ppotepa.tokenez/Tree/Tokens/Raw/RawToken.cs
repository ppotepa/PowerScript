namespace ppotepa.tokenez.Tree.Tokens.Raw
{
    public class RawToken
    {    
        private RawToken(string @string) 
        {
            this.Text = @string.Trim().ToLower();
        }

        public string Text { get; }

        internal static RawToken Create(string @string)
            => new(@string);

        public override string ToString() 
            => $"rawToken: {Text}";

    }
}