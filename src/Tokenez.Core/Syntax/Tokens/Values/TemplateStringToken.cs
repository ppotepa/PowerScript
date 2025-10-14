using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Values
{
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
            var content = TemplateText.Trim('`');

            var currentText = "";
            var i = 0;

            while (i < content.Length)
                if (content[i] == '@' && i + 1 < content.Length)
                {
                    // Save any accumulated text
                    if (currentText.Length > 0)
                    {
                        Parts.Add(new TemplatePart { IsLiteral = true, Text = currentText });
                        currentText = "";
                    }

                    // Extract variable name (letters, digits, underscore)
                    i++; // Skip @
                    var varName = "";
                    while (i < content.Length && (char.IsLetterOrDigit(content[i]) || content[i] == '_'))
                    {
                        varName += content[i];
                        i++;
                    }

                    if (varName.Length > 0) Parts.Add(new TemplatePart { IsLiteral = false, Text = varName });
                }
                else
                {
                    currentText += content[i];
                    i++;
                }

            // Add any remaining text
            if (currentText.Length > 0) Parts.Add(new TemplatePart { IsLiteral = true, Text = currentText });
        }
    }
}