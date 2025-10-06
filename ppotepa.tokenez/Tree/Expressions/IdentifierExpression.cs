using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;

namespace ppotepa.tokenez.Tree.Expressions
{
    public class IdentifierExpression : Expression
    {
        public IdentifierToken Identifier { get; set; }

        public override string ExpressionType => "Identifier";

        public IdentifierExpression(IdentifierToken identifier)
        {
            StartToken = identifier;
            Identifier = identifier;
        }
    }
}
