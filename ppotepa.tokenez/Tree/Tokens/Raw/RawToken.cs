namespace ppotepa.tokenez.Tree.Tokens.Raw
{
    public class RawToken
    {    
        private RawToken(string @string) 
        {
            this.Text = @string.Trim().ToUpper();
        }

        public string Text { get; }
        public string Processed
        {
            get
            {
                return Text
                    .Replace("  ", " ")
                    .Replace("(", "( ")
                    .Replace(")", " )")                   
                    .Replace("[", "[ ")
                    .Replace("]", " ]")
                    .Trim();
            }
        }

        internal static RawToken Create(string @string)
            => new(@string);

        public override string ToString() 
            => $"rawToken: {Text}";
    }
}