using System.Collections.Generic;
using System.IO;
using DocSharp.Documents;
using DocSharp.Generators;
using DocSharp.Generators.HTML;
using DocSharp.Templates;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp
{
    public class Driver
    {
        public Options Options { get; private set; }
        public IDiagnostics Diagnostics { get; private set; }

        public List<IDocument> Documents { get; private set; }
        public List<CSharpSourceFile> Sources { get; private set; }
        public List<IUnresolvedAssembly> Assemblies { get; private set; }
        public ICompilation Compilation { get; private set; }

        public ProjectOutput Output { get; private set; }

        public Driver(Options options, IDiagnostics diagnostics = null)
        {
            Options = options;
            Diagnostics = diagnostics;

            Setup();

            Sources = new List<CSharpSourceFile>();
            Assemblies = new List<IUnresolvedAssembly>();
            Documents = new List<IDocument>();
        }

        public void Setup()
        {
            if (Diagnostics == null)
                Diagnostics = new TextDiagnosticPrinter();

            if (Options.OutputDir == null)
                Options.OutputDir = Directory.GetCurrentDirectory();
        }

        public bool Parse()
        {
            var parser = new Parser(Options, Diagnostics);
            parser.OnSourceParsed += HandleSourceParsed;
            parser.OnAssemblyParsed += HandleAssemblyParsed;
            parser.OnDocumentParsed += HandleDocumentParsed;

            return parser.Parse(Options.Project);
        }

        public void Process()
        {
            PrepareCSharpSourceFiles();
        }

        void PrepareCSharpSourceFiles()
        {
            IProjectContent project = new CSharpProjectContent();

            foreach (var assembly in Assemblies)
                project = project.AddAssemblyReferences(assembly);

            foreach (var source in Sources)
            {
                source.UnresolvedFile = source.SyntaxTree.ToTypeSystem();
                project = project.AddOrUpdateFiles(source.UnresolvedFile);
            }

            Compilation = project.CreateCompilation();

            foreach (var source in Sources)
            {
                source.Resolver = new CSharpAstResolver(Compilation,
                    source.SyntaxTree, source.UnresolvedFile);
            }
        }

        public void Generate()
        {
            if (!Directory.Exists(Options.OutputDir))
                Directory.CreateDirectory(Options.OutputDir);

            Output = new ProjectOutput();

            // Use the Bootstrap-based template for now.
            var template = new BootstrapTemplate();
            template.Process(Options);

            // Generate C# documentation.
            var csharpGen = new HTMLCSharpGenerator(this, template);
            csharpGen.GenerateReference(Compilation);

            // Generate Markdown documentation.
            var markdownGen = new HTMLMarkdownGenerator(this, template);
            foreach (var document in Documents)
                markdownGen.GenerateDocument(document);

            foreach (var output in Output.Files)
            {
                var path = output.Key;

                var outputPath = Path.Combine(Options.OutputDir,
                    Path.GetDirectoryName(path));

                // Make sure the target directory exists.
                Directory.CreateDirectory(outputPath);

                var fullPath = Path.Combine(outputPath, Path.GetFileName(path));

                var outputStream = output.Value;
                outputStream.Position = 0;

                using (var outputFile = File.Create(fullPath))
                    outputStream.CopyTo(outputFile);

                Diagnostics.Message("Generated: {0}", path);
            }
        }

        public void Run()
        {
            Options.Project.BuildInputs();

            Diagnostics.Message("Parsing assemblies...");
            if (!Parse())
                return;

            Diagnostics.Message("Processing assemblies...");
            Process();

            Diagnostics.Message("Generating documentation...");
            Generate();
        }

        void HandleParserResult<T>(ParserResult<T> result)
        {
            var file = result.Input.FullPath;
            if (file.StartsWith(result.Input.BasePath))
            {
                file = file.Substring(result.Input.BasePath.Length);
                file = file.TrimStart('\\');
            }

            switch (result.Kind)
            {
                case ParserResultKind.Success:
                    Diagnostics.Message("Parsed '{0}'", file);
                    break;
                case ParserResultKind.Error:
                    Diagnostics.Message("Error parsing '{0}'", file);
                    break;
                case ParserResultKind.FileNotFound:
                    Diagnostics.Message("File '{0}' was not found", file);
                    break;
            }

            foreach (var diag in result.Diagnostics)
            {
                Diagnostics.Message(string.Format("{0}({1},{2}): {3}: {4}",
                    diag.FileName, diag.LineNumber, diag.ColumnNumber,
                    diag.Level.ToString().ToLower(), diag.Message));
            }
        }

        void HandleSourceParsed(ParserResult<SyntaxTree> result)
        {
            HandleParserResult(result);

            if (result.Kind != ParserResultKind.Success)
                return;

            Sources.Add(new CSharpSourceFile { Input = result.Input, 
                SyntaxTree = result.Output });
        }

        void HandleAssemblyParsed(ParserResult<IUnresolvedAssembly> result)
        {
            HandleParserResult(result);

            if (result.Kind != ParserResultKind.Success)
                return;

            Assemblies.Add(result.Output);
        }

        void HandleDocumentParsed(ParserResult<IDocument> result)
        {
            HandleParserResult(result);

            if (result.Kind != ParserResultKind.Success)
                return;

            Documents.Add(result.Output);
        }
    }
}