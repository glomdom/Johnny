namespace Johnny.Data;

public static class Attribute {
    public const string DisplayText = "Johnny.JohnnyAttribute";
    
    public const string Source = """
namespace Johnny;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Property)]
public sealed class JohnnyAttribute : Attribute { }
""";
}