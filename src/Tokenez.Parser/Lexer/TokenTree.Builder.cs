using Tokenez.Core.DotNet;
using Tokenez.Parser.Processors.Base;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.AST;

namespace Tokenez.Parser.Lexer
{
    /// <summary>
    ///     Partial class that handles the initialization and coordination of the token tree building process.
    ///     Uses dependency injection pattern to set up processors, validators, and builders.
    /// </summary>
    public partial class TokenTree
    {
        private readonly IDotNetLinker _dotNetLinker;
        private readonly ITokenProcessorRegistry _registry;
        private readonly IScopeBuilder _scopeBuilder;

        /// <summary>
        ///     Initializes the token tree builder with all necessary dependencies via constructor injection.
        ///     Follows Dependency Injection and Inversion of Control principles.
        /// </summary>
        public TokenTree(
            ITokenProcessorRegistry registry,
            IDotNetLinker dotNetLinker,
            IScopeBuilder scopeBuilder)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _dotNetLinker = dotNetLinker ?? throw new ArgumentNullException(nameof(dotNetLinker));
            _scopeBuilder = scopeBuilder ?? throw new ArgumentNullException(nameof(scopeBuilder));
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