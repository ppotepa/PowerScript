namespace ppotepa.tokenez.Runtime
{
    /// <summary>
    ///     Special exception used to implement RETURN statement control flow.
    ///     When a RETURN statement executes, it throws this to unwind the call stack.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1064:Exceptions should be public")]
    public class ReturnSignal(object? value = null) : Exception
    {
        public object? Value { get; } = value;
    }
}