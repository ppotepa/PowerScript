namespace ppotepa.tokenez.StandardLib.IO
{
    /// <summary>
    /// PRINT function - outputs text to the console.
    /// Usage: PRINT("Hello World")
    /// </summary>
    public class PrintFunction : StandardLibraryFunctionBase
    {
        public override string Name => "PRINT";
        public override string Description => "Outputs a message to the console";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("message", "STRING", "The message to print")
        };
        public override string ReturnType => "VOID";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 1);
            var message = ConvertArgument<string>(args[0], 0);
            Console.WriteLine(message);
            return null;
        }
    }

    /// <summary>
    /// READ function - reads a line of input from the console.
    /// Usage: READ()
    /// </summary>
    public class ReadFunction : StandardLibraryFunctionBase
    {
        public override string Name => "READ";
        public override string Description => "Reads a line of input from the console";
        public override Parameter[] Parameters => Array.Empty<Parameter>();
        public override string ReturnType => "STRING";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 0);
            return Console.ReadLine() ?? string.Empty;
        }
    }

    /// <summary>
    /// PRINTLN function - outputs text to the console with a newline.
    /// Usage: PRINTLN("Hello World")
    /// </summary>
    public class PrintLineFunction : StandardLibraryFunctionBase
    {
        public override string Name => "PRINTLN";
        public override string Description => "Outputs a message to the console with a newline";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("message", "STRING", "The message to print")
        };
        public override string ReturnType => "VOID";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 1);
            var message = ConvertArgument<string>(args[0], 0);
            Console.WriteLine(message);
            return null;
        }
    }
}
