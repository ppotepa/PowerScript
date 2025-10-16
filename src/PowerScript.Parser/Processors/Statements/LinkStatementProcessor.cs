using PowerScript.Common.Logging;
using PowerScript.Core.AST.Statements;
using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Keywords;
using PowerScript.Core.Syntax.Tokens.Operators;
using PowerScript.Core.Syntax.Tokens.Values;
using PowerScript.Parser.Processors.Base;

namespace PowerScript.Parser.Processors.Statements;

/// <summary>
///     Processes LINK statements for importing .NET namespaces or PowerScript files.
///     Syntax:
///       LINK System            (imports .NET namespace)
///       LINK "path/to/file.ps" (imports PowerScript file)
/// </summary>
public class LinkStatementProcessor : ITokenProcessor
{
    public bool CanProcess(Token token)
    {
        return token is LinkKeywordToken;
    }

    public TokenProcessingResult Process(Token token, ProcessingContext context)
    {
        LoggerService.Logger.Debug($"LinkStatementProcessor: Processing LINK token '{token.RawToken?.Text}' in scope '{context.CurrentScope.ScopeName}'");

        var linkToken = token;
        Token? nextToken = token.Next;

        if (nextToken == null)
        {
            throw new InvalidOperationException("Expected namespace or file path after LINK keyword");
        }

        string linkTarget;
        bool isFilePath = false;

        // Check if it's a file path (string literal) or namespace (identifier)
        if (nextToken is StringLiteralToken stringToken)
        {
            linkTarget = stringToken.RawToken?.OriginalText?.Trim('"') ?? "";
            isFilePath = true;
            nextToken = stringToken.Next;
        }
        else if (nextToken is IdentifierToken identifierToken)
        {
            // Could be: System, System.IO, System.Collections.Generic, etc.
            linkTarget = identifierToken.RawToken?.Text ?? "";

            // Check for dotted namespaces (System.IO, etc.)
            nextToken = identifierToken.Next;
            while (nextToken is DotToken)
            {
                nextToken = nextToken.Next;
                if (nextToken is IdentifierToken nextPart)
                {
                    linkTarget += "." + (nextPart.RawToken?.Text ?? "");
                    nextToken = nextPart.Next;
                }
                else
                {
                    throw new InvalidOperationException($"Expected identifier after '.' in LINK statement, found {nextToken?.GetType().Name}");
                }
            }
        }
        else
        {
            throw new InvalidOperationException($"Expected string literal or identifier after LINK keyword, found {nextToken.GetType().Name}");
        }

        // Create a LinkStatement
        var linkStatement = new LinkStatement
        {
            Target = linkTarget,
            IsFilePath = isFilePath,
            StartToken = linkToken
        };

        context.CurrentScope.Statements.Add(linkStatement);

        LoggerService.Logger.Info($"[OK] LinkStatementProcessor: Registered LINK {(isFilePath ? "file" : "namespace")} '{linkTarget}' in scope '{context.CurrentScope.ScopeName}'");

        return new TokenProcessingResult
        {
            NextToken = nextToken!
        };
    }
}
