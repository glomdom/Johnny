using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Johnny;

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

        while (true) {
            namespaceParts.Push(namespaceDeclaration.Name.ToString());

            var parent = namespaceDeclaration.Parent as BaseNamespaceDeclarationSyntax;
            if (parent == null) {
                break;
            }

            namespaceDeclaration = parent;
        }

        return string.Join(".", namespaceParts);
    }

    public static void HandleAttributes(ref string currentMethod, List<AttributeSyntax> attributesList) {
        if (attributesList.Count == 0) {
            return;
        }

        foreach (var arguments in attributesList.Select(attribute => attribute.ArgumentList?.Arguments.ToList())) {
            if (arguments is not { Count: > 0 }) {
                continue;
            }

            foreach (var argumentValue in arguments.Select(argument => argument.ToString())) {
                if (argumentValue.Contains("Endianness.Little")) {
                    currentMethod = $"ReverseBytes({currentMethod.Trim().TrimEnd(';')});";
                }
            }
        }
    }
}