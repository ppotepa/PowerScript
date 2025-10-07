namespace ppotepa.tokenez.StandardLib.Math
{
    /// <summary>
    /// ADD function - adds two numbers.
    /// Usage: ADD(5, 3) returns 8
    /// </summary>
    public class AddFunction : StandardLibraryFunctionBase
    {
        public override string Name => "ADD";
        public override string Description => "Adds two numbers together";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("a", "INT", "First number"),
            new Parameter("b", "INT", "Second number")
        };
        public override string ReturnType => "INT";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 2);
            var a = ConvertArgument<int>(args[0], 0);
            var b = ConvertArgument<int>(args[1], 1);
            return a + b;
        }
    }

    /// <summary>
    /// SUBTRACT function - subtracts two numbers.
    /// Usage: SUBTRACT(10, 3) returns 7
    /// </summary>
    public class SubtractFunction : StandardLibraryFunctionBase
    {
        public override string Name => "SUBTRACT";
        public override string Description => "Subtracts the second number from the first";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("a", "INT", "Number to subtract from"),
            new Parameter("b", "INT", "Number to subtract")
        };
        public override string ReturnType => "INT";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 2);
            var a = ConvertArgument<int>(args[0], 0);
            var b = ConvertArgument<int>(args[1], 1);
            return a - b;
        }
    }

    /// <summary>
    /// MULTIPLY function - multiplies two numbers.
    /// Usage: MULTIPLY(5, 3) returns 15
    /// </summary>
    public class MultiplyFunction : StandardLibraryFunctionBase
    {
        public override string Name => "MULTIPLY";
        public override string Description => "Multiplies two numbers together";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("a", "INT", "First number"),
            new Parameter("b", "INT", "Second number")
        };
        public override string ReturnType => "INT";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 2);
            var a = ConvertArgument<int>(args[0], 0);
            var b = ConvertArgument<int>(args[1], 1);
            return a * b;
        }
    }

    /// <summary>
    /// DIVIDE function - divides two numbers.
    /// Usage: DIVIDE(10, 2) returns 5
    /// </summary>
    public class DivideFunction : StandardLibraryFunctionBase
    {
        public override string Name => "DIVIDE";
        public override string Description => "Divides the first number by the second";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("a", "INT", "Dividend"),
            new Parameter("b", "INT", "Divisor")
        };
        public override string ReturnType => "INT";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 2);
            var a = ConvertArgument<int>(args[0], 0);
            var b = ConvertArgument<int>(args[1], 1);

            if (b == 0)
                throw new DivideByZeroException($"{Name}: Cannot divide by zero");

            return a / b;
        }
    }

    /// <summary>
    /// MOD function - returns the remainder of division.
    /// Usage: MOD(10, 3) returns 1
    /// </summary>
    public class ModuloFunction : StandardLibraryFunctionBase
    {
        public override string Name => "MOD";
        public override string Description => "Returns the remainder after division";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("a", "INT", "Dividend"),
            new Parameter("b", "INT", "Divisor")
        };
        public override string ReturnType => "INT";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 2);
            var a = ConvertArgument<int>(args[0], 0);
            var b = ConvertArgument<int>(args[1], 1);

            if (b == 0)
                throw new DivideByZeroException($"{Name}: Cannot divide by zero");

            return a % b;
        }
    }

    /// <summary>
    /// POW function - raises a number to a power.
    /// Usage: POW(2, 3) returns 8
    /// </summary>
    public class PowerFunction : StandardLibraryFunctionBase
    {
        public override string Name => "POW";
        public override string Description => "Raises a number to a power";
        public override Parameter[] Parameters => new[]
        {
            new Parameter("base", "INT", "The base number"),
            new Parameter("exponent", "INT", "The exponent")
        };
        public override string ReturnType => "INT";

        public override object Execute(params object[] args)
        {
            ValidateArgumentCount(args, 2);
            var baseNum = ConvertArgument<double>(args[0], 0);
            var exponent = ConvertArgument<double>(args[1], 1);
            return (int)System.Math.Pow(baseNum, exponent);
        }
    }
}
