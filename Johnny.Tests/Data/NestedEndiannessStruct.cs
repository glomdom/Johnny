namespace Johnny.Tests.Data;

[Johnny]
public partial struct NestedEndiannessStruct {
    public StructEndianness EndianStruct { get; set; }
    
    public Vector3 RandomVector { get; set; }
    public int RandomNumber { get; set; }
}