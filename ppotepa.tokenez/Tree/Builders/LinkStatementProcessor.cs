using ppotepa.tokenez.DotNet;
using ppotepa.tokenez.Localization;
using ppotepa.tokenez.Logging;
using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    ///     Processes LINK keyword tokens.
    ///     Responsible for:
    ///     - Parsing library/file import statements
    ///     - Validating LINK statements appear at script top
    ///     - Registering linked libraries in scope
    ///     - Supporting both .NET namespaces (System) and PowerScript files ("file.ps")
    ///     - Integrating with DotNetLinker for .NET namespace resolution
    /// </summary>
    internal class LinkStatementProcessor : ITokenProcessor
    {
        private readonly DotNetLinker _dotNetLinker;
        private readonly HashSet<string> _linkedFiles;
        private readonly ILogger _logger = LoggerService.Logger;

        public LinkStatementProcessor(DotNetLinker dotNetLinker)
        {
            _dotNetLinker = dotNetLinker;
            _linkedFiles = [];
        }

        public bool CanProcess(Token token)
        {
            return token is LinkKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            _logger.DebugLocalized(MessageKeys.Debug.LinkProcessor, token.RawToken?.Text ?? string.Empty, context.Depth);

            var linkToken = token as LinkKeywordToken;

            // LINK statements must appear at the top level (root scope)
            if (context.CurrentScope.ScopeName != "ROOT")
                throw new UnexpectedTokenException(
                    token,
                    "LINK statements must appear at the top of the script, before any functions or other statements"
                );

            // LINK must be followed by either an identifier (library name) or string literal (file path)
            var targetToken = linkToken!.Next;

            string linkTarget;
            var isWellKnownLibrary = false;

            if (targetToken is IdentifierToken identifierToken)
            {
                // Could be a simple library name or a namespace path (e.g., System.Collections.Generic)
                // Collect all parts separated by :: or .
                List<string> namespaceParts = [identifierToken.RawToken!.OriginalText];
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

                _logger.InfoLocalized(MessageKeys.Link.Namespace, linkTarget);
            }
            else if (targetToken is StringLiteralToken stringToken)
            {
                // File path (e.g., LINK "path/to/file.ps")
                linkTarget = stringToken.RawToken!.Text.Trim('"');
                isWellKnownLibrary = false;
                _logger.InfoLocalized(MessageKeys.Link.File, linkTarget);
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
            if (isWellKnownLibrary)
            {
                _logger.DebugLocalized(MessageKeys.Link.LibraryRegistered, linkTarget);

                // Link the namespace via DotNetLinker
                _dotNetLinker.LinkNamespace(linkTarget);
            }
            else
            {
                _logger.DebugLocalized(MessageKeys.Link.FileRegistered, linkTarget);

                // Load and parse the PowerScript file
                if (_linkedFiles.Contains(linkTarget))
                {
                    _logger.InfoLocalized(MessageKeys.Link.AlreadyLinked, linkTarget);
                    return;
                }

                _linkedFiles.Add(linkTarget);

                try
                {
                    // Resolve the file path (support relative paths)
                    var fullPath = ResolveFilePath(linkTarget);

                    if (!File.Exists(fullPath)) throw new FileNotFoundException($"Linked file not found: {fullPath}");

                    _logger.InfoLocalized(MessageKeys.Link.Loading, fullPath);

                    // Note: At this point in the build process, we can't directly inject code
                    // The file should be loaded BEFORE tokenization via PowerScriptInterpreter.LinkLibrary
                    // For now, we'll store it in the scope metadata for reference
                    // The actual file content merging should happen at the interpreter level

                    _logger.InfoLocalized(MessageKeys.Link.BestPractice);
                    _logger.InfoLocalized(MessageKeys.Link.Suggestion, linkTarget);
                }
                catch (Exception ex)
                {
                    _logger.ErrorLocalized(MessageKeys.Link.Error, linkTarget, ex.Message);
                    throw;
                }
            }
        }

        private string ResolveFilePath(string filePath)
        {
            // If the path is already absolute and exists, return it
            if (Path.IsPathRooted(filePath) && File.Exists(filePath)) return filePath;

            // Try relative to current directory
            var currentDirPath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            if (File.Exists(currentDirPath)) return currentDirPath;

            // Try relative to app base directory
            var appDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            if (File.Exists(appDirPath)) return appDirPath;

            // Return the original path (will fail if not found)
            return filePath;
        }
    }
}