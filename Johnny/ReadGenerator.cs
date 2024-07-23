using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Johnny;

[Generator]
public class ReadGenerator : IIncrementalGenerator {
    private static readonly Dictionary<string, string> TypeReaders = new() {
        { "byte", "ReadByte()" },
        { "sbyte", "ReadSByte()" },
        { "short", "ReadInt16()" },
        { "ushort", "ReadUInt16()" },
        { "int", "ReadInt32()" },
        { "uint", "ReadUInt32()" },
        { "long", "ReadInt64()" },
        { "ulong", "ReadUInt64()" },
        { "float", "ReadSingle()" },
        { "double", "ReadDouble()" },
        { "bool", "ReadBoolean()" },
        { "char", "ReadChar()" },
        { "string", "ReadString()" }
    };

    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource("JohnnyAttribute.g.cs", Data.Attribute.Source));

        var structDecls = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: SyntaxHelpers.IsStructWithJohnnyAttribute,
                transform: SyntaxHelpers.GetStructWithJohnnyAttribute
            )
            .Where(static m => m is not null);

        var compAndStructs = context.CompilationProvider.Combine(structDecls.Collect());

        context.RegisterSourceOutput(compAndStructs, static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static void Execute(Compilation comp, ImmutableArray<StructDeclarationSyntax> structs, SourceProductionContext context) {
        foreach (var structDecl in structs) {
            var structName = structDecl.Identifier.Text;
            var namespaceName = CodegenHelpers.GetNamespace(structDecl);
            var properties = structDecl.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(p => (type: p.Type.ToString(), name: p.Identifier.Text))
                .ToList();

            var sourceBuilder = new StringBuilder($@"
using System.IO;

namespace {namespaceName};

public partial struct {structName} {{
    public static {structName} ReadStruct(BinaryReader reader) {{
        var result = new {structName}();
");

            foreach (var property in properties) {
                if (TypeReaders.TryGetValue(property.type, out var readMethod)) {
                    sourceBuilder.AppendLine($"        result.{property.name} = reader.{readMethod};");
                }
            }

            sourceBuilder.AppendLine(@"
        return result;
    }
}");

            context.AddSource($"Johnny_{structName}_Reader.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}
