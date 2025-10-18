using PowerScript.Common.Logging;
using PowerScript.Core.Syntax;
using PowerScript.Parser.Validators;

namespace PowerScript.Parser.Extensions;

/// <summary>
/// Provides a safe sandbox for registering custom syntax patterns with validation.
/// </summary>
public class PatternExtensionSandbox
{
    private readonly CustomSyntaxRegistry _registry;
    private readonly List<string> _registrationLog = [];

    public PatternExtensionSandbox()
    {
        _registry = CustomSyntaxRegistry.Instance;
    }

    /// <summary>
    /// Safely registers a pattern with comprehensive validation.
    /// </summary>
    public bool RegisterPattern(string pattern, string transformationText, bool throwOnError = false)
    {
        try
        {
            LoggerService.Logger.Debug($"[PatternSandbox] Attempting to register: {pattern} => {transformationText}");

            // Step 1: Validate pattern syntax
            var validationErrors = PatternValidator.ValidatePattern(pattern, transformationText);
            if (validationErrors.Count > 0)
            {
                var errorMessage = $"Pattern validation failed:\n  - {string.Join("\n  - ", validationErrors)}";
                LoggerService.Logger.Error($"[PatternSandbox] {errorMessage}");

                if (throwOnError)
                {
                    throw new PatternValidationException(errorMessage, validationErrors);
                }

                return false;
            }

            // Step 2: Check for conflicts with existing patterns
            var existingPatterns = _registry.GetPatternTransformations();
            var conflicts = PatternValidator.CheckForConflicts(pattern, existingPatterns);
            if (conflicts.Count > 0)
            {
                var warningMessage = $"Pattern may conflict:\n  - {string.Join("\n  - ", conflicts)}";
                LoggerService.Logger.Warning($"[PatternSandbox] {warningMessage}");

                // Conflicts are warnings, not errors, unless throwOnError is set
                if (throwOnError)
                {
                    throw new PatternConflictException(warningMessage, conflicts);
                }
            }

            // Step 3: Validate type constraints
            if (!PatternValidator.ValidateTypeConstraints(pattern))
            {
                var errorMessage = "Pattern contains invalid type constraints";
                LoggerService.Logger.Error($"[PatternSandbox] {errorMessage}");

                if (throwOnError)
                {
                    throw new PatternValidationException(errorMessage, new List<string> { errorMessage });
                }

                return false;
            }

            // Step 4: Register the pattern
            var transformation = new SyntaxTransformation
            {
                Pattern = pattern,
                Transformation = transformationText
            };
            _registry.Register(transformation);

            _registrationLog.Add($"{DateTime.Now:HH:mm:ss} - Registered: {pattern}");
            LoggerService.Logger.Info($"[PatternSandbox] Successfully registered pattern: {pattern}");

            return true;
        }
        catch (PatternValidationException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (PatternConflictException)
        {
            throw; // Re-throw conflict exceptions
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error registering pattern '{pattern}': {ex.Message}";
            LoggerService.Logger.Error($"[PatternSandbox] {errorMessage}");

            if (throwOnError)
            {
                throw new PatternRegistrationException(errorMessage, ex);
            }

            return false;
        }
    }

    /// <summary>
    /// Registers multiple patterns from a file with batch validation.
    /// </summary>
    public PatternBatchResult RegisterPatternsFromFile(string filePath)
    {
        var result = new PatternBatchResult();

        if (!File.Exists(filePath))
        {
            result.Errors.Add($"File not found: {filePath}");
            return result;
        }

        try
        {
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//"))
                {
                    continue; // Skip empty lines and comments
                }

                if (line.Contains("=>"))
                {
                    var parts = line.Split("=>", 2, StringSplitOptions.TrimEntries);
                    if (parts.Length == 2)
                    {
                        var pattern = parts[0].Trim();
                        var transformation = parts[1].Trim();

                        if (RegisterPattern(pattern, transformation, throwOnError: false))
                        {
                            result.SuccessCount++;
                        }
                        else
                        {
                            result.FailureCount++;
                            result.Errors.Add($"Failed to register: {pattern}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading file: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Gets the registration log for auditing.
    /// </summary>
    public IReadOnlyList<string> GetRegistrationLog() => _registrationLog.AsReadOnly();

    /// <summary>
    /// Clears the registration log.
    /// </summary>
    public void ClearLog() => _registrationLog.Clear();
}

/// <summary>
/// Result of batch pattern registration.
/// </summary>
public class PatternBatchResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = [];

    public bool IsSuccess => FailureCount == 0 && Errors.Count == 0;
}

/// <summary>
/// Exception thrown when pattern validation fails.
/// </summary>
public class PatternValidationException : Exception
{
    public List<string> ValidationErrors { get; }

    public PatternValidationException(string message, List<string> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors;
    }
}

/// <summary>
/// Exception thrown when pattern conflicts are detected.
/// </summary>
public class PatternConflictException : Exception
{
    public List<string> Conflicts { get; }

    public PatternConflictException(string message, List<string> conflicts)
        : base(message)
    {
        Conflicts = conflicts;
    }
}

/// <summary>
/// Exception thrown during pattern registration.
/// </summary>
public class PatternRegistrationException : Exception
{
    public PatternRegistrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
