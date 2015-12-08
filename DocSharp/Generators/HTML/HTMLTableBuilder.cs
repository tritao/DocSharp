using System;
using System.Collections.Generic;
using System.Linq;

namespace DocSharp.Generators.HTML
{
    /// <summary>
    /// Represents a column in an HTML table.
    /// </summary>
    public struct HTMLTableColumn
    {
        public string Description;
        public string Class;
    }

    /// <summary>
    /// Represents a row in an HTML table.
    /// </summary>
    public struct HTMLTableRow
    {
    }

    /// <summary>
    /// Represents an HTML table builder helper class.
    /// </summary>
    public class HTMLTableBuilder : HTMLTextGenerator
    {
        public string Name;
        public List<HTMLTableColumn> Columns;
        public List<string[]> Rows;
        public bool GenerateColumnDescriptions;

        public HTMLTableBuilder(string heading = null)
        {
            Name = heading;
            Columns = new List<HTMLTableColumn>();
            Rows = new List<string[]>();
        }

        public HTMLTableBuilder(string heading, IEnumerable<string> columns)
        {
            Name = heading;
            Columns = columns.Select(s => new HTMLTableColumn { Description = s})
                .ToList();
            Rows = new List<string[]>();
        }

        /// <summary>
        /// Adds a column to the table.
        /// </summary>
        /// <param name="description"></param>
        public void Column(string description)
        {
            var row = new HTMLTableColumn { Description = description };
            Columns.Add(row);
        }

        /// <summary>
        /// Adds a row to the table.
        /// </summary>
        /// <remarks>
        /// Values should have as much items as number of columns.
        /// </remarks>
        /// <param name="values"></param>
        public void Row(params string[] values)
        {
            if (values.Length != Columns.Count)
                throw new Exception("Different number of values than columns");

            Rows.Add(values);
        }

        /// <summary>
        /// Generates the HTML code representing the table.
        /// </summary>
        public override string ToString()
        {
            Clear();
            TagIndent(HTMLTag.Div, new { @class = "panel panel-default" });

            TagIndent(HTMLTag.Div, new { @class = "panel-heading" });
            Heading(Name, 4);
            CloseTagIndent(); // Panel heading

            TagIndent(HTMLTag.Table, new { @class = "table table-striped table-condensed" });

            if (GenerateColumnDescriptions)
            {
                TagIndent(HTMLTag.Tr);

                foreach (var column in Columns)
                    Content(HTMLTag.Th, column.Description);

                CloseTagIndent(); // Tr
            }

            foreach (var row in Rows)
            {
                TagIndent(HTMLTag.Tr);

                foreach (var item in row)
                {
                    if (item == row.First())
                        Tag(HTMLTag.Td, new { @class = "col-xs-3"});
                    else
                        Tag(HTMLTag.Td);

                    Write(item);
                    CloseTag();
                }

                CloseTagIndent(); // Tr
            }

            CloseTagIndent(); // Table
            CloseTagIndent(); // Panel

            return base.ToString();
        }
    }
}
