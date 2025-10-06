using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Prompt
{
    /// <summary>
    /// Represents user input code that will be tokenized.
    /// Automatically wraps user code in a MAIN function to ensure all code executes in a function context.
    /// </summary>
    public class UserPrompt
    {
        private RawToken[] _rawTokens = default;
        private string[] _commandLineArgs;

        /// <summary>
        /// Creates a new user prompt and wraps it in a MAIN function.
        /// </summary>
        /// <param name="prompt">The user's code input</param>
        /// <param name="commandLineArgs">Command line arguments to pass to MAIN function</param>
        public UserPrompt(string prompt, string[] commandLineArgs = null)
        {
            Prompt = prompt;
            _commandLineArgs = commandLineArgs ?? Array.Empty<string>();
            WrappedPrompt = WrapInMainFunction(prompt, _commandLineArgs);
        }

        /// <summary>
        /// The original user code before wrapping
        /// </summary>
        public string Prompt { get; }

        /// <summary>
        /// The user code wrapped in a MAIN function declaration
        /// </summary>
        public string WrappedPrompt { get; }

        /// <summary>
        /// Wraps user code in a MAIN function to ensure all code runs in a function scope.
        /// This enforces the language rule that all executable code must be within a function.
        /// </summary>
        private string WrapInMainFunction(string userCode, string[] args)
        {
            // Create MAIN function with empty parameter list - args are implicitly available
            return $"FUNCTION MAIN ( ) {{ {userCode} }}";
        }

        /// <summary>
        /// Lazily tokenizes the wrapped prompt into raw tokens.
        /// Separates all operators, delimiters, and keywords into individual tokens.
        /// </summary>
        public RawToken[] RawTokens
        {
            get
            {
                // Add spaces around all delimiters and operators to ensure they're separate tokens
                string[] split = [.. WrappedPrompt
                    .Trim()
                    .Replace("{", " { ")      // Scope start
                    .Replace("}", " } ")      // Scope end
                    .Replace(")", " ) ")      // Parenthesis close
                    .Replace("(", " ( ")      // Parenthesis open
                    .Replace("[", " ] ")      // Array bracket start (future use)
                    .Replace("]", " [ ")      // Array bracket end (future use)
                    .Replace(",", " , ")      // Parameter separator
                    .Replace("+", " + ")      // Addition operator
                    .Replace("-", " - ")      // Subtraction operator
                    .Replace("*", " * ")      // Multiplication operator
                    .Replace("/", " / ")      // Division operator
                    .Split([' '], StringSplitOptions.RemoveEmptyEntries)];

                // Create raw tokens only once (lazy initialization)
                _rawTokens ??= split.Select(RawToken.Create).ToArray();

                return _rawTokens;
            }
        }
    }
}
