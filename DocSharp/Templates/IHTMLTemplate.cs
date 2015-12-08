using DocSharp.Generators;

namespace DocSharp.Templates
{
    public interface IHTMLTemplate
    {
        HTMLTextGenerator Gen { get; set; }
        Options Options { get; }

        /// <summary>
        /// Begins the HTML head section.
        /// </summary>
        void HTMLHeadBegin(string title);

        /// <summary>
        /// Ends the HTML head section.
        /// </summary>
        void HTMLHeadEnd();

        /// <summary>
        /// Begins the HTML body section.
        /// </summary>
        void HTMLBodyBegin();

        /// <summary>
        /// Ends the HTML body section.
        /// </summary>
        void HTMLBodyEnd();

        /// <summary>
        /// Begins the HTML footer section.
        /// </summary>
        void HTMLFooterBegin();

        /// <summary>
        /// Ends the HTML footer section.
        /// </summary>
        void HTMLFooterEnd();

        /// <summary>
        /// Called once for custom template processing.
        /// </summary>
        void Process();
    }
}
