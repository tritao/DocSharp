using DocSharp.Generators;

namespace DocSharp
{
    public class Options
    {
        public Options()
        {
            Project = new Project();
        }

        public Project Project;

        // General options
        public bool Verbose;
        public bool ShowHelpText;
        public bool OutputDebug;

        // Parser options
        public bool IgnoreParseErrors;

        // Generator options
        public GeneratorKind Generator;
        public string OutputNamespace;
        public string OutputDir;
    }
}
