using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Johnny;

public static class CodegenHelpers {
    public static string GetNamespace(SyntaxNode syntaxNode) {
        SyntaxNode? current = syntaxNode.Parent;

        while (current != null && current is not NamespaceDeclarationSyntax && current is not FileScopedNamespaceDeclarationSyntax) {
            current = current.Parent;
        }

        if (current is not BaseNamespaceDeclarationSyntax namespaceDeclaration) {
            return string.Empty;
        }

        var namespaceParts = new Stack<string>();
        namespaceParts.Push(namespaceDeclaration.Name.ToString());

        while (namespaceDeclaration.Parent is BaseNamespaceDeclarationSyntax parentNamespace) {
            namespaceDeclaration = parentNamespace;
            namespaceParts.Push(namespaceDeclaration.Name.ToString());
        }

        return string.Join(".", namespaceParts);
    }
}