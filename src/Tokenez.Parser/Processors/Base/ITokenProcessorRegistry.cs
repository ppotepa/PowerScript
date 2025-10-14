using Tokenez.Parser.Processors.Base;
using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Parser.Processors.Base;

/// <summary>
/// Interface for token processor registry that manages token processors.
/// </summary>
public interface ITokenProcessorRegistry
{
    /// <summary>
    /// Registers a token processor.
    /// </summary>
    /// <param name="processor">The processor to register</param>
    void Register(ITokenProcessor processor);

    /// <summary>
    /// Gets a processor that can handle the given token.
    /// </summary>
    /// <param name="token">The token to find a processor for</param>
    /// <returns>A processor that can handle the token, or null if none found</returns>
    ITokenProcessor? GetProcessor(Token token);
}
