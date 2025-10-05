using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Base
{
    public abstract class Token
    {
        private readonly RawToken _rawToken = default;
        public int Index { get; set; }
        public Token Next { get; set; }
        public Token Prev { get; set; }
        protected Token() { }

        protected Token(RawToken rawToken)
        {
            this._rawToken = rawToken;
        }

        public abstract Type[] Expectations { get; }
        public virtual bool HasDelimiter => false;
        public virtual string Delimiter => "END";
        public RawToken RawToken => _rawToken;
        public TokenTree Tree { get; }
        public virtual string KeyWord => string.Empty;
    }
}