using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez.Runtime
{
    /// <summary>
    ///     Represents a single function call on the call stack.
    ///     Contains information about the function being executed.
    /// </summary>
    public class StackFrame(FunctionDeclaration function)
    {
        public string FunctionName { get; } = function?.Identifier.RawToken.Text ?? throw new ArgumentNullException(nameof(function));
        public FunctionDeclaration Function { get; } = function ?? throw new ArgumentNullException(nameof(function));
        public object? ReturnValue { get; set; }

        public override string ToString()
        {
            return $"StackFrame[{FunctionName}]";
        }
    }
}