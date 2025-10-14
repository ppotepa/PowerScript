using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords.Types;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Delimiters
{
    /// <summary>
    ///     Token representing '[' - opening square bracket.
    ///     Used for:
    ///     1. Function return type declarations: FUNCTION add(a, b)[INT]
    ///     2. Array indexing: numbers[5], array[i]
    /// </summary>
    public class BracketOpen : Token
    {
        public BracketOpen()
        {
        }

        public BracketOpen(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>
        ///     After '[', expect:
        ///     - Type token for return type in function declarations
        ///     - Identifier, value, or parenthesis for array indexing
        /// </summary>
        public override Type[] Expectations =>
        [
            typeof(ITypeToken), // For function return types
            typeof(IdentifierToken), // For array indexing: arr[varName]
            typeof(ValueToken), // For array indexing: arr[5]
            typeof(ParenthesisOpen) // For array indexing: arr[(i+1)]
        ];
    }
}