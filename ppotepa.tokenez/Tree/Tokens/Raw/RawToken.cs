namespace ppotepa.tokenez.Tree.Tokens.Raw
{
    /// <summary>
    /// Represents a raw lexical token (text fragment from source code).
    /// This is the lowest-level token before semantic classification.
    /// Stores the original text and provides normalized/processed versions.
    /// </summary>
    public class RawToken
    {
        private RawToken(string @string)
        {
            // Normalize: trim whitespace and uppercase
            this.Text = @string.Trim().ToUpper();
        }

        /// <summary>The normalized token text (trimmed, uppercased)</summary>
        public string Text { get; }

        /// <summary>
        /// Processed version with spacing around delimiters.
        /// Ensures delimiters are separated from other tokens.
        /// Example: "add(a,b)" -> "add ( a , b )"
        /// </summary>
        public string Processed
        {
            get
            {
                return Text
                    .Replace("  ", " ")      // Collapse multiple spaces
                    .Replace("(", "( ")       // Space after (
                    .Replace(")", " )")      // Space before )
                    .Replace("[", "[ ")       // Space after [
                    .Replace("]", " ]")      // Space before ]
                    .Trim();
            }
        }

        /// <summary>Factory method to create a RawToken from source text</summary>
        internal static RawToken Create(string @string)
            => new(@string);

        public override string ToString()
            => $"rawToken: {Text}";
    }
}