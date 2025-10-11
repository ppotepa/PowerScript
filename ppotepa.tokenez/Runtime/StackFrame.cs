using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez.Runtime
{
    /// <summary>
    /// Represents a single function call on the call stack.
    /// Contains information about the function being executed.
    /// </summary>
    public class StackFrame
    {
        public string FunctionName { get; }
        public FunctionDeclaration Function { get; }
        public object? ReturnValue { get; set; }

        public StackFrame(FunctionDeclaration function)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));
            FunctionName = function.Identifier.RawToken.Text;
        }

        public override string ToString()
        {
            return $"StackFrame[{FunctionName}]";
        }
    }
}
