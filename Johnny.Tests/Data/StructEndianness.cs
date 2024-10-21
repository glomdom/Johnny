namespace Johnny.Tests.Data;

[Johnny]
public partial struct StructEndianness {
    [Johnny(endian: Endianness.Little)]
    public short LittleEndianInteger { get; set; }
    
    public short BigEndianInteger { get; set; }
}