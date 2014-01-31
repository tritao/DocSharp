using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocSharp.Documents;
using DocSharp.Templates;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp.Generators
{
    class HTMLCSharpClassGenerator : HTMLTextGenerator
    {
        public ITypeDefinition Type;

        public HTMLCSharpClassGenerator(ITypeDefinition type)
        {
            Type = type;
        }

        public void Generate()
        {
            Heading(string.Format("{0}: {1}", Type.Kind, Type.Name));

            Content(HTMLTag.P, Type.Documentation);

            //foreach (var nestedType in Type.NestedTypes)
            //    Generate();

            const int classMemberLevel = 3;

            if (Type.Properties.Any())
                Heading("Properties", classMemberLevel);

            TagIndent(HTMLTag.Table);

            foreach (var property in Type.Properties)
            {
                TagIndent(HTMLTag.Tr);

                Tag(HTMLTag.Td);
                WriteLine(property.Name);
                CloseTag();

                Tag(HTMLTag.Td);
                WriteLine(property.Documentation);
                CloseTag();

                CloseTagIndent(); // Tr
            }

            CloseTagIndent(); // Table

            if (Type.Methods.Any())
                Heading("Methods", classMemberLevel);

            TagIndent(HTMLTag.Table);

            foreach (var method in Type.Methods)
            {
                if (method.SymbolKind != SymbolKind.Method)
                    continue;

                TagIndent(HTMLTag.Tr);

                Tag(HTMLTag.Td);
                WriteLine(method.Name);
                CloseTag();

                Tag(HTMLTag.Td);
                WriteLine(method.Documentation);
                CloseTag();

                CloseTagIndent(); // Tr

                //GenerateMethod(method);
            }

            CloseTagIndent(); // Table

            if (Type.Methods.Any())
            //    Heading("Delegates", classMemberLevel);

            //foreach (var @delegate in type.Delegates)
            //{
            //    Content(HTMLTag.P, @delegate.Name);
            //}

            if (Type.Events.Any())
                Heading("Events", classMemberLevel);

            foreach (var @event in Type.Events)
            {
                Content(HTMLTag.P, @event.Documentation);
                Content(HTMLTag.P, @event.Name);
            }
        }

        void GenerateMethod(IUnresolvedMethod method)
        {
            Content(HTMLTag.P, method.Name);
        }
    }

    class HTMLCSharpNamespaceGenerator : HTMLTextGenerator
    {
        void GenerateNamespace(INamespace @namespace)
        {
            Heading("Classes");
            HorizontalRule();

            foreach (var @class in @namespace.Types)
            {

            }

            Heading("Structures");
            HorizontalRule();

            Heading("Interfaces");
            HorizontalRule();

            Heading("Delegates");
            HorizontalRule();

            Heading("Enumerations");
            HorizontalRule();
        }
    }

    class HTMLCSharpFileGenerator : HTMLTextGenerator
    {
        public ObservableAstVisitor Visitor;
        public CSharpSourceFile File;

        public HTMLCSharpFileGenerator(CSharpSourceFile file, ITemplate template)
        {
            File = file;

            Visitor = new ObservableAstVisitor();
            Visitor.EnterPropertyDeclaration += OnEnterPropertyDeclaration;
            Visitor.EnterMethodDeclaration += OnEnterMethodDeclaration;
            Visitor.EnterDelegateDeclaration += OnEnterDelegateDeclaration;
            Visitor.EnterEventDeclaration += OnEnterEventDeclaration;
            file.SyntaxTree.AcceptVisitor(Visitor);

            var baseName = Path.GetFileNameWithoutExtension(file.SyntaxTree.FileName);
            template.HTMLHead(this, baseName);
        }

        void OnEnterPropertyDeclaration(PropertyDeclaration property)
        {
            var result = File.Resolver.Resolve(property);
            if (result.IsError)
                return;

            var type = result.Type as ITypeDefinition;
            if (type == null)
                return;
            
            WriteLine("{0}", type.Documentation);
            WriteLine("{0}", type.Name);
        }

        void OnEnterMethodDeclaration(MethodDeclaration method)
        {
        }

        void OnEnterDelegateDeclaration(DelegateDeclaration @delegate)
        {
        }

        void OnEnterEventDeclaration(EventDeclaration @event)
        {
        }
    }

    public class HTMLCSharpGenerator
    {
        private readonly Driver Driver;
        private readonly ITemplate Template;

        public HTMLCSharpGenerator(Driver driver, ITemplate template)
        {
            Driver = driver;
            Template = template;
        }

        public void GenerateReference(ICompilation compilation)
        {
            GenerateReferenceIndex();

            foreach (var source in Driver.Sources)
                GenerateReferenceFile(source);

            foreach (var source in Driver.Sources)
                GenerateReferenceClasses(source);
        }

        static string GetNamespacePath(INamedElement element)
        {
            var namespaces = new List<string>();
            namespaces.AddRange(element.Namespace.Split('.'));

            return Path.Combine(Path.Combine(namespaces.ToArray()));
        }

        void GenerateReferenceClasses(CSharpSourceFile source)
        {
            var visitor = new ObservableAstVisitor();
            visitor.EnterTypeDeclaration += declaration =>
            {
                if (declaration.SymbolKind != SymbolKind.TypeDefinition)
                    return;

                var result = source.Resolver.Resolve(declaration);
                if (result.IsError)
                    return;

                var type = result.Type as ITypeDefinition;
                if (type == null)
                    return;

                var classGen = new HTMLCSharpClassGenerator(type);
                Template.HTMLHead(classGen, type.Name);
                classGen.Generate();

                var baseDir = GetNamespacePath(type);
                var baseName = Path.GetFileNameWithoutExtension(
                    source.Input.FullPath);
                var outPath = Path.Combine(baseDir, baseName);

                var path = string.Format(@"classes\{0}.html", outPath);
                Driver.Output.WriteOutput(path, classGen);
            };

            source.SyntaxTree.AcceptVisitor(visitor);
        }

        void GenerateReferenceFile(CSharpSourceFile source)
        {
            var apiGen = new HTMLCSharpFileGenerator(source, Template);

            var file = source.UnresolvedFile;

            var subPath = source.Input.FullPath.Remove(0,
                source.Input.BasePath.Length);

            if (Path.IsPathRooted(subPath))
                subPath = subPath.Substring(1);

            var baseDir = Path.GetDirectoryName(subPath);
            var baseName = Path.GetFileNameWithoutExtension(subPath);
            var outPath = Path.Combine(baseDir, baseName);

            var path = string.Format(@"files\{0}.html", outPath);
            Driver.Output.WriteOutput(path, apiGen);
        }

        void GenerateReferenceIndex()
        {
            var gen = new HTMLTextGenerator();
            Template.HTMLHead(gen, "Docs");

            gen.TagIndent(HTMLTag.Body, new
            {
                @class = "docs",
                data_spy = "scroll",
                data_target = "nav-sidebar"
            });
            gen.TagIndent(HTMLTag.Div, new { @class = "container" });
            gen.TagIndent(HTMLTag.Div, new { @class = "row" });

            gen.TagIndent(HTMLTag.Div, new { @class = "span3 sidebar" });
            gen.Comment("Sidebar content");

            gen.CloseTagIndent();

            gen.TagIndent(HTMLTag.Div, new { @class = "span9 content" });
            gen.Comment("Body content");
            //GenerateDocumentSections(document, gen);
            gen.CloseTagIndent();

            gen.CloseTagIndent();
            gen.CloseTagIndent();
            gen.CloseTagIndent();

            gen.CloseTag();

            Driver.Output.WriteOutput("APIReference.html", gen);
        }
    }
}
