using System;
using System.Collections.Generic;
using System.Linq;
using DocSharp.Templates;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp.Generators.HTML
{
    class HTMLCSharpNamespacesGenerator : HTMLPage
    {
        private readonly Driver driver;
        private readonly IHTMLTemplate template;

        public HTMLCSharpNamespacesGenerator(Driver driver, IHTMLTemplate template)
        {
            this.driver = driver;
            this.template = template;
            Title = "Namespaces";
        }

        private static void GetNamespaces(INamespace @namespace,
            ICollection<INamespace> namespaces)
        {
            namespaces.Add(@namespace);
            foreach (var childNamespace in @namespace.ChildNamespaces)
                GetNamespaces(childNamespace, namespaces);
        }

        private static IEnumerable<INamespace> GetNamespaces(INamespace @namespace)
        {
            var namespaces = new List<INamespace>();
            GetNamespaces(@namespace, namespaces);
            namespaces.Sort((ns, ns1) =>
                StringComparer.OrdinalIgnoreCase.Compare(ns.FullName, ns1.FullName));
            return namespaces;
        }

        protected override void GenerateContents()
        {
            TagIndent(HTMLTag.Ul);
            var rootNamespace = driver.Compilation.RootNamespace;
            foreach (var @namespace in GetNamespaces(rootNamespace))
            {
                if (@namespace == rootNamespace)
                    continue;

                if (!@namespace.Types.Any())
                    continue;

                InlineTag(HTMLTag.Li);
                Anchor("?d=" + HTMLCSharpNamespaceGenerator.GetFilePath(@namespace));
                Write(@namespace.FullName);
                CloseInlineTag(HTMLTag.A);
                CloseTag(HTMLTag.Li);
            }
            CloseTagIndent(); // Ul
        }
    }
}