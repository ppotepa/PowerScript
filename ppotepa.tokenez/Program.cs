using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            // User just writes their functions - no need to write MAIN
            string userCode = "FUNCTION EXAMPLE ( INT a , INT b ) { RETURN a + b }";

            Console.WriteLine("=== Function-Based Language ===\n");
            Console.WriteLine($"User Code:\n{userCode}\n");

            while (!userCode.Trim().Equals("quit", StringComparison.InvariantCultureIgnoreCase))
            {
                //userCode = Console.ReadLine();

                if (string.IsNullOrEmpty(userCode))
                {
                    continue;
                }

                // UserPrompt automatically wraps code in MAIN function with params
                UserPrompt prompt = new(userCode, args);
                Console.WriteLine($"Wrapped in MAIN:\n{prompt.WrappedPrompt}\n");

                TokenTree tree = new TokenTree().Create(prompt);
                Console.WriteLine("\n=== Execution Complete ===");
                Console.WriteLine("Tokens processed successfully!");
                Console.ReadLine();
                break;
            }
        }
    }
}
