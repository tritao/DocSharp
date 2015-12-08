using System.Text;
using DocSharp.Documents;
using DocSharp.Templates;
using MarkdownDeep;

namespace DocSharp.Generators.HTML
{
    class HTMLMarkdownGenerator : HTMLPage
    {
        private readonly Driver Driver;
        private readonly IHTMLTemplate Template;
        private readonly MarkdownDocument Document;

        public HTMLMarkdownGenerator(Driver driver, IHTMLTemplate template,
            MarkdownDocument document)
        {
            Driver = driver;
            Template = template;
            Document = document;

            Title = document.Title;
            Parent = driver.Root;
        }

        protected override void GenerateContents()
        {
            //GenerateDocumentIndex();
            GenerateDocumentSections();
        }

        #region Index

        void GenerateDocumentIndex()
        {
            var index = Document.BuildIndex();

            TagIndent(HTMLTag.Div, new { @class = "contents well well-small" });
            Content(HTMLTag.Div, "Contents", new { @class = "nav-header" });
            GenerateDocumentIndexSections(index);
            CloseTagIndent();
        }

        void GenerateDocumentIndexSections(MarkdownDocumentSection section)
        {
            var isRootSection = section.Parent == null;
            if (isRootSection)
                TagIndent(HTMLTag.Ul, new { @class = "nav nav-list nav-sidebar" });

            foreach (var childSection in section.Sections)
            {
                if (childSection.Block != null)
                    GenerateDocumentIndexSection(childSection);

                GenerateDocumentIndexSections(childSection);
            }

            if (isRootSection)
                CloseTagIndent();
        }

        void GenerateDocumentIndexSection(MarkdownDocumentSection section)
        {
            if (section.Block.blockType > BlockType.h2)
                return;

            if (section.Block.blockType == BlockType.h1)
            {
                InlineTag(HTMLTag.Li, new { @class = "divider" });
                CloseTag(HTMLTag.Li);
            }

            TagIndent(HTMLTag.Li);

            Anchor("#" + section.Shortcut);
            Write(section.Block.Content);
            CloseTag(HTMLTag.A);

            var isChildSection = section.Parent != null;
            if (isChildSection)
            {
                Tag(HTMLTag.Ul);
                CloseTag();
            }

            CloseTagIndent();
        }

        #endregion

        void GenerateDocumentSections()
        {
            var markdown = Document as MarkdownDocument;

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
                    Section((new { id = section.Shortcut }));
                }

                var sb = new StringBuilder();
                block.Render(markdown.Markdown, sb);

                if (block.blockType == BlockType.codeblock)
                    WriteRaw(sb.ToString());
                else
                    Write(sb.ToString());

                if (block.IsSectionHeader)
                {
                    if (block.blockType == BlockType.h1)
                    {
                        InlineTag(HTMLTag.Div, new { @class = "divider" });
                        CloseTag(HTMLTag.Div);
                    }

                    CloseTagIndent();
                }
            }
        }
    }
}
