using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Johnny {
    public static class SyntaxHelpers {
        public static bool IsStructWithJohnnyAttribute(SyntaxNode node, CancellationToken cancellationToken) {
            return node is StructDeclarationSyntax { AttributeLists.Count: > 0 };
        }

        public static StructDeclarationSyntax? GetStructWithJohnnyAttribute(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
            var structDecl = (StructDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            return (from attributeList in structDecl.AttributeLists from attribute in attributeList.Attributes select semanticModel.GetSymbolInfo(attribute).Symbol).Any(attributeSymbol => attributeSymbol?.ContainingType.ToDisplayString() == Data.Attribute.DisplayText) ? structDecl : null;
        }
    }
}