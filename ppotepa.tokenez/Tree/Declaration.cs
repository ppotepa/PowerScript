#nullable enable
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Base class for all declarations (functions, parameters, variables).
    ///     Declarations define new named entities that can be referenced later.
    /// </summary>
    public abstract class Declaration
    {
        protected Declaration(Token identifier)
        {
            Identifier = identifier;
        }

        /// <summary>The identifier token containing the name of this declaration</summary>
        public Token Identifier { get; set; }
    }
}