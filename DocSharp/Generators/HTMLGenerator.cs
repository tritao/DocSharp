using System.IO;
using System.Text;
using ICSharpCode.NRefactory.TypeSystem;
using MarkdownDeep;

namespace DocSharp.Generators
{
    public class HTMLGenerator : IGenerator
    {
        public Driver Driver;

        public HTMLGenerator(Driver driver)
        {
            Driver = driver;
        }

        public void GenerateDocument(IDocument document)
        {
            var gen = new HTMLTextGenerator();
            gen.Doctype();

            gen.Tag(HTMLTag.HTML, new { lang = "en"});

            gen.TagIndent(HTMLTag.Head);

            // CSS assets
            gen.Link("css/bootstrap.css",
                new { rel = "stylesheet", media = "screen" });
            gen.Link("css/docs.css",
                new { rel = "stylesheet", media = "screen" });
            //gen.Link("css/bootstrap-responsive.css",
            //    new { rel = "stylesheet", media = "screen"});

            // JS assets
            gen.Script("js/jquery.js", new { type = "text/javascript" });
            gen.Script("js/bootstrap.js", new { type = "text/javascript" });

            gen.Content(HTMLTag.Title, "Docs");
            gen.CloseTagIndent();

            gen.TagIndent(HTMLTag.Body, new { data_spy = "scroll",
                data_target="nav-sidebar" });
            gen.TagIndent(HTMLTag.Div, new { @class = "container" });
            gen.TagIndent(HTMLTag.Div, new { @class = "row" });

            gen.TagIndent(HTMLTag.Div, new { @class = "span3" });
            gen.Comment("Sidebar content");
            GenerateDocumentIndex(document, gen);
            gen.CloseTagIndent();

            gen.TagIndent(HTMLTag.Div, new { @class = "span9" });
            gen.Comment("Body content");
            GenerateDocumentSections(document, gen);
            gen.CloseTagIndent();

            gen.CloseTagIndent();
            gen.CloseTagIndent();
            gen.CloseTagIndent();

            gen.CloseTag();

            var file = Path.GetFileNameWithoutExtension(document.Input.FullPath)
                + ".html";
            var fullPath = Path.Combine(Driver.Options.OutputDir, file);

            File.WriteAllText(fullPath, gen.ToString());
        }

        public void GenerateReference(ICompilation compilation)
        {
            var root = compilation.RootNamespace;
        }

        public void GenerateDocumentIndex(IDocument document,
            HTMLTextGenerator gen)
        {
            var index = document.BuildIndex();

            gen.TagIndent(HTMLTag.Div, new { @class = "contents",
                data_spy = "affix", data_offset_top="" });
            gen.Content(HTMLTag.Li, "Contents", new { @class = "nav-header" });
            GenerateDocumentIndexSections(index, gen);
            gen.CloseTagIndent();
        }

        public void GenerateDocumentIndexSections(DocumentSection section,
            HTMLTextGenerator gen)
        {
            var isRootSection = section.Parent == null;
            if (isRootSection)
                gen.TagIndent(HTMLTag.Ul, new { @class = "nav nav-list nav-sidebar" });
            else
                gen.TagIndent(HTMLTag.Ul);

            foreach (var childSection in section.Sections)
            {
                if (childSection.Block != null)
                GenerateDocumentIndexSection(childSection, gen);

                GenerateDocumentIndexSections(childSection, gen);
            }

            gen.CloseTagIndent();
        }

        public void GenerateDocumentIndexSection(DocumentSection section,
            HTMLTextGenerator gen)
        {
            if (section.Block.blockType > BlockType.h2)
                return;

            if (section.Block.blockType == BlockType.h1)
            {
                gen.InlineTag(HTMLTag.Li, new { @class = "divider" });
                gen.CloseTag(HTMLTag.Li);
            }

            gen.Anchor("#" + section.Shortcut);
            gen.Content(HTMLTag.Li, section.Block.Content);
            gen.CloseTag(HTMLTag.A);
        }

        public void GenerateDocumentSections(IDocument document,
            HTMLTextGenerator gen)
        {
            var markdown = document as MarkdownDocument;

            foreach (var block in markdown.Markdown.Blocks)
            {
                if (block.IsSectionHeader)
                {
                    var section = new DocumentSection(block);
                    gen.Section((new { id = section.Shortcut }));
                }

                var sb = new StringBuilder();
                block.Render(markdown.Markdown, sb);

                gen.WriteLine(sb.ToString());

                if (block.IsSectionHeader)
                {
                    if (block.blockType == BlockType.h1)
                    {
                        gen.InlineTag(HTMLTag.Div, new {@class = "divider"});
                        gen.CloseTag(HTMLTag.Div);
                    }

                    gen.CloseTagIndent();
                }
            }
        }
    }
}
