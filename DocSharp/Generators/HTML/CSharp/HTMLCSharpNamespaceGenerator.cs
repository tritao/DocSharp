using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocSharp.Templates;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp.Generators.HTML
{
    class HTMLCSharpNamespaceGenerator : HTMLPage
    {
        private readonly IHTMLTemplate template;
        private readonly INamespace @namespace;

        public static string GetNamespaceName(INamespace @namespace)
        {
            return string.IsNullOrWhiteSpace(@namespace.FullName)
                ? "root" : @namespace.FullName;
        }

        public HTMLCSharpNamespaceGenerator(IHTMLTemplate template,
            INamespace @namespace)
        {
            this.template = template;
            this.@namespace = @namespace;

            Title = GetNamespaceName(@namespace);
        }

        public override string FullPath
        {
            get { return GetFilePath(@namespace); }
        }

        public static string GetFilePath(INamespace @namespace)
        {
            var baseDir = GetFilePathBase(@namespace);
            var baseName = Path.GetFileNameWithoutExtension(@namespace.Name);

            if (string.IsNullOrWhiteSpace(@namespace.Name))
                baseName = "root";

            var outPath = Path.Combine(baseDir, baseName);
            return string.Format(@"namespaces\{0}.html", outPath).Replace('\\', '/');
        }

        protected override void GenerateContents()
        {
            var baseDir = GetFilePathBase(@namespace);
            var numPaths = baseDir.Split(Path.DirectorySeparatorChar).Length;
            LinkHrefPrefix = StringExtensions.Repeat("../", numPaths + 1);

            var types = GetTypes(@namespace);

            GenerateEntities("Classes",
                types.Where(type => type.Kind == TypeKind.Class));

            GenerateEntities("Structures",
                types.Where(type => type.Kind == TypeKind.Struct));

            GenerateEntities("Interfaces",
                types.Where(type => type.Kind == TypeKind.Interface));

            GenerateEntities("Delegates",
                types.Where(type => type.Kind == TypeKind.Delegate));

            GenerateEntities("Enumerations",
                types.Where(type => type.Kind == TypeKind.Enum));
        }

        static IEnumerable<ITypeDefinition> GetTypes(INamespace @namespace)
        {
            var types = new List<ITypeDefinition>(@namespace.Types);
            types.Sort((def0, def1) => StringComparer.OrdinalIgnoreCase.Compare(
                def0.Name, def1.Name));

            var defs = new SortedDictionary<string, ITypeDefinition>();
            foreach (var type in types)
            {
                if (!defs.ContainsKey(type.FullName))
                {
                    defs[type.FullName] = type;
                    continue;
                }

                var existingType = defs[type.FullName];
                if (existingType.Members.Count > type.Members.Count)
                    continue;


                if (existingType.Documentation != null && type.Documentation == null)
                    continue;

                defs[type.FullName] = type;
            }

            return defs.Values;
        }

        private void GenerateEntities(string name, IEnumerable<ITypeDefinition> types)
        {
            var typeDefinitions = types.ToList();
            if (!typeDefinitions.Any())
                return;

            InlineTag(HTMLTag.H4);
            GlyphIcon("expand");
            Write(name);
            CloseTag(HTMLTag.H4);

            TagIndent(HTMLTag.Table, new { @class = "table table-bordered table-striped table-condensed" });

            foreach (var type in typeDefinitions)
            {
                TagIndent(HTMLTag.Tr);

                Tag(HTMLTag.Td);

                Anchor(GenLink(HTMLCSharpClassGenerator.GetFilePath(type)));
                WriteLine(type.Name);
                CloseTag(HTMLTag.A);

                CloseTag(); // Td

                Tag(HTMLTag.Td);
                WriteLine(type.Documentation);
                CloseTag();

                CloseTagIndent(); // Tr
            }

            CloseTagIndent(); // Table
        }

        public static string GetFilePathBase(INamespace @namespace)
        {
            var namespaces = new List<string>();

            var parent = @namespace.ParentNamespace;
            while (parent != null)
            {
                namespaces.Add(parent.Name);
                parent = parent.ParentNamespace;
            }

            namespaces.Reverse();
            return Path.Combine(namespaces.ToArray());
        }
    }
}