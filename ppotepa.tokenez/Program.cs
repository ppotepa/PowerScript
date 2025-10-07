using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.StandardLib;
using ppotepa.tokenez.Tree;

namespace ppotepa.tokenez
{
    /// <summary>
    /// Main entry point for PowerScript.
    /// 1. Reads program.ps
    /// 2. Builds token tree
    /// 3. Compiles functions
    /// 4. Executes compiled code
    /// </summary>
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                      POWERSCRIPT v1.0                          ║");
            Console.WriteLine("║          A Function-Based Language with Standard Library       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

            // Display the standard library
            var standardLibrary = new StandardLibrary();
            standardLibrary.DisplayFunctions();

            // Step 1: Read program.ps
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║        READING PROGRAM.PS              ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");

            string programFile = "program.ps";
            if (!File.Exists(programFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Error: {programFile} not found!");
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadLine();
                return;
            }

            string code = File.ReadAllText(programFile);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Read {programFile}");
            Console.ResetColor();
            Console.WriteLine($"\nCode:\n{code}\n");

            try
            {
                // Step 2: Build token tree
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║        BUILDING TOKEN TREE             ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");

                UserPrompt prompt = new(code, args);
                TokenTree tree = new TokenTree().Create(prompt);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Token tree built successfully!");
                Console.ResetColor();

                tree.Visualize();

                // Step 3 & 4: Compile and execute
                PowerScriptCompiler compiler = new(tree);
                compiler.CompileAndExecute();

                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║        EXECUTION COMPLETE              ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ Error: {ex.Message}");
                Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadLine();
        }
    }
}
