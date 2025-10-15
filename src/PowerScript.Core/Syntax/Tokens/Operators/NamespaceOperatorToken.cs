using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Operators;

/// <summary>
///     Token representing the namespace operator ::.
///     Used for accessing .NET namespaces after the NET keyword.
///     Example: "NET::System.Console"
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