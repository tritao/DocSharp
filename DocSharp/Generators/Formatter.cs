using System.Collections.Generic;
using System.IO;
using ColorCode;
using ColorCode.Formatting;
using ColorCode.Parsing;

namespace CppSharp
{
    public class ColorCodeFormatter : IFormatter
    {
        private readonly HtmlFormatter formatter;

        public ColorCodeFormatter()
        {
            formatter = new HtmlFormatter();
        }

        public void Write(string parsedSourceCode,
                          IList<Scope> scopes,
                          IStyleSheet styleSheet,
                          TextWriter textWriter)
        {
            formatter.Write(parsedSourceCode, scopes, styleSheet, textWriter);
        }

        public void WriteFooter(IStyleSheet styleSheet,
                                ILanguage language,
                                TextWriter textWriter)
        {
        }

        public void WriteHeader(IStyleSheet styleSheet,
                                ILanguage language,
                                TextWriter textWriter)
        {
        }
    }
}