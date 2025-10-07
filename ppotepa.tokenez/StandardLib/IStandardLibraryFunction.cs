namespace ppotepa.tokenez.StandardLib
{
    /// <summary>
    /// Interface for all standard library functions in PowerScript.
    /// </summary>
    public interface IStandardLibraryFunction
    {
        /// <summary>
        /// The name of the function (e.g., "PRINT", "ADD").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of what the function does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The function's parameters.
        /// </summary>
        Parameter[] Parameters { get; }

        /// <summary>
        /// The return type of the function.
        /// </summary>
        string ReturnType { get; }

        /// <summary>
        /// Executes the function with the provided arguments.
        /// </summary>
        object Execute(params object[] args);
    }

    /// <summary>
    /// Represents a function parameter.
    /// </summary>
    public record Parameter(string Name, string Type, string Description = "");
}
