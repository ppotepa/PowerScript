using ppotepa.tokenez;
using ppotepa.tokenez.Interpreter;

namespace CompilerDemo
{
    /// <summary>
    /// Demonstrates the PowerScript Compiler API
    /// Shows how to use the compiler without any CLI/REPL features
    /// </summary>
    class Demo
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== PowerScript Compiler API Demo ===\n");

            // Demo 1: Execute inline code
            Demo1_InlineCode();

            // Demo 2: Execute from file
            Demo2_FileExecution();

            // Demo 3: Persistent interpreter with multiple executions
            Demo3_PersistentInterpreter();

            Console.WriteLine("\n=== Demo Complete ===");
        }

        /// <summary>
        /// Demo 1: Simple inline code execution
        /// </summary>
        static void Demo1_InlineCode()
        {
            Console.WriteLine("\n--- Demo 1: Inline Code Execution ---");

            var code = @"
                NUMBER x = 10
                NUMBER y = 20
                NUMBER sum = x + y
                PRINT sum
            ";

            try
            {
                Console.WriteLine("Executing code:");
                Console.WriteLine(code);
                Console.WriteLine("\nResult:");

                Program.CompileAndExecute(code);

                Console.WriteLine("✓ Execution completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Demo 2: Execute code from a file
        /// </summary>
        static void Demo2_FileExecution()
        {
            Console.WriteLine("\n--- Demo 2: File Execution ---");

            // Create a test script file
            var scriptPath = "demo_script.ps";
            var scriptContent = @"
NUMBER a = 5
NUMBER b = 3
PRINT a * b
";

            try
            {
                // Write the script to a file
                File.WriteAllText(scriptPath, scriptContent);
                Console.WriteLine($"Created script file: {scriptPath}");
                Console.WriteLine($"Content:\n{scriptContent}");
                Console.WriteLine("\nResult:");

                // Execute it
                Program.CompileAndExecuteFile(scriptPath);

                Console.WriteLine("✓ File execution completed successfully");

                // Cleanup
                File.Delete(scriptPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Demo 3: Using a persistent interpreter for multiple executions
        /// </summary>
        static void Demo3_PersistentInterpreter()
        {
            Console.WriteLine("\n--- Demo 3: Persistent Interpreter ---");

            try
            {
                // Create an interpreter instance
                var interpreter = PowerScriptInterpreter.CreateNew();
                Console.WriteLine("Created persistent interpreter instance\n");

                // Execute multiple statements - variables persist
                Console.WriteLine("Step 1: Declare variable");
                interpreter.ExecuteCode("NUMBER counter = 0");

                Console.WriteLine("\nStep 2: Increment counter");
                interpreter.ExecuteCode("counter = counter + 1");

                Console.WriteLine("\nStep 3: Increment again");
                interpreter.ExecuteCode("counter = counter + 1");

                Console.WriteLine("\nStep 4: Print counter value");
                interpreter.ExecuteCode("PRINT counter");

                Console.WriteLine("\n✓ Persistent state demo completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }
    }
}
