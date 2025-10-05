using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            string currentPrompt = "FUNCTION EXAMPLE (INT a, INT b) RETURN a + b";

            while (!currentPrompt.Trim().Equals("quit", StringComparison.InvariantCultureIgnoreCase))
            {
                //currentPrompt = Console.ReadLine();

                if (string.IsNullOrEmpty(currentPrompt))
                {
                    continue;
                }

                UserPrompt prompt = new(currentPrompt);
                TokenTree tree = new TokenTree().Create(prompt);
            }
        }
    }
}
