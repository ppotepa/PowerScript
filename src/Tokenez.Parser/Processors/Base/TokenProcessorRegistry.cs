using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Parser.Processors.Base
{
    /// <summary>
    ///     Registry that maps tokens to their processors.
    ///     Implements the Chain of Responsibility pattern - processors are tried in registration order.
    ///     This allows adding new token types without modifying existing code.
    /// </summary>
    public class TokenProcessorRegistry : ITokenProcessorRegistry
    {
        private readonly List<ITokenProcessor> _processors = [];

        /// <summary>
        ///     Registers a processor for handling specific token types.
        ///     Processors are checked in registration order.
        /// </summary>
        public void Register(ITokenProcessor processor)
        {
            _processors.Add(processor);
        }

        /// <summary>
        ///     Finds the first processor that can handle the given token.
        ///     Returns null if no processor is found.
        /// </summary>
        public ITokenProcessor? GetProcessor(Token token)
        {
            return _processors.FirstOrDefault(p => p.CanProcess(token));
        }

        /// <summary>
        ///     Checks if any registered processor can handle the given token.
        /// </summary>
        public bool HasProcessor(Token token)
        {
            return _processors.Any(p => p.CanProcess(token));
        }
    }
}
