using System.Collections.Generic;
using System.Linq;

namespace DocSharp.Generators
{
    public enum HTMLTag
    {
        HTML,
        Head,
        Body,
        Title,
        Meta,
        Script,
        Link,
        Div,
        Section,
        Ul,
        Li,
        A,
        P
    }

    public class HTMLTextGenerator : TextGenerator
    {
        public Stack<HTMLTag> Tags;

        public HTMLTextGenerator()
        {
            Tags = new Stack<HTMLTag>();
        }

        public void Doctype()
        {
            WriteLine("<!DOCTYPE html>");
        }

        public void Comment(string text)
        {
            WriteLine("<!-- {0} -->", text);
        }

        static string GetAttributesString(params object[] attributes)
        {
            if (attributes == null)
                return string.Empty;

            var keyValues = new List<string>();
            foreach (var attr in attributes)
            {
                foreach (var prop in attr.GetType().GetProperties())
                {
                    var name = prop.Name.Replace('_', '-');
                    var value = prop.GetValue(attr);
                    keyValues.Add(string.Format("{0}='{1}'", name, value));
                }
            }

            var attrs = string.Join(" ", keyValues);
            if (keyValues.Count > 0)
                attrs = " " + attrs;

            return attrs;
        }

        public void Content(HTMLTag tag, string content, params object[] attributes)
        {
            Write("<{0}{1}>", tag.ToString().ToLowerInvariant(),
                GetAttributesString(attributes));
            Write(content);
            CloseTag(tag);
        }

        public void Link(string href, params object[] attributes)
        {
            var attrs = new object[] { new { href } }.Concat(attributes)
                .ToArray();
            InlineTag(HTMLTag.Link, attrs);
            NewLine();
        }

        public void Script(string src, params object[] attributes)
        {
            var attrs = new object[] { new { src } }.Concat(attributes)
                .ToArray();
            InlineTag(HTMLTag.Script, attrs);
            CloseTag(HTMLTag.Script);
        }

        public void Anchor(string href, params object[] attributes)
        {
            var attrs = new object[] { new { href } }.Concat(attributes)
                .ToArray();
            InlineTag(HTMLTag.A, attrs);
        }

        public void Div(params object[] attributes)
        {
            TagIndent(HTMLTag.Div, attributes);
        }

        public void Section(params object[] attributes)
        {
            TagIndent(HTMLTag.Section, attributes);
        }

        public void Tag(HTMLTag tag, params object[] attributes)
        {
            InlineTag(tag, attributes);
            NewLine();
            Tags.Push(tag);
        }

        public void TagIndent(HTMLTag tag, params object[] attributes)
        {
            Tag(tag, attributes);
            PushIndent();
        }

        public void InlineTag(HTMLTag tag, params object[] attributes)
        {
            Write("<{0}{1}>", tag.ToString().ToLowerInvariant(),
                GetAttributesString(attributes));
        }

        public void CloseTag()
        {
            var tag = Tags.Pop();
            CloseTag(tag);
        }

        public void CloseTag(HTMLTag tag)
        {
            WriteLine("</{0}>", tag.ToString().ToLowerInvariant());
        }

        public void CloseTagIndent()
        {
            PopIndent();
            CloseTag();
        }

        public void CloseTagIndent(HTMLTag tag)
        {
            PopIndent();
            CloseTag(tag);
        }
    }
}