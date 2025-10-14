using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.AST;

namespace Tokenez.Parser.Processors.Base
{
    /// <summary>
    ///     Interface for all token processors.
    ///     Implements the Strategy pattern - each token type can have its own processor.
    ///     Processors are registered in a registry and invoked when matching tokens are encountered.
    /// </summary>
    public interface ITokenProcessor
    {
        /// <summary>
        ///     Determines if this processor can handle the given token.
        /// </summary>
        /// <param name="token">The token to check</param>
        /// <returns>True if this processor can handle the token</returns>
        bool CanProcess(Token token);

        /// <summary>
        ///     Processes the token and returns the next token to process.
        /// </summary>
        /// <param name="token">The token to process</param>
        /// <param name="context">The current processing context</param>
        /// <returns>Result containing the next token and any scope modifications</returns>
        TokenProcessingResult Process(Token token, ProcessingContext context);
    }
}
