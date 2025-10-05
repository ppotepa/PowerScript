using ppotepa.tokenez.Tree.Builders;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    /// Partial class responsible for coordinating the token tree building process
    /// Delegates to specialized processors following Single Responsibility Principle
    /// </summary>
    public partial class TokenTree
    {
        private readonly ScopeBuilder _scopeBuilder;

        public TokenTree()
        {
            var parameterProcessor = new ParameterProcessor();
            var functionProcessor = new FunctionProcessor(parameterProcessor);
            _scopeBuilder = new ScopeBuilder(functionProcessor);
        }

        public Scope CreateScope(Token currentToken, Scope scope, int depth = 0, int iteration = 0, int parenthesisDepth = 0)
        {
            return _scopeBuilder.BuildScope(currentToken, scope, depth);
        }
    }
}