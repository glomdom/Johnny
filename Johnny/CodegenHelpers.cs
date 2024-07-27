using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Johnny {
    public static class CodegenHelpers {
        public static string GetNamespace(SyntaxNode syntaxNode) {
            var current = syntaxNode.Parent;

            while (current is not (NamespaceDeclarationSyntax or FileScopedNamespaceDeclarationSyntax)) {
                current = current?.Parent;
            }

            if (current is not BaseNamespaceDeclarationSyntax namespaceDeclaration) {
                return string.Empty;
            }

            var namespaceParts = new Stack<string>();

            while (namespaceDeclaration != null) {
                namespaceParts.Push(namespaceDeclaration.Name.ToString());
                namespaceDeclaration = namespaceDeclaration.Parent as BaseNamespaceDeclarationSyntax;
            }

            return string.Join(".", namespaceParts);
        }
    }
}