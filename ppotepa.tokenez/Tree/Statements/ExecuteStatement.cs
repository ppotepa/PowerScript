namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    /// Represents an EXECUTE command to run an external PowerScript file.
    /// Syntax: EXECUTE "filename.ps"
    /// </summary>
    public class ExecuteStatement : Statement
    {
        /// <summary>
        /// Path to the script file to execute
        /// </summary>
        public string FilePath { get; }

        /// <summary>Statement type identifier</summary>
        public override string StatementType => "EXECUTE";

        public ExecuteStatement(string filePath)
        {
            FilePath = filePath;
        }

        public override string ToString()
        {
            return $"EXECUTE \"{FilePath}\"";
        }
    }
}
