namespace ppotepa.tokenez.Runtime
{
    /// <summary>
    /// Special exception used to implement RETURN statement control flow.
    /// When a RETURN statement executes, it throws this to unwind the call stack.
    /// </summary>
    public class ReturnSignal : Exception
    {
        public object? Value { get; }

        public ReturnSignal(object? value = null)
        {
            Value = value;
        }
    }
}
