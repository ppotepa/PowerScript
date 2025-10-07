using ppotepa.tokenez.Tree.Builders;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    /// Partial class that handles the initialization and coordination of the token tree building process.
    /// Uses dependency injection pattern to set up processors, validators, and builders.
    /// </summary>
    public partial class TokenTree
    {
        private readonly ScopeBuilder _scopeBuilder;
        private readonly TokenProcessorRegistry _registry;
        private readonly ExpectationValidator _validator;

        /// <summary>
        /// Initializes the token tree builder with all necessary processors.
        /// Sets up the processing pipeline following Single Responsibility Principle.
        /// </summary>
        public TokenTree()
        {
            // Create core infrastructure
            _validator = new ExpectationValidator();
            _registry = new TokenProcessorRegistry();

            // Create the scope builder first (needed by ScopeProcessor)
            _scopeBuilder = new ScopeBuilder(_registry, _validator);

            // Create specialized processors for different token types
            var parameterProcessor = new ParameterProcessor();
            var functionProcessor = new FunctionProcessor(parameterProcessor, _validator);
            var returnProcessor = new ReturnStatementProcessor(_validator);
            // var printProcessor = new PrintStatementProcessor(_validator); // TODO: Re-implement PRINT
            var scopeProcessor = new ScopeProcessor(_registry, _validator, _scopeBuilder);

            // Register all processors with the central registry
            _registry.Register(functionProcessor);
            _registry.Register(returnProcessor);
            // _registry.Register(printProcessor); // TODO: Re-implement PRINT
            _registry.Register(scopeProcessor);
        }

        /// <summary>
        /// Creates and builds a scope hierarchy starting from the given token.
        /// Delegates to ScopeBuilder which uses the processor registry to handle different token types.
        /// </summary>
        /// <param name="currentToken">The token to start building from</param>
        /// <param name="scope">The parent scope</param>
        /// <param name="depth">Current nesting depth (for logging)</param>
        /// <param name="iteration">Iteration count (deprecated, kept for compatibility)</param>
        /// <param name="parenthesisDepth">Parenthesis depth (deprecated, kept for compatibility)</param>
        /// <returns>The built scope with all nested scopes and declarations</returns>
        public Scope CreateScope(Token currentToken, Scope scope, int depth = 0, int iteration = 0, int parenthesisDepth = 0)
        {
            return _scopeBuilder.BuildScope(currentToken, scope, depth);
        }
    }
}