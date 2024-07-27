namespace Johnny.Data;

public static class Attribute {
    public const string DisplayText = "Johnny.JohnnyAttribute";
    
    public const string Source = """
namespace Johnny;

public enum Endianness {
    Little,
    Big
}

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property)]
public sealed class JohnnyAttribute : Attribute {
    public Endianness Endian { get; }
    
    public JohnnyAttribute(Endianness endian = Endianness.Little) {
        Endian = endian;
    }
}
""";
}