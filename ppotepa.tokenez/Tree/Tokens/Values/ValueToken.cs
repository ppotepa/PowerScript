using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Tokens.Values
{
    /// <summary>
    /// Token representing a literal value (number, string, etc.).
    /// Examples: 42, "hello", 3.14
    /// </summary>
    public class ValueToken : Token
    {
        public ValueToken()
        {
        }

        /// <summary>After value, context-dependent (handled by processors)</summary>
        public override Type[] Expectations => [];
    }
}