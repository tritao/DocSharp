using System.IO;
using DocSharp.Generators;

namespace DocSharp.Templates
{
    /// <summary>
    /// Implements a Bootstrap-based HTML template.
    /// </summary>
    public class BootstrapTemplate : IHTMLTemplate
    {
        public HTMLTextGenerator Gen { get; set; }
        public Options Options { get; private set; }

        public BootstrapTemplate(Options options)
        {
            Options = options;
        }

        public void HTMLHeadBegin(string title)
        {
            if (Options.GenerateBareHTML)
                return;

            Gen.Doctype();
            Gen.Tag(HTMLTag.HTML, new { lang = "en" });

            Gen.TagIndent(HTMLTag.Head);

            // CSS assets
            var classes = new {rel = "stylesheet", media = "screen"};
            Gen.Link("css/bootstrap.css", classes);
            Gen.Link("css/docs.css", classes);
            Gen.Link("css/syntax.css", classes);
            Gen.Link("sunlight/themes/sunlight.default.css", classes);

            // JS assets
            Gen.Javascript("js/jquery.js");
            Gen.Javascript("js/bootstrap.js");
            Gen.Javascript("sunlight/sunlight-min.js");
            Gen.Javascript("sunlight/plugins/sunlight-plugin.linenumbers.js");
            Gen.Javascript("sunlight/lang/sunlight.csharp-min.js");
            Gen.Javascript("sunlight/jquery.sunlight.js");

            Gen.TagIndent(HTMLTag.Script);
            Gen.WriteLine("$(document).ready( function($) {");
            Gen.WriteLineIndent("$('pre').sunlight({ lineNumbers: true })");
            Gen.WriteLine("})");
            Gen.CloseTagIndent();

            Gen.Content(HTMLTag.Title, title);
            Gen.CloseTagIndent();
        }

        public void HTMLHeadEnd()
        {
            if (Options.GenerateBareHTML)
                return;

            Gen.CloseTag();
        }

        public void HTMLBodyBegin()
        {
            if (Options.GenerateBareHTML)
                return;

            Gen.TagIndent(HTMLTag.Body, new
            {
                @class = "docs",
                data_spy = "scroll",
                data_target = "nav-sidebar"
            });
        }

        public void HTMLBodyEnd()
        {
            if (Options.GenerateBareHTML)
                return;

            Gen.CloseTagIndent();
        }

        public void HTMLFooterBegin()
        {
            if (Options.GenerateBareHTML)
                return;

            throw new System.NotImplementedException();
        }

        public void HTMLFooterEnd()
        {
            if (Options.GenerateBareHTML)
                return;

            throw new System.NotImplementedException();
        }

        public void Process()
        {
            if (Directory.Exists("bootstrap"))
                Helpers.CopyDirectory("bootstrap", Options.OutputDir);
        }
    }
}
