namespace ppotepa.tokenez
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string currentPrompt = string.Empty;

            while (!currentPrompt.Trim().Equals("quit", StringComparison.InvariantCultureIgnoreCase))
            {
                currentPrompt = Console.ReadLine();

                if (string.IsNullOrEmpty(currentPrompt))
                {
                    continue;
                }

                var prompt = new UserPrompt(currentPrompt);
                Console.WriteLine(prompt.RawTokens);
            }
        }
    }
}
