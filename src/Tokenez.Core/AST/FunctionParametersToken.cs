using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.AST;

/// <summary>
///     Token representing a function's parameter list.
///     Contains the parsed parameter declarations.
///     Example: In "FUNCTION add(NUMBER a, NUMBER b)", this holds declarations for 'a' and 'b'.
/// </summary>
public class FunctionParametersToken : Token
{
    public FunctionParametersToken()
    {
    }

    public FunctionParametersToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>No tokens expected after parameters (handled by parent processor)</summary>
    public override Type[] Expectations => [];

    /// <summary>List of parameter declarations in order</summary>
    public List<Declaration> Declarations { get; set; } = [];
}