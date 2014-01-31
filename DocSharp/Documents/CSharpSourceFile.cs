using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;

namespace DocSharp.Documents
{    
    /// <summary>
    /// Represents a C# source file in the documentation system.
    /// </summary>
    public class CSharpSourceFile
    {
        /// <summary>
        /// Project input for the source file.
        /// </summary>
        public ProjectInput Input { get; set; }

        /// <summary>
        /// NRefactory syntactic representation.
        /// </summary>
        public SyntaxTree SyntaxTree { get; set; }

        /// <summary>
        /// NRefactory semantic/type-system representation.
        /// </summary>
        public CSharpUnresolvedFile UnresolvedFile { get; set; }

        /// <summary>
        /// NRefactory resolver that maps between the syntactic and semantic
        /// representations.
        /// </summary>
        public CSharpAstResolver Resolver { get; set; }
    }
}
