using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocSharp.Documents;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Documentation;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp
{
    public enum ParserDiagnosticLevel
    {
        Ignored,
        Note,
        Warning,
        Error,
        Fatal
    }

    public struct ParserDiagnostic
    {
        public string FileName;
        public string Message;
        public ParserDiagnosticLevel Level;
        public int LineNumber;
        public int ColumnNumber;
    }

    public enum ParserResultKind
    {
        Success,
        Error,
        FileNotFound
    }

    public class ParserResult<T>
    {
        public ParserResult()
        {
            Kind = ParserResultKind.Success;
            Diagnostics = new List<ParserDiagnostic>();
        }

        public ProjectInput Input;
        public ParserResultKind Kind;
        public List<ParserDiagnostic> Diagnostics;

        public T Output;

        public bool HasErrors
        {
            get
            {
                return Diagnostics.Any(diagnostic =>
                    diagnostic.Level == ParserDiagnosticLevel.Error ||
                    diagnostic.Level == ParserDiagnosticLevel.Fatal);
            }
        }
    }

    public delegate void ParserHandler<T>(ProjectInput input, ParserResult<T> result);

    public class Parser
    {
        private readonly Options Options;
        private readonly IDiagnostics Diagnostics;

        public delegate void ParsedDelegate<T>(ParserResult<T> result);

        public ParsedDelegate<SyntaxTree> OnSourceParsed = delegate { };
        public ParsedDelegate<IUnresolvedAssembly> OnAssemblyParsed = delegate { };
        public ParsedDelegate<IDocument> OnDocumentParsed = delegate { };

        public Parser(Options options, IDiagnostics diagostics)
        {
            Options = options;
            Diagnostics = diagostics;
        }

        public void ParseSource(ProjectInput input, ParserResult<SyntaxTree> result)
        {
            try
            {
                var parser = new CSharpParser();

                try
                {
                    result.Output = parser.Parse(input.Stream, input.FullPath);

                }
                catch (Exception ex)
                {
                    result.Kind = ParserResultKind.Error;
                }
            }
            finally
            {
                OnSourceParsed(result);
            }
        }

        public void ParseAssembly(ProjectInput input, ParserResult<IUnresolvedAssembly> result)
        {
            try
            {
                var loader = new CecilLoader();

                var xml = Path.ChangeExtension(input.FullPath, ".xml");

                if (File.Exists(xml))
                    loader.DocumentationProvider = new XmlDocumentationProvider(xml);

                try
                {
                    result.Output = loader.LoadAssemblyFile(input.FullPath);
                }
                catch (Exception ex)
                {
                    result.Kind = ParserResultKind.Error;
                }
            }
            finally
            {
                OnAssemblyParsed(result);
            }
        }

        public void ParseDocument(ProjectInput input, ParserResult<IDocument> result)
        {
            try
            {
                var textReader = new StreamReader(input.Stream);
                var text = textReader.ReadToEnd();

                var document = new MarkdownDocument
                {
                    Markdown = { ExtraMode = true },
                    Input = input
                };

                //document.Markdown.Parse(text);

                result.Output = document;
            }
            finally
            {
                OnDocumentParsed(result);
            }
        }

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

        public bool Parse(Project project)
        {
            var hasErrors = false;

            foreach (var input in project.SourceInputs)
            {
                var result = ParseInput<SyntaxTree>(input, ParseSource);
                hasErrors |= result.HasErrors;
            }

            foreach (var input in project.AssemblyInputs)
            {
                var result = ParseInput<IUnresolvedAssembly>(input, ParseAssembly);
                hasErrors |= result.HasErrors;
            }

            foreach (var input in project.DocumentInputs)
            {
                var result = ParseInput<IDocument>(input, ParseDocument);
                hasErrors |= result.HasErrors;
            }

            return !hasErrors;
        }
    }
}
