using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Builders
{
    internal class TokenProcessorRegistry
    {
        private readonly List<ITokenProcessor> _processors = new();

        public void Register(ITokenProcessor processor)
        {
            _processors.Add(processor);
        }

        public ITokenProcessor GetProcessor(Token token)
        {
            return _processors.FirstOrDefault(p => p.CanProcess(token));
        }

        public bool HasProcessor(Token token)
        {
            return _processors.Any(p => p.CanProcess(token));
        }
    }
}
