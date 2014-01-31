using DocSharp.Generators;

namespace DocSharp.Templates
{
    public interface ITemplate
    {
        #region HTML generator

        void HTMLHead(HTMLTextGenerator generator, string title);
        void HTMLFooter(HTMLTextGenerator generator);

        void Process(Options options);

        #endregion
    }
}
