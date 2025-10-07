namespace ppotepa.tokenez.StandardLib
{
    /// <summary>
    /// Base class for standard library functions with common implementation.
    /// </summary>
    public abstract class StandardLibraryFunctionBase : IStandardLibraryFunction
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract Parameter[] Parameters { get; }
        public abstract string ReturnType { get; }

        public abstract object Execute(params object[] args);

        /// <summary>
        /// Validates that the correct number of arguments were provided.
        /// </summary>
        protected void ValidateArgumentCount(object[] args, int expected)
        {
            if (args.Length != expected)
            {
                throw new ArgumentException(
                    $"{Name} expects {expected} argument(s), but {args.Length} were provided.");
            }
        }

        /// <summary>
        /// Converts an argument to the specified type with error handling.
        /// </summary>
        protected T ConvertArgument<T>(object arg, int index)
        {
            try
            {
                if (arg is T typedArg)
                    return typedArg;

                return (T)Convert.ChangeType(arg, typeof(T));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    $"Argument {index} for {Name} must be of type {typeof(T).Name}.", ex);
            }
        }
    }
}
