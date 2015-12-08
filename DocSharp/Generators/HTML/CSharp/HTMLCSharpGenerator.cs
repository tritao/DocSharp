using System;
using System.Collections.Generic;
using DocSharp.Documents;
using DocSharp.Templates;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp.Generators.HTML
{
    public class HTMLCSharpGenerator
    {
        public readonly Driver Driver;
        public readonly IHTMLTemplate Template;

        public HTMLPage Namespaces;
        public Dictionary<string, HTMLPage> NamespacePages;
        public Dictionary<ITypeDefinition, HTMLPage> TypeDefinitionPages;
        
        public HTMLPage Files;

        public HTMLCSharpGenerator(Driver driver, IHTMLTemplate template)
        {
            Driver = driver;
            Template = template;

            NamespacePages = new Dictionary<string, HTMLPage>();
        }

        private HTMLPage GetNamespacePage(string @namespace)
        {
            if (string.IsNullOrWhiteSpace(@namespace))
                @namespace = "root";

            if (!NamespacePages.ContainsKey(@namespace))
                throw new Exception("No namespace was found");

            return NamespacePages[@namespace];
        }

        public void Generate(ICompilation compilation)
        {
            GenerateReferenceIndex();

            GenerateReferenceNamespaces();

            foreach (var source in Driver.Sources)
                GenerateReferenceFile(source);

            foreach (var source in Driver.Sources)
                GenerateReferenceClasses(source);
        }

        void GenerateReferenceNamespaces()
        {
            var root = Driver.Compilation.RootNamespace;
            GenerateReferenceNamespace(root);
        }

        void GenerateReferenceNamespace(INamespace @namespace)
        {
            var gen = new HTMLCSharpNamespaceGenerator(Template,
                @namespace)
            {
                Parent = Namespaces
            };
            gen.Generate();

            var name = HTMLCSharpNamespaceGenerator.GetNamespaceName(@namespace);
            NamespacePages[name] = gen;

            Driver.Output.WritePage(gen);

            foreach (var childNamespace in @namespace.ChildNamespaces)
                GenerateReferenceNamespace(childNamespace);
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

                var namespacePage = GetNamespacePage(type.Namespace);
                var classGen = new HTMLCSharpClassGenerator(type)
                {
                    Parent = namespacePage
                };
                classGen.Generate();

                Driver.Output.WritePage(classGen);
            };

            source.SyntaxTree.AcceptVisitor(visitor);
        }

        void GenerateReferenceFile(CSharpSourceFile source)
        {
            var apiGen = new HTMLCSharpFileGenerator(source, Template);

            var path = HTMLCSharpFileGenerator.GetFilePath(source.Input);
            Driver.Output.WriteOutput(path, apiGen);
        }

        void GenerateReferenceIndex()
        {
            Namespaces = new HTMLCSharpNamespacesGenerator(Driver, Template)
            {
                Parent = Driver.Root
            };
            Namespaces.Generate();

            Driver.Output.WritePage(Namespaces);
        }
    }
}
