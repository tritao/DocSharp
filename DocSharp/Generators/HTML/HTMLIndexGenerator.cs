
using System.Linq;
using DocSharp.Documents;

namespace DocSharp.Generators.HTML
{
    public class HTMLIndexGenerator : HTMLPage
    {
        public Driver Driver { get; set; }
        public HTMLCSharpGenerator CSharpGen { get; set; }

        public HTMLIndexGenerator(Driver driver, HTMLCSharpGenerator csharpGen)
        {
            Driver = driver;
            CSharpGen = csharpGen;
            Title = "Index";
            FileName = "index.html";
            GenerateBreadcrumb = false;
        }

        protected override void GenerateContents()
        {
            Anchor(GenLink(CSharpGen.Namespaces.FullPath));
            Write("API Reference");
            CloseInlineTag(HTMLTag.A);
            LineBreak();

            foreach (var document in Driver.Documents.Cast<MarkdownDocument>())
            {
                Anchor(GenLink(document.Title + ".html"));
                Write(document.Title);
                CloseTag(HTMLTag.A);
                LineBreak();
            }
        }
    }
}
