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
        ProjectInput Input { get; }

        /// <summary>
        /// Builds an hierarchical index of the document.
        /// </summary>
        MarkdownDocumentSection BuildIndex();
    }

    /// <summary>
    /// Represents a section in a document.
    /// </summary>
    public class MarkdownDocumentSection
    {
        public MarkdownDocumentSection Parent;
        public List<MarkdownDocumentSection> Sections;

        public Block Block;
        public string Shortcut;

        public MarkdownDocumentSection(Block block)
        {
            Block = block;
            Sections = new List<MarkdownDocumentSection>();

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
                entry = string.Join("", entry.Where(c => char.IsLetterOrDigit(c)
                    || char.IsWhiteSpace(c) || c == '-'));
                entries.Add(entry);
            }

            return string.Join("-", entries);
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Block.blockType, Block.Content);
        }
    }
}
