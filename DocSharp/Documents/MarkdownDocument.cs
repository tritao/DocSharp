using System.Collections.Generic;
using System.IO;
using MarkdownDeep;

namespace DocSharp.Documents
{
    /// <summary>
    /// Represents a Markdown text document.
    /// </summary>
    public class MarkdownDocument : IDocument
    {
        public MarkdownDocumentSection Index { get; private set; }
        public ProjectInput Input { get; set; }
        public Markdown Markdown;

        public string Title
        {
            get
            {
                return Path.GetFileNameWithoutExtension(Input.FullPath);
            }
        }

        public MarkdownDocument()
        {
            Markdown = new Markdown();
        }

        /// <summary>
        /// Builds an hierarchical index of the document.
        /// </summary>
        public MarkdownDocumentSection BuildIndex()
        {
            var index = new MarkdownDocumentSection(null);

            var stack = new Stack<MarkdownDocumentSection>();
            stack.Push(index);

            var currentLevel = 0;
            foreach (var block in Markdown.Blocks)
                currentLevel = BuildBlock(block, stack, currentLevel);

            return index;
        }

        private static int BuildBlock(Block block, Stack<MarkdownDocumentSection> stack,
            int currentLevel)
        {
            if (!block.IsSectionHeader)
                return currentLevel;

            var section = new MarkdownDocumentSection(block)
            {
                Parent = stack.Peek()
            };

            var level = (int) block.blockType - (int) BlockType.h1 + 1;
            var newLevel = (level - currentLevel);

            // Same-level section
            if (newLevel == 0)
            {
                stack.Pop();
            }
            // Outer-level section.
            else if (newLevel < 0)
            {
                while (currentLevel >= level)
                    currentLevel -= (int) stack.Pop().Block.blockType;
                stack.Pop();
            }

            stack.Peek().Sections.Add(section);
            stack.Push(section);
            currentLevel = level;
            return currentLevel;
        }
    }
}
