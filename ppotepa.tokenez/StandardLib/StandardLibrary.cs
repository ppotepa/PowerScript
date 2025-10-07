using ppotepa.tokenez.StandardLib.IO;
using ppotepa.tokenez.StandardLib.Math;
using ppotepa.tokenez.StandardLib.String;

namespace ppotepa.tokenez.StandardLib
{
    /// <summary>
    /// Registry of all standard library functions available in PowerScript.
    /// This serves as the "standard library" that's pre-loaded in the root scope.
    /// </summary>
    public class StandardLibrary
    {
        private readonly Dictionary<string, IStandardLibraryFunction> _functions;

        public StandardLibrary()
        {
            _functions = new Dictionary<string, IStandardLibraryFunction>(StringComparer.OrdinalIgnoreCase);
            LoadStandardFunctions();
        }

        /// <summary>
        /// Gets all registered standard library functions.
        /// </summary>
        public IEnumerable<IStandardLibraryFunction> Functions => _functions.Values;

        /// <summary>
        /// Gets a function by name.
        /// </summary>
        public IStandardLibraryFunction GetFunction(string name)
        {
            return _functions.TryGetValue(name, out var function) ? function : null;
        }

        /// <summary>
        /// Checks if a function exists in the standard library.
        /// </summary>
        public bool HasFunction(string name)
        {
            return _functions.ContainsKey(name);
        }

        /// <summary>
        /// Loads all built-in functions into the registry.
        /// </summary>
        private void LoadStandardFunctions()
        {
            // I/O Functions
            Register(new PrintFunction());
            Register(new ReadFunction());
            Register(new PrintLineFunction());

            // Math Functions
            Register(new AddFunction());
            Register(new SubtractFunction());
            Register(new MultiplyFunction());
            Register(new DivideFunction());
            Register(new ModuloFunction());
            Register(new PowerFunction());

            // String Functions
            Register(new ConcatFunction());
            Register(new LengthFunction());
            Register(new SubstringFunction());
            Register(new ToUpperFunction());
            Register(new ToLowerFunction());
            Register(new TrimFunction());
        }

        /// <summary>
        /// Registers a function in the standard library.
        /// </summary>
        private void Register(IStandardLibraryFunction function)
        {
            _functions[function.Name] = function;
        }

        /// <summary>
        /// Displays all available standard library functions.
        /// </summary>
        public void DisplayFunctions()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              POWERSCRIPT STANDARD LIBRARY                      ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.ResetColor();

            var categories = new[]
            {
                ("I/O Functions", _functions.Values.Where(f => f.GetType().Namespace.Contains("IO"))),
                ("Math Functions", _functions.Values.Where(f => f.GetType().Namespace.Contains("Math"))),
                ("String Functions", _functions.Values.Where(f => f.GetType().Namespace.Contains("String")))
            };

            foreach (var (category, funcs) in categories)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"┌─ {category}");
                Console.ResetColor();

                foreach (var func in funcs.OrderBy(f => f.Name))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"│  • {func.Name}");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    if (func.Parameters.Length > 0)
                    {
                        var paramList = string.Join(", ", func.Parameters.Select(p => $"{p.Type} {p.Name}"));
                        Console.Write($"({paramList})");
                    }
                    else
                    {
                        Console.Write("()");
                    }

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($" → {func.ReturnType}");
                    Console.ResetColor();
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"│    {func.Description}");
                    Console.ResetColor();
                }
                Console.WriteLine("│");
            }
        }
    }
}
