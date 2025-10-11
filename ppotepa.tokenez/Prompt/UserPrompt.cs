using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Prompt
{
    /// <summary>
    /// Represents user input code that will be tokenized.
    /// User code defines functions directly at root scope, which serves as a standard library.
    /// </summary>
    public class UserPrompt
    {
        private RawToken[] _rawTokens = default;
        private string[] _commandLineArgs;

        /// <summary>
        /// Creates a new user prompt.
        /// </summary>
        /// <param name="prompt">The user's code input</param>
        /// <param name="commandLineArgs">Command line arguments (reserved for future use)</param>
        public UserPrompt(string prompt, string[] commandLineArgs = null)
        {
            Prompt = prompt;
            _commandLineArgs = commandLineArgs ?? Array.Empty<string>();
            WrappedPrompt = prompt; // No wrapping needed - root scope is the standard library
        }

        /// <summary>
        /// The original user code
        /// </summary>
        public string Prompt { get; }

        /// <summary>
        /// The code to process (same as Prompt - no wrapping)
        /// </summary>
        public string WrappedPrompt { get; }

        /// <summary>
        /// Lazily tokenizes the wrapped prompt into raw tokens.
        /// Separates all operators, delimiters, and keywords into individual tokens.
        /// Preserves string literals as single tokens.
        /// </summary>
        public RawToken[] RawTokens
        {
            get
            {
                // First, extract and protect string literals
                var text = WrappedPrompt.Trim();
                var tokens = new List<string>();
                int i = 0;

                while (i < text.Length)
                {
                    // Handle string literals
                    if (text[i] == '"')
                    {
                        int startQuote = i;
                        i++; // Skip opening quote
                        while (i < text.Length && text[i] != '"')
                        {
                            i++;
                        }
                        if (i < text.Length)
                        {
                            i++; // Include closing quote
                            tokens.Add(text.Substring(startQuote, i - startQuote));
                        }
                    }
                    // Handle other characters
                    else if (char.IsWhiteSpace(text[i]))
                    {
                        i++;
                    }
                    else
                    {
                        int start = i;
                        while (i < text.Length && !char.IsWhiteSpace(text[i]) && text[i] != '"')
                        {
                            i++;
                        }
                        tokens.Add(text.Substring(start, i - start));
                    }
                }

                // Now add spaces around delimiters and operators (but not inside string literals)
                var processedTokens = new List<string>();
                foreach (var token in tokens)
                {
                    if (token.StartsWith("\""))
                    {
                        // Keep string literals as-is
                        processedTokens.Add(token);
                    }
                    else
                    {
                        // Add spaces around delimiters and operators
                        var processed = token
                            .Replace("::", " :: ")  // Namespace operator
                            .Replace(".", " . ")     // Dot operator
                            .Replace("{", " { ")
                            .Replace("}", " } ")
                            .Replace(")", " ) ")
                            .Replace("(", " ( ")
                            .Replace("[", " [ ")
                            .Replace("]", " ] ")
                            .Replace(",", " , ")
                            .Replace("+", " + ")
                            .Replace("-", " - ")
                            .Replace("*", " * ")
                            .Replace("/", " / ")
                            .Replace("=", " = ");   // Assignment operator

                        processedTokens.AddRange(processed.Split([' '], StringSplitOptions.RemoveEmptyEntries));
                    }
                }

                // Create raw tokens only once (lazy initialization)
                _rawTokens ??= processedTokens.Select(RawToken.Create).ToArray();

                return _rawTokens;
            }
        }
    }
}
