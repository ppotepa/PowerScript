using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez
{
    /// <summary>
    /// Main entry point for the tokenizer application.
    /// Demonstrates the function-based language where all code is automatically wrapped in a MAIN function.
    /// </summary>
    internal static class Program
    {
        static void Main(string[] args)
        {
            // Example user code - demonstrates a simple function with parameters and return statement
            string userCode = "FUNCTION EXAMPLE ( INT a , INT b ) { RETURN a + b }";

            Console.WriteLine("=== Function-Based Language ===\n");
            Console.WriteLine($"User Code:\n{userCode}\n");

            // Main processing loop - can be extended to accept user input
            while (!userCode.Trim().Equals("quit", StringComparison.InvariantCultureIgnoreCase))
            {
                // Uncomment to enable interactive mode
                //userCode = Console.ReadLine();

                // Skip empty input
                if (string.IsNullOrEmpty(userCode))
                {
                    continue;
                }

                // Create prompt and automatically wrap user code in MAIN function
                // UserPrompt automatically wraps code in MAIN function with params
                UserPrompt prompt = new(userCode, args);
                Console.WriteLine($"Wrapped in MAIN:\n{prompt.WrappedPrompt}\n");

                // Build and process the token tree
                TokenTree tree = new TokenTree().Create(prompt);
                Console.WriteLine("\n=== Execution Complete ===");
                Console.WriteLine("Tokens processed successfully!");
                Console.ReadLine();
                break;
            }
        }
    }
}
