using DocSharp;
using DocSharp.Generators;

namespace CppSharp
{
    public class DocGen
    {
        static void Main(string[] args)
        {
            var options = new Options
            {
                Generator = GeneratorKind.HTML,
                Verbose = false,
                GenerateBareHTML = true
            };

            var project = options.Project;

            const string rootDir = @"C:\Development\flood2\";
            options.OutputDir = rootDir + @"\build\vs2013\docs";

            project.AssemblyDirs.Add(rootDir + @"\build\vs2013\lib\Debug_x32");
            project.Assemblies.Add("EngineBindings.dll");
            project.Assemblies.Add("EngineManaged.dll");

            project.DocumentDirs.Add(rootDir + @"\docs");
            project.Documents.Add(@".*\.md");

            project.SourceDirs.Add(rootDir + @"\src");

            var driver = new Driver(options);
            driver.Run();
        }
    }
}