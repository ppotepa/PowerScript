using ppotepa.tokenez.Tree.Builders;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree
{
    public partial class TokenTree
    {
        private readonly ScopeBuilder _scopeBuilder;
        private readonly TokenProcessorRegistry _registry;
        private readonly ExpectationValidator _validator;

        public TokenTree()
        {
            _validator = new ExpectationValidator();
            _registry = new TokenProcessorRegistry();

            var parameterProcessor = new ParameterProcessor();
            var functionProcessor = new FunctionProcessor(parameterProcessor, _validator);
            var returnProcessor = new ReturnStatementProcessor(_validator);
            var scopeProcessor = new ScopeProcessor(_registry, _validator);

            _registry.Register(functionProcessor);
            _registry.Register(returnProcessor);
            _registry.Register(scopeProcessor);

            _scopeBuilder = new ScopeBuilder(_registry, _validator);
        }

        public Scope CreateScope(Token currentToken, Scope scope, int depth = 0, int iteration = 0, int parenthesisDepth = 0)
        {
            return _scopeBuilder.BuildScope(currentToken, scope, depth);
        }
    }
}