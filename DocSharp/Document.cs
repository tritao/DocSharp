using System;
using System.Collections.Generic;
using System.Linq;
using MarkdownDeep;

namespace DocSharp
{
    /// <summary>
    /// Base interface for text documents.
    /// </summary>
    public interface IDocument
    {
        /// <summary>
        /// Builds an hierarchical index of the document.
        /// </summary>
        DocumentSection BuildIndex();

        ProjectInput Input { get; }
    }

    /// <summary>
    /// Represents a section in a document.
    /// </summary>
    public class DocumentSection
    {
        public DocumentSection Parent;
        public List<DocumentSection> Sections;

        public Block Block;
        public string Shortcut;

        public DocumentSection(Block block)
        {
            Block = block;
            Sections = new List<DocumentSection>();

            if (Block != null)
                Shortcut = GenerateShortcutString(Block.Content);
        }

        public static string GenerateShortcutString(string content)
        {
            var splitEntries = content.Split(new char[] {' '},
                StringSplitOptions.RemoveEmptyEntries);

            var entries = new List<string>();
            foreach (var split in splitEntries)
            {
                var entry = split.ToLowerInvariant();
                entry = string.Join("", entry.Where(c => char.IsLetterOrDigit(c) ||
                    char.IsWhiteSpace(c) || c == '-'));
                entries.Add(entry);
            }

            return string.Join("-", entries);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Block.blockType.ToString(),
                Block.Content);
        }
    }

    /// <summary>
    /// Represents a Markdown text document.
    /// </summary>
    public class MarkdownDocument : IDocument
    {
        public DocumentSection Index { get; private set; }
        public ProjectInput Input { get; set; }

        public MarkdownDocument()
        {
            Markdown = new Markdown();
        }

        public Markdown Markdown;

        /// <summary>
        /// Builds an hierarchical index of the document.
        /// </summary>
        public DocumentSection BuildIndex()
        {
            var index = new DocumentSection(null);

            var stack = new Stack<DocumentSection>();
            stack.Push(index);

            var currentLevel = 0;
            foreach (var block in Markdown.Blocks)
            {
                if (!block.IsSectionHeader)
                    continue;

                var section = new DocumentSection(block)
                {
                    Parent = stack.Peek()
                };

                var level = (int)block.blockType - (int)BlockType.h1 + 1;
                var newLevel = (level - currentLevel);

                // Same-level section
                if(newLevel == 0)
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
            }

            return index;
        }
    }
}
