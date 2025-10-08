using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Operators
{
    /// <summary>
    /// Token representing the namespace operator ::.
    /// Used for accessing .NET namespaces after the NET keyword.
    /// Example: "NET::System.Console"
    /// </summary>
    public class NamespaceOperatorToken : Token
    {
        public NamespaceOperatorToken()
        {
        }

        public NamespaceOperatorToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After ::, expect a namespace or class identifier</summary>
        public override Type[] Expectations => [typeof(IdentifierToken)];
    }
}
