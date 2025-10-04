
namespace ppotepa.tokenez
{
    public class RawToken
    {    
        private RawToken(string @string) 
        {
            this.Text = @string.Trim();
        }

        public string Text { get; }

        internal static RawToken Create(string @string)
            => new(@string);

        public override string ToString() 
            => $"rawToken: {Text}";

    }
}