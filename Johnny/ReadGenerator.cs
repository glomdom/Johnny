using System;
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
            .Select(p => (
                Type: p.Type.ToString(),
                Name: p.Identifier.Text,
                Attributes: p.AttributeLists
                    .SelectMany(attributesList => attributesList.Attributes
                        .Where(attribute => attribute.Name.ToString() == "Johnny"))))
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
            var foundPrimitiveMethod = TypeReaders.TryGetValue(property.Type, out var readMethod);
            string method;

            if (foundPrimitiveMethod) {
                method = $"reader.{readMethod};";

                CodegenHelpers.HandleAttributes(ref method, property.Attributes.ToList());
                method = $"result.{property.Name} = {method}";
            } else {
                method = $"result.{property.Name} = {property.Type}.ReadStruct(reader);";
            }

            sourceBuilder.AppendLine(method);
        }

        sourceBuilder.AppendLine($@"
            return result;
        }}

        private static T ReverseBytes<T>(T value)
        {{
            if (typeof(T) == typeof(short) || typeof(T) == typeof(ushort))
            {{
                var bytes = BitConverter.GetBytes((dynamic)value);
                Array.Reverse(bytes);
                return (T)(dynamic)BitConverter.ToInt16(bytes, 0);
            }}

            if (typeof(T) == typeof(int) || typeof(T) == typeof(uint))
            {{
                var bytes = BitConverter.GetBytes((dynamic)value);
                Array.Reverse(bytes);
                return (T)(dynamic)BitConverter.ToInt32(bytes, 0);
            }}

            if (typeof(T) == typeof(long) || typeof(T) == typeof(ulong))
            {{
                var bytes = BitConverter.GetBytes((dynamic)value);
                Array.Reverse(bytes);
                return (T)(dynamic)BitConverter.ToInt64(bytes, 0);
            }}

            if (typeof(T) == typeof(float))
            {{
                var bytes = BitConverter.GetBytes((dynamic)value);
                Array.Reverse(bytes);
                return (T)(dynamic)BitConverter.ToSingle(bytes, 0);
            }}

            if (typeof(T) == typeof(double))
            {{
                var bytes = BitConverter.GetBytes((dynamic)value);
                Array.Reverse(bytes);
                return (T)(dynamic)BitConverter.ToDouble(bytes, 0);
            }}

            throw new NotSupportedException($""Type {{typeof(T)}} not supported for byte reversal"");
        }}
    }}
}}");

        context.AddSource($"Johnny_{structName}_Reader.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
}