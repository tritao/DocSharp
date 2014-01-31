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
                    Verbose = true
                };

            var project = options.Project;

            const string rootDir = @"C:\Development\CppSharp";
            options.OutputDir = rootDir + @"\build\vs2012\docs";

            project.AssemblyDirs.Add(rootDir + @"\build\vs2012\lib\Release_x32");
            project.Assemblies.Add("CppSharp*");

            project.DocumentDirs.Add(rootDir + @"\docs");
            project.Documents.Add(@".*\.md");

            project.SourceDirs.Add(rootDir + @"\src");

            var driver = new Driver(options);
            driver.Run();
        }
    }
}