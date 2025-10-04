using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens
{
    public abstract class Token
    {
        private readonly RawToken _rawToken = default;

        protected Token() { }
        protected Token(RawToken rawToken)
        {
            this._rawToken = rawToken;
        }


        public abstract Token[] Expects { get; }
        public RawToken RawToken => _rawToken;
        public TokenTree Tree { get; }
    }
}