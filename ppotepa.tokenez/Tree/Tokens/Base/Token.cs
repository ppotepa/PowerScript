using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Base
{
    /// <summary>
    /// Base class for all tokens in the tokenizer.
    /// Tokens are linked in a doubly-linked list and can specify expected following tokens.
    /// Each token wraps a RawToken (text + position) and provides semantic meaning.
    /// </summary>
    public abstract class Token
    {
        private readonly RawToken _rawToken = default;

        /// <summary>Position index in the token sequence</summary>
        public int Index { get; set; }

        /// <summary>Next token in the linked list</summary>
        public Token Next { get; set; }

        /// <summary>Previous token in the linked list</summary>
        public Token Prev { get; set; }

        protected Token() { }

        protected Token(RawToken rawToken)
        {
            this._rawToken = rawToken;
        }

        /// <summary>
        /// Array of token types that can follow this token.
        /// Used by ExpectationValidator for syntax checking.
        /// </summary>
        public abstract Type[] Expectations { get; }

        /// <summary>Whether this token requires a delimiter (e.g., '(' needs ')')</summary>
        public virtual bool HasDelimiter => false;

        /// <summary>The delimiter keyword (e.g., "END", ")", "}")</summary>
        public virtual string Delimiter => "END";

        /// <summary>The underlying raw token (text + position)</summary>
        public RawToken RawToken => _rawToken;

        /// <summary>Reference to the token tree (for lookups)</summary>
        public TokenTree Tree { get; }

        /// <summary>Keyword text for keyword tokens (FUNCTION, RETURN, etc.)</summary>
        public virtual string KeyWord => string.Empty;

        /// <summary>
        /// Gets n previous tokens in the linked list.
        /// Used for error context display.
        /// </summary>
        public Token[] GetPrevious(int n)
        {
            var tokens = new List<Token>();
            var current = Prev;
            for (int i = 0; i < n && current != null; i++)
            {
                tokens.Add(current);
                current = current.Prev;
            }
            return tokens.ToArray();
        }

        /// <summary>
        /// Gets n next tokens in the linked list.
        /// Used for error context display and lookahead.
        /// </summary>
        public Token[] GetNext(int n)
        {
            var tokens = new List<Token>();
            var current = Next;
            for (int i = 0; i < n && current != null; i++)
            {
                tokens.Add(current);
                current = current.Next;
            }
            return tokens.ToArray();
        }
    }
}