using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Values;

/// <summary>
///     Represents a template string literal with variable interpolation.
///     Syntax: `Hello @name, you are @age years old`
///     The @ symbol indicates a variable to be interpolated.
/// </summary>
public class TemplateStringToken : Token
{
    public TemplateStringToken()
    {
        TemplateText = "";
    }

    public TemplateStringToken(RawToken rawToken) : base(rawToken)
    {
        TemplateText = rawToken.Text;
        ParseTemplate();
    }

    /// <summary>
    ///     The raw template string including backticks and @ markers
    /// </summary>
    public string TemplateText { get; set; }

    /// <summary>
    ///     List of text segments and variable names extracted from the template
    /// </summary>
    public List<TemplatePart> Parts { get; set; } = [];

    public override Type[] Expectations => new Type[] { };

    /// <summary>
    ///     Parses the template string to extract literal parts and variable names.
    ///     Example: `Hello @name` -> ["Hello ", VariableRef("name")]
    /// </summary>
    private void ParseTemplate()
    {
        // Remove backticks
        string content = TemplateText.Trim('`');

        string currentText = "";
        int i = 0;

        while (i < content.Length)
        {
            if (content[i] == '@' && i + 1 < content.Length)
            {
                // Save any accumulated text
                if (currentText.Length > 0)
                {
                    Parts.Add(new TemplatePart { IsLiteral = true, Text = currentText });
                    currentText = "";
                }

                i++; // Skip @

                // Check if variable is in braces: @{varName}
                bool hasBraces = false;
                if (i < content.Length && content[i] == '{')
                {
                    hasBraces = true;
                    i++; // Skip {
                }

                // Extract variable name (letters, digits, underscore)
                string varName = "";
                while (i < content.Length)
                {
                    if (hasBraces && content[i] == '}')
                    {
                        i++; // Skip }
                        break;
                    }
                    if (!hasBraces && !char.IsLetterOrDigit(content[i]) && content[i] != '_')
                    {
                        break;
                    }
                    varName += content[i];
                    i++;
                }

                if (varName.Length > 0)
                {
                    Parts.Add(new TemplatePart { IsLiteral = false, Text = varName });
                }
            }
            else
            {
                currentText += content[i];
                i++;
            }
        }

        // Add any remaining text
        if (currentText.Length > 0)
        {
            Parts.Add(new TemplatePart { IsLiteral = true, Text = currentText });
        }
    }
}