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
        context.RegisterPostInitializationOutput(
            ctx =>
                ctx.AddSource("JohnnyAttribute.g.cs", Data.Attribute.Source)
        );

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
        var properties = GetStructProperties(structDecl);

        var sb = new StringBuilder();
        sb.AppendLine("using System.IO;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName} {{");
        sb.AppendLine($"\tpublic partial struct {structName} {{");
        sb.AppendLine($"\t\tpublic static {structName} ReadStruct(BinaryReader reader) {{");
        sb.AppendLine($"\t\t\tvar result = new {structName}();");

        foreach (var property in properties) {
            var readStatement = GenerateReadStatement(property);
            sb.AppendLine(readStatement);
        }

        sb.AppendLine("\t\t\treturn result;");
        sb.AppendLine("\t\t}");
        sb.AppendLine();
        sb.Append(GenerateReverseBytesMethod());
        sb.AppendLine("\t}");
        sb.AppendLine("}");

        context.AddSource($"Johnny_{structName}_Reader.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private static IEnumerable<(string Type, string Name, List<AttributeSyntax> Attributes)> GetStructProperties(StructDeclarationSyntax structDecl) {
        return structDecl.Members
            .OfType<PropertyDeclarationSyntax>()
            .Select(
                p => (
                    Type: p.Type.ToString(),
                    Name: p.Identifier.Text,
                    Attributes: p.AttributeLists.SelectMany(a => a.Attributes).ToList()
                )
            );
    }

    private static string GenerateReadStatement((string Type, string Name, List<AttributeSyntax> Attributes) property) {
        if (!TypeReaders.TryGetValue(property.Type, out var readMethod)) {
            return $"\t\t\tresult.{property.Name} = {property.Type}.ReadStruct(reader);";
        }

        var method = $"reader.{readMethod};";
        CodegenHelpers.HandleAttributes(ref method, property.Attributes);

        return $"\t\t\tresult.{property.Name} = {method}";
    }

    private static string GenerateReverseBytesMethod() {
        var sb = new StringBuilder();
        sb.AppendLine("\t\tprivate static T ReverseBytes<T>(T value) {");
        sb.AppendLine("\t\t\tvar bytes = BitConverter.GetBytes((dynamic)value);");
        sb.AppendLine("\t\t\tArray.Reverse(bytes);");
        sb.AppendLine();
        sb.AppendLine("\t\t\treturn typeof(T) switch {");
        sb.AppendLine("\t\t\t\tType t when t == typeof(short) || t == typeof(ushort) => (T)(dynamic)BitConverter.ToInt16(bytes, 0),");
        sb.AppendLine("\t\t\t\tType t when t == typeof(int) || t == typeof(uint) => (T)(dynamic)BitConverter.ToInt32(bytes, 0),");
        sb.AppendLine("\t\t\t\tType t when t == typeof(long) || t == typeof(ulong) => (T)(dynamic)BitConverter.ToInt64(bytes, 0),");
        sb.AppendLine("\t\t\t\tType t when t == typeof(float) => (T)(dynamic)BitConverter.ToSingle(bytes, 0),");
        sb.AppendLine("\t\t\t\tType t when t == typeof(double) => (T)(dynamic)BitConverter.ToDouble(bytes, 0),");
        sb.AppendLine("\t\t\t\t" + """_ => throw new NotSupportedException($"Type {typeof(T)} not supported for byte reversing.")""");
        sb.AppendLine("\t\t\t};");
        sb.AppendLine("\t\t}");

        return sb.ToString();
    }
}