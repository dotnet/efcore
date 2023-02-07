// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

public class JsonOwnedAllTypes
{
    public string TestDefaultString { get; set; }
    public string TestMaxLengthString { get; set; }
    public short TestInt16 { get; set; }
    public int TestInt32 { get; set; }
    public long TestInt64 { get; set; }
    public double TestDouble { get; set; }
    public decimal TestDecimal { get; set; }
    public DateTime TestDateTime { get; set; }
    public DateTimeOffset TestDateTimeOffset { get; set; }
    public TimeSpan TestTimeSpan { get; set; }
    public float TestSingle { get; set; }
    public bool TestBoolean { get; set; }
    public byte TestByte { get; set; }
    public Guid TestGuid { get; set; }
    public ushort TestUnsignedInt16 { get; set; }
    public uint TestUnsignedInt32 { get; set; }
    public ulong TestUnsignedInt64 { get; set; }
    public char TestCharacter { get; set; }
    public sbyte TestSignedByte { get; set; }
    public int? TestNullableInt32 { get; set; }
    public JsonEnum TestEnum { get; set; }
    public JsonEnum TestEnumWithIntConverter { get; set; }
    public JsonEnum? TestNullableEnum { get; set; }
    public JsonEnum? TestNullableEnumWithIntConverter { get; set; }
    public JsonEnum? TestNullableEnumWithConverterThatHandlesNulls { get; set; }
}
