namespace Johnny.Tests.Data;

[Johnny]
public partial struct Vector3 {
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
}

[Johnny]
public partial struct NestedStruct {
    public Vector3 Vector { get; set; }
    public Vector3 AnotherVector { get; set; }
    
    public int RandomInteger { get; set; }
}