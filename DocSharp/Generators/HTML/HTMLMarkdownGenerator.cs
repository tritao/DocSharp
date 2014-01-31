using System.IO;
using System.Text;
using DocSharp.Documents;
using DocSharp.Templates;
using MarkdownDeep;

namespace DocSharp.Generators.HTML
{
    class HTMLMarkdownGenerator
    {
        private readonly Driver Driver;
        private readonly ITemplate Template;

        public HTMLMarkdownGenerator(Driver driver, ITemplate template)
        {
            Driver = driver;
            Template = template;
        }

        public void GenerateDocument(IDocument document)
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
            GenerateDocumentIndex(document, gen);
            gen.CloseTagIndent();

            gen.TagIndent(HTMLTag.Div, new { @class = "span9 content" });
            gen.Comment("Body content");
            GenerateDocumentSections(document, gen);
            gen.CloseTagIndent();

            gen.CloseTagIndent();
            gen.CloseTagIndent();
            gen.CloseTagIndent();

            gen.CloseTag();

            var file = Path.GetFileNameWithoutExtension(document.Input.FullPath);
            var path = string.Format("{0}.html", file);
            Driver.Output.WriteOutput(path, gen);
        }

        void GenerateDocumentIndex(IDocument document,
            HTMLTextGenerator gen)
        {
            var index = document.BuildIndex();

            gen.TagIndent(HTMLTag.Div, new { @class = "contents well well-small" });
            gen.Content(HTMLTag.Div, "Contents", new { @class = "nav-header" });
            GenerateDocumentIndexSections(index, gen);
            gen.CloseTagIndent();
        }

        void GenerateDocumentIndexSections(MarkdownDocumentSection section,
            HTMLTextGenerator gen)
        {
            var isRootSection = section.Parent == null;
            if (isRootSection)
                gen.TagIndent(HTMLTag.Ul, new { @class = "nav nav-list nav-sidebar" });

            foreach (var childSection in section.Sections)
            {
                if (childSection.Block != null)
                    GenerateDocumentIndexSection(childSection, gen);

                GenerateDocumentIndexSections(childSection, gen);
            }

            if (isRootSection)
                gen.CloseTagIndent();
        }

        void GenerateDocumentIndexSection(MarkdownDocumentSection section,
            HTMLTextGenerator gen)
        {
            if (section.Block.blockType > BlockType.h2)
                return;

            if (section.Block.blockType == BlockType.h1)
            {
                gen.InlineTag(HTMLTag.Li, new { @class = "divider" });
                gen.CloseTag(HTMLTag.Li);
            }

            gen.TagIndent(HTMLTag.Li);

            gen.Anchor("#" + section.Shortcut);
            gen.Write(section.Block.Content);
            gen.CloseTag(HTMLTag.A);

            var isChildSection = section.Parent != null;
            if (isChildSection)
            {
                gen.Tag(HTMLTag.Ul);
                gen.CloseTag();
            }

            gen.CloseTagIndent();
        }

        void GenerateDocumentSections(IDocument document,
            HTMLTextGenerator gen)
        {
            var markdown = document as MarkdownDocument;

            foreach (var block in markdown.Markdown.Blocks)
            {
                if (block.blockType != BlockType.codeblock)
                    continue;

                if (block.codeBlockLang.Trim() != "csharp")
                    continue;

                //var sourceCode = block.Content;
                //var buffer = new StringBuilder(sourceCode.Length * 2);

                //using (TextWriter writer = new StringWriter(buffer))
                //{
                //    var colorizer = new CodeColorizer();
                //    colorizer.Colorize(sourceCode, Languages.CSharp,
                //        new ColorCodeFormatter(), StyleSheets.Default, writer);

                //    block.contentStart = -1;
                //    block.buf = sourceCode;
                //}
            }

            foreach (var block in markdown.Markdown.Blocks)
            {
                if (block.IsSectionHeader)
                {
                    var section = new MarkdownDocumentSection(block);
                    gen.Section((new { id = section.Shortcut }));
                }

                var sb = new StringBuilder();
                block.Render(markdown.Markdown, sb);

                if (block.blockType == BlockType.codeblock)
                    gen.WriteRaw(sb.ToString());
                else
                    gen.Write(sb.ToString());

                if (block.IsSectionHeader)
                {
                    if (block.blockType == BlockType.h1)
                    {
                        gen.InlineTag(HTMLTag.Div, new { @class = "divider" });
                        gen.CloseTag(HTMLTag.Div);
                    }

                    gen.CloseTagIndent();
                }
            }
        }
    }
}
