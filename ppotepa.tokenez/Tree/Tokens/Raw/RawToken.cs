namespace ppotepa.tokenez.Tree.Tokens.Raw
{
    /// <summary>
    /// Represents a raw lexical token (text fragment from source code).
    /// This is the lowest-level token before semantic classification.
    /// Stores both the original text (with casing) and normalized version.
    /// </summary>
    public class RawToken
    {
        private RawToken(string @string)
        {
            // Store original text preserving case
            this.OriginalText = @string.Trim();
            // Normalize: trim whitespace and uppercase for comparison
            this.Text = this.OriginalText.ToUpper();
        }

        /// <summary>The original token text (trimmed, preserving case)</summary>
        public string OriginalText { get; }

        /// <summary>The normalized token text (trimmed, uppercased for comparison)</summary>
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