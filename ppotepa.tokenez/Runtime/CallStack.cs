#nullable enable
using System.Text;

namespace ppotepa.tokenez.Runtime;

/// <summary>
///     Manages the call stack for function invocations.
///     Tracks which functions are currently executing.
/// </summary>
public class CallStack
{
    private readonly Stack<StackFrame> _frames = new();

    public int Depth => _frames.Count;

    public void Push(StackFrame frame)
    {
        _frames.Push(frame);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[CALL STACK] → Entering: {frame.FunctionName}() [Depth: {Depth}]");
        Console.ResetColor();
    }

    public StackFrame Pop()
    {
        if (_frames.Count == 0)
        {
            throw new InvalidOperationException("Cannot pop from empty call stack");
        }

        StackFrame frame = _frames.Pop();

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"[CALL STACK] ← Exiting: {frame.FunctionName}() [Depth: {Depth}]");
        Console.ResetColor();

        return frame;
    }

    public StackFrame? Peek()
    {
        return _frames.Count > 0 ? _frames.Peek() : null;
    }

    public string GetStackTrace()
    {
        StringBuilder trace = new();
        trace.AppendLine("Call Stack:");

        List<StackFrame> frameList = [.. _frames];
        for (int i = 0; i < frameList.Count; i++)
        {
            StackFrame frame = frameList[i];
            trace.AppendLine($"  [{i}] {frame.FunctionName}()");
        }

        return trace.ToString();
    }
}