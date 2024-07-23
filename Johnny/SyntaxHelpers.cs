using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Johnny;

public static class SyntaxHelpers {
    public static bool IsStructWithJohnnyAttribute(SyntaxNode node, CancellationToken cancellationToken) {
        return node is StructDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    public static StructDeclarationSyntax? GetStructWithJohnnyAttribute(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
        var structDecl = (StructDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        foreach (var attributeSyntaxes in structDecl.AttributeLists)
        foreach (var attributeSyntax in attributeSyntaxes.Attributes) {
            var attributeSymbol = semanticModel.GetSymbolInfo(attributeSyntax).Symbol;

            if (attributeSymbol?.ContainingType.ToDisplayString() == Data.Attribute.DisplayText) {
                return structDecl;
            }
        }

        return null;
    }
}