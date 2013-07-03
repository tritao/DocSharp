using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DocSharp.Generators;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp
{
    public class Driver
    {
        public DriverOptions Options { get; private set; }
        public IDiagnosticConsumer Diagnostics { get; private set; }
        public Parser Parser { get; private set; }

        public List<SyntaxTree> Sources { get; private set; }
        public List<IUnresolvedAssembly> Assemblies { get; private set; }
        public List<IDocument> Documents { get; private set; }

        public ICompilation Compilation { get; private set; }

        public Driver(DriverOptions options, IDiagnosticConsumer diagnostics)
        {
            Options = options;
            Diagnostics = diagnostics;

            Parser = new Parser(this);
            Parser.OnSourceParsed += HandleSourceParsed;
            Parser.OnAssemblyParsed += HandleAssemblyParsed;
            Parser.OnDocumentParsed += HandleDocumentParsed;

            Sources = new List<SyntaxTree>();
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

        #region Parsing
        ParserResult<T> ParseInput<T>(ProjectInput input, ParserHandler<T> handler)
        {
            var result = new ParserResult<T>
            {
                Input = input
            };

            if (!File.Exists(input.FullPath))
            {
                result.Kind = ParserResultKind.FileNotFound;
                return result;
            }

            input.Stream = File.OpenRead(input.FullPath);

            handler(input, result);
            return result;
        }

        public bool Parse()
        {
            var hasErrors = false;

            foreach (var project in Options.Projects)
            {
                foreach (var input in project.SourceInputs)
                {
                    var result = ParseInput<SyntaxTree>(input, Parser.ParseSource);
                    hasErrors |= result.HasErrors;
                }

                foreach (var input in project.AssemblyInputs)
                {
                    var result = ParseInput<IUnresolvedAssembly>(input, Parser.ParseAssembly);
                    hasErrors |= result.HasErrors;
                }

                foreach (var input in project.DocumentInputs)
                {
                    var result = ParseInput<IDocument>(input, Parser.ParseDocument);
                    hasErrors |= result.HasErrors;
                }
            }

            return !hasErrors;
        }

        void HandleSourceParsed(ParserResult<SyntaxTree> result)
        {
            if (result.Kind != ParserResultKind.Success)
                return;

            Sources.Add(result.Output);
        }

        void HandleAssemblyParsed(ParserResult<IUnresolvedAssembly> result)
        {
            if (result.Kind != ParserResultKind.Success)
                return;

            Assemblies.Add(result.Output);
        }

        void HandleDocumentParsed(ParserResult<IDocument> result)
        {
            if (result.Kind != ParserResultKind.Success)
                return;

            Documents.Add(result.Output);
        }
        #endregion

        public void Process()
        {
            var project = new CSharpProjectContent();

            foreach (var source in Sources)
                project.AddOrUpdateFiles(source.ToTypeSystem());

            foreach (var assembly in Assemblies)
                project.AddAssemblyReferences(assembly);

            Compilation = project.CreateCompilation();
        }

        public void Generate()
        {
            if (!Directory.Exists(Options.OutputDir))
                Directory.CreateDirectory(Options.OutputDir);

            var generator = new HTMLGenerator(this);

            generator.GenerateReference(Compilation);

            foreach (var document in Documents)
                generator.GenerateDocument(document);

            // Copy the media assets.
            if (Directory.Exists("bootstrap"))
                Helpers.CopyDirectory("bootstrap", Options.OutputDir);
        }
    }

    #region Options
    public class ProjectInput
    {
        public string FullPath;
        public string Path;
        public Stream Stream;
    }

    public class Project
    {
        public string Name;
        public string OutputPath;

        public List<string> SourceDirs;
        public List<string> AssemblyDirs;
        public List<string> DocumentDirs;

        public List<string> Sources;
        public List<string> Assemblies;
        public List<string> Documents;

        internal List<ProjectInput> SourceInputs;
        internal List<ProjectInput> AssemblyInputs;
        internal List<ProjectInput> DocumentInputs;

        public Project()
        {
            SourceDirs = new List<string>();
            AssemblyDirs = new List<string>();
            DocumentDirs = new List<string>();

            Sources = new List<string>();
            Assemblies = new List<string>();
            Documents = new List<string>();

            SourceInputs = new List<ProjectInput>();
            AssemblyInputs = new List<ProjectInput>();
            DocumentInputs = new List<ProjectInput>();
        }

        public void BuildInputs()
        {
            foreach (var path in SourceDirs)
            {
                var files = Directory.EnumerateFiles(path, "*.cs",
                    SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var input = new ProjectInput
                    {
                        Path = path,
                        FullPath = file,
                    };

                    SourceInputs.Add(input);
                }
            }

            foreach (var path in AssemblyDirs)
            {
                var files = Directory.EnumerateFiles(path, "*.dll");

                foreach (var file in files)
                {
                    var matches = false;
                    foreach (var assembly in Assemblies)
                        matches |= Regex.IsMatch(file, assembly);

                    if (!matches) continue;

                    var input = new ProjectInput
                    {
                        Path = path,
                        FullPath = file,
                    };

                    AssemblyInputs.Add(input);
                }
            }

            foreach (var path in DocumentDirs)
            {
                var files = Directory.EnumerateFiles(path, "*.md");

                foreach (var file in files)
                {
                    var matches = false;
                    foreach (var doc in Documents)
                        matches |= Regex.IsMatch(file, doc);

                    if (!matches) continue;

                    var input = new ProjectInput
                    {
                        Path = path,
                        FullPath = file,
                    };

                    DocumentInputs.Add(input);
                }
            }
        }
    }

    public enum GeneratorKind
    {
        HTML,
        CHM
    }

    public class DriverOptions
    {
        public DriverOptions()
        {
            Projects = new List<Project>();
        }

        // General options
        public bool Verbose;
        public bool ShowHelpText;
        public bool OutputDebug;

        // Parser options
        public List<Project> Projects;
        public bool IgnoreParseErrors;

        // Generator options
        public GeneratorKind Generator;
        public string OutputNamespace;
        public string OutputDir;
    }
    #endregion

    #region Console Driver
    public static class ConsoleDriver
    {
        static void PrintParseDiagnostics<T>(ParserResult<T> result)
        {
            var file = result.Input.FullPath;
            if (file.StartsWith(result.Input.Path))
            {
                file = file.Substring(result.Input.Path.Length);
                file = file.TrimStart('\\');
            }

            switch (result.Kind)
            {
                case ParserResultKind.Success:
                    Console.WriteLine("  Parsed '{0}'", file);
                    break;
                case ParserResultKind.Error:
                    Console.WriteLine("  Error parsing '{0}'", file);
                    break;
                case ParserResultKind.FileNotFound:
                    Console.WriteLine("  File '{0}' was not found", file);
                    break;
            }

            foreach (var diag in result.Diagnostics)
            {
                Console.WriteLine(String.Format("{0}({1},{2}): {3}: {4}",
                    diag.FileName, diag.LineNumber, diag.ColumnNumber,
                    diag.Level.ToString().ToLower(), diag.Message));
            }
        }

        public static void Run(DriverOptions options)
        {
            Console.BufferHeight = 1999;

            var driver = new Driver(options, new TextDiagnosticPrinter());
            driver.Parser.OnSourceParsed += PrintParseDiagnostics;
            driver.Parser.OnAssemblyParsed += PrintParseDiagnostics;
            driver.Parser.OnDocumentParsed += PrintParseDiagnostics;

            driver.Setup();

            Console.WriteLine("Parsing assemblies...");
            if (!driver.Parse())
                return;

            Console.WriteLine("Processing assemblies...");
            driver.Process();

            Console.WriteLine("Generating documentation...");
            driver.Generate();
        }
    }
    #endregion
}