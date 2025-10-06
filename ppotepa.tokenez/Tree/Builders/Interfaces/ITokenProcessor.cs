using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Builders.Interfaces
{
    /// <summary>
    /// Contract for all token processors. Each keyword or special token type should have its own processor.
    /// </summary>
    internal interface ITokenProcessor
    {
        /// <summary>
        /// Determines if this processor can handle the given token
        /// </summary>
        bool CanProcess(Token token);

        /// <summary>
        /// Processes the token and returns the result with next token information
        /// </summary>
        TokenProcessingResult Process(Token token, ProcessingContext context);
    }
}
