using ppotepa.tokenez.DotNet;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Values;
using ppotepa.tokenez.Tree.Builders.Interfaces;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Processes LINK keyword tokens.
    /// Responsible for:
    /// - Parsing library/file import statements
    /// - Validating LINK statements appear at script top
    /// - Registering linked libraries in scope
    /// - Supporting both .NET namespaces (System) and PowerScript files ("file.ps")
    /// - Integrating with DotNetLinker for .NET namespace resolution
    /// </summary>
    internal class LinkStatementProcessor : ITokenProcessor
    {
        private readonly DotNetLinker _dotNetLinker;
        private readonly HashSet<string> _linkedFiles;

        public LinkStatementProcessor(DotNetLinker dotNetLinker)
        {
            _dotNetLinker = dotNetLinker;
            _linkedFiles = new HashSet<string>();
        }

        public bool CanProcess(Token token)
        {
            return token is LinkKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] LinkStatementProcessor: Processing LINK token '{token.RawToken?.Text}' at depth {context.Depth}");
            Console.ResetColor();

            var linkToken = token as LinkKeywordToken;

            // LINK statements must appear at the top level (root scope)
            if (context.CurrentScope.ScopeName != "ROOT")
            {
                throw new UnexpectedTokenException(
                    token,
                    "LINK statements must appear at the top of the script, before any functions or other statements"
                );
            }

            // LINK must be followed by either an identifier (library name) or string literal (file path)
            var targetToken = linkToken!.Next;

            string linkTarget;
            bool isWellKnownLibrary = false;
            bool isNamespacePath = false;

            if (targetToken is IdentifierToken identifierToken)
            {
                // Could be a simple library name or a namespace path (e.g., System.Collections.Generic)
                // Collect all parts separated by :: or .
                List<string> namespaceParts = new List<string> { identifierToken.RawToken!.OriginalText };
                var currentToken = identifierToken.Next;
                Token lastProcessedToken = identifierToken;

                // Check for namespace paths with :: (System::Collections::Generic)
                while (currentToken is NamespaceOperatorToken)
                {
                    currentToken = currentToken.Next;
                    if (currentToken is IdentifierToken nextPart)
                    {
                        namespaceParts.Add(nextPart.RawToken!.OriginalText);
                        lastProcessedToken = nextPart;
                        currentToken = nextPart.Next;
                    }
                    else
                    {
                        break;
                    }
                }

                // Check for namespace paths with dots (System.Collections.Generic)
                while (currentToken is DotToken)
                {
                    currentToken = currentToken.Next;
                    if (currentToken is IdentifierToken nextPart)
                    {
                        namespaceParts.Add(nextPart.RawToken!.OriginalText);
                        lastProcessedToken = nextPart;
                        currentToken = nextPart.Next;
                    }
                    else
                    {
                        break;
                    }
                }

                linkTarget = string.Join(".", namespaceParts);
                isWellKnownLibrary = true;
                targetToken = lastProcessedToken;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[LINK] Linking .NET namespace: {linkTarget}");
                Console.ResetColor();
            }
            else if (targetToken is StringLiteralToken stringToken)
            {
                // File path (e.g., LINK "path/to/file.ps")
                linkTarget = stringToken.RawToken!.Text.Trim('"');
                isWellKnownLibrary = false;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[LINK] Linking file: {linkTarget}");
                Console.ResetColor();
            }
            else
            {
                throw new UnexpectedTokenException(
                    targetToken,
                    "LINK must be followed by a library name (identifier) or file path (string literal)",
                    typeof(IdentifierToken),
                    typeof(StringLiteralToken)
                );
            }

            // Register the link
            RegisterLink(context.CurrentScope, linkTarget, isWellKnownLibrary);

            // Continue processing from the token after the link target
            return TokenProcessingResult.Continue(targetToken!.Next);
        }

        private void RegisterLink(Scope scope, string linkTarget, bool isWellKnownLibrary)
        {
            // Create a simple record of the linked library/file
            Console.ForegroundColor = ConsoleColor.Blue;
            if (isWellKnownLibrary)
            {
                Console.WriteLine($"[DEBUG] Registered well-known library link: {linkTarget}");

                // Link the namespace via DotNetLinker
                _dotNetLinker.LinkNamespace(linkTarget);
            }
            else
            {
                Console.WriteLine($"[DEBUG] Registered file link: {linkTarget}");

                // Load and parse the PowerScript file
                if (_linkedFiles.Contains(linkTarget))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[LINK] File already linked, skipping: {linkTarget}");
                    Console.ResetColor();
                    return;
                }

                _linkedFiles.Add(linkTarget);

                try
                {
                    // Resolve the file path (support relative paths)
                    string fullPath = ResolveFilePath(linkTarget);

                    if (!File.Exists(fullPath))
                    {
                        throw new FileNotFoundException($"Linked file not found: {fullPath}");
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[LINK] Loading PowerScript file: {fullPath}");
                    Console.ResetColor();

                    // Note: At this point in the build process, we can't directly inject code
                    // The file should be loaded BEFORE tokenization via PowerScriptInterpreter.LinkLibrary
                    // For now, we'll store it in the scope metadata for reference
                    // The actual file content merging should happen at the interpreter level

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[LINK] Note: PowerScript file linking is best done via PowerScriptInterpreter.LinkLibrary()");
                    Console.WriteLine($"[LINK] Add this to your code before execution: interpreter.LinkLibrary(\"{linkTarget}\")");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[LINK] Error linking file '{linkTarget}': {ex.Message}");
                    Console.ResetColor();
                    throw;
                }
            }
            Console.ResetColor();
        }

        private string ResolveFilePath(string filePath)
        {
            // If the path is already absolute and exists, return it
            if (Path.IsPathRooted(filePath) && File.Exists(filePath))
            {
                return filePath;
            }

            // Try relative to current directory
            string currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            if (File.Exists(currentDirPath))
            {
                return currentDirPath;
            }

            // Try relative to app base directory
            string appDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            if (File.Exists(appDirPath))
            {
                return appDirPath;
            }

            // Return the original path (will fail if not found)
            return filePath;
        }
    }
}