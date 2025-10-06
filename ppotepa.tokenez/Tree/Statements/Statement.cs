using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Statements
{
    public abstract class Statement
    {
        public Token StartToken { get; set; }
        public abstract string StatementType { get; }
    }
}
