using ppotepa.tokenez.DotNet;
using ppotepa.tokenez.Tree.Builders;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Partial class that handles the initialization and coordination of the token tree building process.
    ///     Uses dependency injection pattern to set up processors, validators, and builders.
    /// </summary>
    public partial class TokenTree
    {
        private readonly DotNetLinker _dotNetLinker;
        private readonly TokenProcessorRegistry _registry;
        private readonly ScopeBuilder _scopeBuilder;
        private readonly ExpectationValidator _validator;

        /// <summary>
        ///     Initializes the token tree builder with all necessary processors.
        ///     Sets up the processing pipeline following Single Responsibility Principle.
        /// </summary>
        public TokenTree()
        {
            // Create core infrastructure
            _validator = new ExpectationValidator();
            _registry = new TokenProcessorRegistry();
            _dotNetLinker = new DotNetLinker();

            // Create the scope builder first (needed by ScopeProcessor)
            _scopeBuilder = new ScopeBuilder(_registry, _validator);

            // Create specialized processors for different token types
            ParameterProcessor parameterProcessor = new();
            FunctionProcessor functionProcessor = new(parameterProcessor, _validator);
            FunctionCallProcessor functionCallProcessor = new();
            LinkStatementProcessor linkProcessor = new(_dotNetLinker);
            FlexVariableProcessor flexVariableProcessor = new();
            CycleLoopProcessor cycleLoopProcessor = new(_scopeBuilder);
            IfStatementProcessor ifStatementProcessor = new(_validator, _scopeBuilder);
            ReturnStatementProcessor returnProcessor = new(_validator);
            PrintStatementProcessor printProcessor = new(_validator);
            ExecuteCommandProcessor executeProcessor = new(_validator);
            NetMethodCallProcessor netMethodCallProcessor = new(_validator);
            VariableDeclarationProcessor variableDeclarationProcessor = new(_validator);
            ScopeProcessor scopeProcessor = new(_registry, _validator, _scopeBuilder);

            // Register all processors with the central registry
            // LINK processor should be registered first since LINK statements must come at the top
            _registry.Register(linkProcessor);
            _registry.Register(functionProcessor);
            _registry.Register(functionCallProcessor); // Function calls (identifier followed by parentheses)
            _registry.Register(flexVariableProcessor); // FLEX variable declarations
            _registry.Register(cycleLoopProcessor); // CYCLE loops
            _registry.Register(ifStatementProcessor); // IF conditional statements
            _registry.Register(returnProcessor);
            _registry.Register(printProcessor);
            _registry.Register(executeProcessor);
            _registry.Register(netMethodCallProcessor);
            _registry.Register(variableDeclarationProcessor);
            _registry.Register(scopeProcessor);
        }

        /// <summary>
        ///     Creates and builds a scope hierarchy starting from the given token.
        ///     Delegates to ScopeBuilder which uses the processor registry to handle different token types.
        /// </summary>
        /// <param name="currentToken">The token to start building from</param>
        /// <param name="scope">The parent scope</param>
        /// <param name="depth">Current nesting depth (for logging)</param>
        /// <param name="iteration">Iteration count (deprecated, kept for compatibility)</param>
        /// <param name="parenthesisDepth">Parenthesis depth (deprecated, kept for compatibility)</param>
        /// <returns>The built scope with all nested scopes and declarations</returns>
        public Scope CreateScope(Token currentToken, Scope scope, int depth = 0, int iteration = 0,
            int parenthesisDepth = 0)
        {
            return _scopeBuilder.BuildScope(currentToken, scope, depth);
        }
    }
}