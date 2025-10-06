using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Context information passed through token processing pipeline
    /// </summary>
    internal class ProcessingContext
    {
        public Scope CurrentScope { get; set; }
        public int Depth { get; set; }
        public Stack<Token> ProcessingStack { get; set; } = new();
        public bool IsInsideFunction { get; set; }
        public int ParenthesisDepth { get; set; }

        public ProcessingContext(Scope currentScope, int depth)
        {
            CurrentScope = currentScope;
            Depth = depth;
        }

        public void EnterFunction()
        {
            IsInsideFunction = true;
        }

        public void ExitFunction()
        {
            IsInsideFunction = false;
        }

        public ProcessingContext Clone()
        {
            return new ProcessingContext(CurrentScope, Depth)
            {
                IsInsideFunction = IsInsideFunction,
                ParenthesisDepth = ParenthesisDepth,
                ProcessingStack = new Stack<Token>(ProcessingStack)
            };
        }
    }
}
