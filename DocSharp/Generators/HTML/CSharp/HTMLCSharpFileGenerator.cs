using System.IO;
using DocSharp.Documents;
using DocSharp.Templates;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp.Generators.HTML
{
    class HTMLCSharpFileGenerator : HTMLPage
    {
        public ObservableAstVisitor Visitor;
        public CSharpSourceFile File;
        private IHTMLTemplate Template;

        public HTMLCSharpFileGenerator(CSharpSourceFile file, IHTMLTemplate template)
        {
            File = file;
            Template = template;
            Template.Gen = this;
            Title = File.SyntaxTree.FileName;
        }

        protected override void GenerateContents()
        {
            Visitor = new ObservableAstVisitor();
            Visitor.EnterPropertyDeclaration += OnEnterPropertyDeclaration;
            Visitor.EnterMethodDeclaration += OnEnterMethodDeclaration;
            Visitor.EnterDelegateDeclaration += OnEnterDelegateDeclaration;
            Visitor.EnterEventDeclaration += OnEnterEventDeclaration;
            File.SyntaxTree.AcceptVisitor(Visitor);

            var baseName = Path.GetFileNameWithoutExtension(File.SyntaxTree.FileName);
            Template.HTMLHeadBegin(baseName);
        }

        void OnEnterPropertyDeclaration(PropertyDeclaration property)
        {
            var result = File.Resolver.Resolve(property);
            if (result.IsError)
                return;

            var type = result.Type as ITypeDefinition;
            if (type == null)
                return;
            
            WriteLine("{0}", type.Documentation);
            WriteLine("{0}", type.Name);
        }

        void OnEnterMethodDeclaration(MethodDeclaration method)
        {
        }

        void OnEnterDelegateDeclaration(DelegateDeclaration @delegate)
        {
        }

        void OnEnterEventDeclaration(EventDeclaration @event)
        {
        }

        public static string GetFilePath(ProjectInput file)
        {
            var subPath = file.FullPath.Remove(0, file.BasePath.Length);

            if (Path.IsPathRooted(subPath))
                subPath = subPath.Substring(1);

            var baseDir = Path.GetDirectoryName(subPath);
            var baseName = Path.GetFileNameWithoutExtension(subPath);
            var outPath = Path.Combine(baseDir, baseName);

            return string.Format(@"files\{0}.html", outPath);
        }
    }
}