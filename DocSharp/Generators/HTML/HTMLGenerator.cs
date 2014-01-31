using System.Collections.Generic;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace DocSharp.Generators
{
    public class HTMLEntityReference : HTMLTextGenerator
    {
        public EntityDeclaration Declaration;
        public IUnresolvedTypeDefinition Type;

        public HTMLEntityReference(EntityDeclaration decl)
        {
            Declaration = decl;
        }

        public HTMLEntityReference(IUnresolvedTypeDefinition type)
        {
            Type = type;
        }
    }

    public class HTMLReferenceVisitor : TypeVisitor
    {
        private readonly Driver driver;

        public List<HTMLEntityReference> Entities;

        public HTMLReferenceVisitor(Driver driver)
        {
            this.driver = driver;
            Entities = new List<HTMLEntityReference>();
        }

        public override IType VisitTypeDefinition(ITypeDefinition type)
        {
            var doc = new HTMLEntityReference(type);
            Entities.Add(doc);

            return type.VisitChildren(this);
        }
    }
}
