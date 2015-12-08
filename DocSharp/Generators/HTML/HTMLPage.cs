using System;
using System.Collections.Generic;
using System.Linq;

namespace DocSharp.Generators.HTML
{
    public abstract class HTMLPage : HTMLTextGenerator
    {
        public static int DefaultHeading = 3;

        public HTMLPage Parent;

        public bool GenerateBreadcrumb;

        public string Title;

        private string fileName;
        public string FileName
        {
            get
            {
                return String.IsNullOrWhiteSpace(fileName) ?
                    Title + ".html" : fileName;
            }
            set { fileName = value; }
        }

        public virtual string FullPath
        {
            get { return FileName; }
        }

        protected HTMLPage(HTMLPage parent = null)
        {
            Parent = parent;
            GenerateBreadcrumb = true;
            Title = String.Empty;
            FileName = String.Empty;
        }

        public static string GenLink(string format, params object[] args)
        {
            return "?d=" + string.Format(format, args);
        }

        public void Generate()
        {
            if (String.IsNullOrEmpty(Title))
                throw new Exception("Page title should not be empty");

            if (GenerateBreadcrumb)
                GenerateBreadcrumbs();

            GenerateContents();
        }

        private void GenerateHeader()
        {
            //template.Gen = this;
            //template.HTMLHeadBegin("Docs");
            //template.HTMLBodyBegin();

            //TagIndent(HTMLTag.Div, new { @class = "container" });
            //TagIndent(HTMLTag.Div, new { @class = "row" });

            //TagIndent(HTMLTag.Div, new { @class = "span3 sidebar" });
            //Comment("Sidebar content");
            //CloseTagIndent(); // Sidebar

            //TagIndent(HTMLTag.Div, new { @class = "span9 content" });
            //Comment("Body content");
        }

        private void GenerateFooter()
        {
            //GenerateDocumentSections(document, gen);
            CloseTagIndent(); // Div (content)

            CloseTagIndent(); // Row
            CloseTagIndent(); // Container

            //template.HTMLBodyEnd();
            //template.HTMLHeadEnd();
        }

        private List<HTMLPage> GatherPages()
        {
            var pages = new List<HTMLPage> {this};

            var currentPage = this;
            while (currentPage.Parent != null)
            {
                pages.Add(currentPage.Parent);
                currentPage = currentPage.Parent;
            }

            pages.Reverse();
            return pages;
        }

        private void GenerateBreadcrumbs()
        {
            TagIndent(HTMLTag.Ol, new { @class = "breadcrumb" });

            var pages = GatherPages();
            foreach (var page in pages)
            {
                var isActive = page == pages.Last();

                if (isActive)
                    Tag(HTMLTag.Li, new { @class = "active" });
                else
                    Tag(HTMLTag.Li);

                if (!isActive)
                    Anchor("?d=" + page.FullPath);

                if (page == pages.First())
                    GlyphIcon("home");

                Write(page.Title);

                if (!isActive)
                    CloseInlineTag(HTMLTag.A);

                CloseTag(); // Li
            }

            CloseTagIndent(); // Or
        }

        protected abstract void GenerateContents();
    }

    public static class ProjectOuputExtensions
    {
        public static void WritePage(this ProjectOutput output,
            HTMLPage page)
        {
            output.WriteOutput(page.FullPath, page);
        }
    }
}
