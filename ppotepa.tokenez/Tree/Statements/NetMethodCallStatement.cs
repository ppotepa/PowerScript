using ppotepa.tokenez.Tree.Expressions;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    /// Represents a direct call to a .NET framework method.
    /// Example: NET::System.Console.WriteLine("Hello")
    /// </summary>
    public class NetMethodCallStatement : Statement
    {
        /// <summary>
        /// The fully qualified .NET method path (e.g., "System.Console.WriteLine")
        /// </summary>
        public string FullMethodPath { get; }

        /// <summary>
        /// The arguments to pass to the method
        /// </summary>
        public List<Expression> Arguments { get; }

        public override string StatementType => "NET_METHOD_CALL";

        public NetMethodCallStatement(string fullMethodPath, List<Expression> arguments)
        {
            FullMethodPath = fullMethodPath;
            Arguments = arguments ?? new List<Expression>();
        }

        public override string ToString()
        {
            var args = string.Join(", ", Arguments.Select(a => a.ToString()));
            return $"NET::{FullMethodPath}({args})";
        }
    }
}
