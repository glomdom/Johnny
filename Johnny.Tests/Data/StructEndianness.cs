namespace Johnny.Tests.Data;

[Johnny]
public partial struct StructEndianness {
    [Johnny(endian: Endianness.Little)]
    public short Integer { get; set; }
}