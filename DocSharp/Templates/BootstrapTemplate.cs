using System.IO;
using DocSharp.Generators;

namespace DocSharp.Templates
{
    /// <summary>
    /// Implements a Bootstrap-based HTML template.
    /// </summary>
    public class BootstrapTemplate : ITemplate
    {
        public void HTMLHead(HTMLTextGenerator gen, string title)
        {
            gen.Doctype();

            gen.Tag(HTMLTag.HTML, new { lang = "en" });

            gen.TagIndent(HTMLTag.Head);

            // CSS assets
            gen.Link("css/bootstrap.css",
                     new { rel = "stylesheet", media = "screen" });
            gen.Link("css/docs.css",
                new { rel = "stylesheet", media = "screen" });
            gen.Link("css/syntax.css",
                new { rel = "stylesheet", media = "screen" });
            gen.Link("sunlight/themes/sunlight.default.css",
                new { rel = "stylesheet", media = "screen" });

            // JS assets
            gen.Javascript("js/jquery.js");
            gen.Javascript("js/bootstrap.js");
            gen.Javascript("sunlight/sunlight-min.js");
            gen.Javascript("sunlight/plugins/sunlight-plugin.linenumbers.js");
            gen.Javascript("sunlight/lang/sunlight.csharp-min.js");
            gen.Javascript("sunlight/jquery.sunlight.js");

            gen.TagIndent(HTMLTag.Script);
            gen.WriteLine("$(document).ready( function($) {");
            gen.WriteLineIndent("$('pre').sunlight({ lineNumbers: true })");
            gen.WriteLine("})");
            gen.CloseTagIndent();

            gen.Content(HTMLTag.Title, title);
            gen.CloseTagIndent();
        }

        public void HTMLFooter(HTMLTextGenerator generator)
        {
            throw new System.NotImplementedException();
        }

        public void Process(Options options)
        {
            if (Directory.Exists("bootstrap"))
                Helpers.CopyDirectory("bootstrap", options.OutputDir);
        }
    }
}
