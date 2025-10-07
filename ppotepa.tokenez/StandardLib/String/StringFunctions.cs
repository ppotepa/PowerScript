namespace ppotepa.tokenez.StandardLib.String
{
    /// <summary>
    /// CONCAT function - concatenates strings together.
    /// Usage: CONCAT("Hello", " ", "World") returns "Hello World"
    /// </summary>
    public class ConcatFunction : StandardLibraryFunctionBase
    {
        public override string Name => "CONCAT";
        public override string Description => "Concatenates two or more strings together";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("str1", "STRING", "First string"),
            new Parameter("str2", "STRING", "Second string")
        };
        public override string ReturnType => "STRING";

        public override object Execute(params object[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException($"{Name} expects at least 2 arguments");

            return string.Concat(args.Select(a => a?.ToString() ?? ""));
        }
    }

    /// <summary>
    /// LENGTH function - returns the length of a string.
    /// Usage: LENGTH("Hello") returns 5
    /// </summary>
    public class LengthFunction : StandardLibraryFunctionBase
    {
        public override string Name => "LENGTH";
        public override string Description => "Returns the length of a string";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("str", "STRING", "The string to measure")
        };
        public override string ReturnType => "INT";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 1);
            var str = ConvertArgument<string>(args[0], 0);
            return str.Length;
        }
    }

    /// <summary>
    /// SUBSTRING function - extracts a portion of a string.
    /// Usage: SUBSTRING("Hello", 1, 3) returns "ell"
    /// </summary>
    public class SubstringFunction : StandardLibraryFunctionBase
    {
        public override string Name => "SUBSTRING";
        public override string Description => "Extracts a portion of a string";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("str", "STRING", "The source string"),
            new Parameter("start", "INT", "Starting index (0-based)"),
            new Parameter("length", "INT", "Number of characters to extract")
        };
        public override string ReturnType => "STRING";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 3);
            var str = ConvertArgument<string>(args[0], 0);
            var start = ConvertArgument<int>(args[1], 1);
            var length = ConvertArgument<int>(args[2], 2);

            if (start < 0 || start >= str.Length)
                throw new ArgumentOutOfRangeException(nameof(start), $"{Name}: Start index out of range");

            if (length < 0 || start + length > str.Length)
                throw new ArgumentOutOfRangeException(nameof(length), $"{Name}: Length out of range");

            return str.Substring(start, length);
        }
    }

    /// <summary>
    /// TOUPPER function - converts a string to uppercase.
    /// Usage: TOUPPER("hello") returns "HELLO"
    /// </summary>
    public class ToUpperFunction : StandardLibraryFunctionBase
    {
        public override string Name => "TOUPPER";
        public override string Description => "Converts a string to uppercase";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("str", "STRING", "The string to convert")
        };
        public override string ReturnType => "STRING";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 1);
            var str = ConvertArgument<string>(args[0], 0);
            return str.ToUpper();
        }
    }

    /// <summary>
    /// TOLOWER function - converts a string to lowercase.
    /// Usage: TOLOWER("HELLO") returns "hello"
    /// </summary>
    public class ToLowerFunction : StandardLibraryFunctionBase
    {
        public override string Name => "TOLOWER";
        public override string Description => "Converts a string to lowercase";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("str", "STRING", "The string to convert")
        };
        public override string ReturnType => "STRING";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 1);
            var str = ConvertArgument<string>(args[0], 0);
            return str.ToLower();
        }
    }

    /// <summary>
    /// TRIM function - removes leading and trailing whitespace.
    /// Usage: TRIM("  hello  ") returns "hello"
    /// </summary>
    public class TrimFunction : StandardLibraryFunctionBase
    {
        public override string Name => "TRIM";
        public override string Description => "Removes leading and trailing whitespace from a string";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("str", "STRING", "The string to trim")
        };
        public override string ReturnType => "STRING";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 1);
            var str = ConvertArgument<string>(args[0], 0);
            return str.Trim();
        }
    }
}
