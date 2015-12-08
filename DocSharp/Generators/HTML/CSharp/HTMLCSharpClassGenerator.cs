using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocSharp.Generators.HTML.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp.Generators.HTML
{
    public class HTMLCSharpClassGenerator : HTMLPage
    {
        public ITypeDefinition Type;

        public HTMLCSharpClassGenerator(ITypeDefinition type)
        {
            Type = type;
            Title = Type.Name;
        }

        public override string FullPath
        {
            get { return GetFilePath(Type); }
        }

        public static string GetFilePath(INamedElement element)
        {
            var namespaces = new List<string>();
            namespaces.AddRange(element.Namespace.Split('.'));

            var namespacePath = Path.Combine(namespaces.ToArray());
            var outPath = Path.Combine(namespacePath, element.Name);
            return string.Format(@"classes/{0}.html", outPath).Replace('\\', '/');
        }

        protected override void GenerateContents()
        {
            var title = string.Format("{0} {1}", Type.Name, Type.Kind);

            Heading(title, DefaultHeading);
            Paragraph(Type.Documentation);

            var inherits = new HTMLTextGenerator();

            // Write the type hierarchy.
            var bases = Type.GetAllBaseTypeDefinitions().ToList();
            bases.RemoveAt(bases.Count - 1);

            if (bases.Any())
            {
                Heading("Hierarchy:", DefaultHeading + 1);
                foreach (var @base in bases)
                    Paragraph(GetLink(GetLocalHref(@base), @base.FullName));
            }

            GenerateNestedTypesSummary();
            GenerateConstructorsSummary();
            GeneratePropertiesSummary();
            GenerateFieldsSummary();
            GenerateMethodsSummary();
            GenerateEventsSummary();
        }

        private void GenerateNestedTypesSummary()
        {
            if (!Type.NestedTypes.Any())
                return;

            var table = new HTMLTableBuilder("Nested Types",
                new[] { "Modifier and Type", "Class and Description" });

            foreach (var nestedType in Type.NestedTypes)
            {
                var link = GetLink(GetLocalHref(nestedType), GetTypeName(nestedType.DeclaringType));
                table.Row(link, nestedType.Name);
            }

            Write(table);
        }

        private void GenerateEventsSummary()
        {
            if (!Type.Events.Any())
                return;

            var table = new HTMLTableBuilder("Events",
                new[] { "Type", "Name" });

            foreach (var @event in Type.Events)
            {
                var link = GetLink(GetLocalHref(@event), GetTypeName(@event.DeclaringType));
                table.Row(link, @event.Name);
            }

            Write(table);
        }

        #region C# constructs tables

        private void GenerateConstructorsSummary()
        {
            var constructors = Type.GetConstructors().ToList();

            if (!constructors.Any())
                return;

            var table = new HTMLTableBuilder("Constructors",
                new[] { "Signature", "Description" });

            foreach (var ctor in constructors)
            {
                if (ctor.SymbolKind != SymbolKind.Constructor)
                    return;

                var desc = GetLink(GetLocalHref(ctor), GetTypeName(ctor.DeclaringType));
                desc.Write(" ({0})", GetMethodSignature(ctor));

                table.Row(desc, ctor.Documentation);
            }

            Write(table);
        }

        private void GeneratePropertiesSummary()
        {
            var properties = Type.Properties.ToList();
            properties.Sort((prop, prop1) => string.CompareOrdinal(prop.Name,
                prop1.Name));

            if (!properties.Any())
                return;

            var table = new HTMLTableBuilder("Properties",
                new[] { "Type", "Name", "Description" });

            foreach (var prop in properties)
            {
                if (prop.SymbolKind != SymbolKind.Property)
                    return;

                var type = GetLink(GetHref(prop.ReturnType), GetTypeName(prop.ReturnType));
                var link = GetLink(GetLocalHref(prop), prop.Name);
                table.Row(GetTypeName(prop.ReturnType), link, prop.Documentation);
            }

            Write(table);
        }

        private void GenerateFieldsSummary()
        {
            var fields = Type.Fields.ToList();
            fields.Sort((prop, prop1) => string.CompareOrdinal(prop.Name,
                prop1.Name));

            if (!fields.Any())
                return;

            var table = new HTMLTableBuilder("Fields",
                new[] { "Type", "Name", "Description" });

            foreach (var field in fields)
            {
                if (field.SymbolKind != SymbolKind.Field)
                    return;

                var type = GetLink(GetHref(field.Type), GetTypeName(field.Type));
                var name = GetLink(GetLocalHref(field), field.Name);
                table.Row(type, name, field.Documentation);
            }

            Write(table);
        }

        private void GenerateMethodsSummary()
        {
            var methods = Type.Methods.ToList();
            methods.Sort((method, method1) => string.CompareOrdinal(method.Name,
                method1.Name));

            if (!methods.Any())
                return;

            var table = new HTMLTableBuilder("Methods", new[] { "Return Type", "Description" });

            foreach (var method in methods)
            {
                if (method.SymbolKind != SymbolKind.Method)
                    return;

                var desc = GetLink(GetLocalHref(method), method.Name);
                desc.Write(" ({0})", GetMethodSignature(method));

                if (!string.IsNullOrWhiteSpace(method.Documentation))
                {
                    desc.LineBreak();
                    desc.WriteLine(method.Documentation);
                }

                table.Row(GetTypeName(method.ReturnType), desc);
            }

            Write(table);
        }

        #endregion

        #region Helpers

        static string GetLocalHref(IEntity entity)
        {
            return MarkdownDocumentSection.GenerateShortcutString(entity.Name);
        }

        static string GetHref(IType type)
        {
            return MarkdownDocumentSection.GenerateShortcutString(type.Name);
        }

        static string GetTypeName(IType type)
        {
            type.AcceptVisitor(new CSharpTypePrinter());
            return type.Name;
        }

        static string GetParamSignature(IParameter param)
        {
            var name = string.Empty;

            if (param.IsRef)
                name += "ref ";
            else if (param.IsOut)
                name += "out ";

            name += GetLink(GetHref(param.Type), GetTypeName(param.Type));

            name += " " + param.Name;

            if (param.IsOptional)
                name += " = " + param.ConstantValue;

            return name;
        }

        static string GetMethodSignature(IMethod method)
        {
            var paramTypes = new List<string>();
            foreach (var param in method.Parameters)
            {
                var name = GetParamSignature(param);
                paramTypes.Add(name);
            }

            return string.Join(", ", paramTypes);
        }

        static HTMLTextGenerator GetLink(string href, string text)
        {
            var gen = new HTMLTextGenerator();
            gen.Anchor(href);
            gen.Write(text);
            gen.CloseInlineTag(HTMLTag.A);

            return gen;
        }

        #endregion

    }
}