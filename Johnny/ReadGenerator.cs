using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Johnny {
    public struct GeneratorOptions(bool isLittleEndian) {
        public bool IsLittleEndian { get; } = isLittleEndian;
    }

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
            context.RegisterPostInitializationOutput(ctx =>
                ctx.AddSource("JohnnyAttribute.g.cs", Data.Attribute.Source));

            var structDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(SyntaxHelpers.IsStructWithJohnnyAttribute, SyntaxHelpers.GetStructWithJohnnyAttribute)
                .Where(static m => m != null);

            var compilationAndStructs = context.CompilationProvider.Combine(structDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndStructs, (spc, source) => Execute(source.Left, source.Right!, spc));
        }

        private static void Execute(Compilation comp, ImmutableArray<StructDeclarationSyntax> structs, SourceProductionContext context) {
            foreach (var structDecl in structs) {
                GenerateReadMethodForStruct(structDecl, context);
            }
        }

        private static void GenerateReadMethodForStruct(StructDeclarationSyntax structDecl, SourceProductionContext context) {
            var structName = structDecl.Identifier.Text;
            var namespaceName = CodegenHelpers.GetNamespace(structDecl);
            var properties = structDecl.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(p => (Type: p.Type.ToString(), Name: p.Identifier.Text))
                .ToList();

            var sourceBuilder = new StringBuilder($@"
using System.IO;

namespace {namespaceName}
{{
    public partial struct {structName}
    {{
        public static {structName} ReadStruct(BinaryReader reader)
        {{
            var result = new {structName}();
");

            foreach (var property in properties) {
                sourceBuilder.AppendLine(TypeReaders.TryGetValue(property.Type, out var readMethod)
                    ? $"            result.{property.Name} = reader.{readMethod};"
                    : $"            result.{property.Name} = {property.Type}.ReadStruct(reader);");
            }

            sourceBuilder.AppendLine(@"
            return result;
        }
    }
}");

            context.AddSource($"Johnny_{structName}_Reader.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
    }
}