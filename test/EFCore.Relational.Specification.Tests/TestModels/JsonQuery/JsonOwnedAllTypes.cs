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

    public string[] TestDefaultStringCollection { get; set; }
    public List<string> TestMaxLengthStringCollection { get; set; }
    public IList<short> TestInt16Collection { get; set; }
    public int[] TestInt32Collection { get; set; }
    public List<long> TestInt64Collection { get; set; }
    public IList<double> TestDoubleCollection { get; set; }
    public decimal[] TestDecimalCollection { get; set; }
    public List<DateTime> TestDateTimeCollection { get; set; }
    public IList<DateTimeOffset> TestDateTimeOffsetCollection { get; set; }
    public TimeSpan[] TestTimeSpanCollection { get; set; }
    public List<float> TestSingleCollection { get; set; }
    public IList<bool> TestBooleanCollection { get; set; }
    public byte[] TestByteCollection { get; set; }
    public List<Guid> TestGuidCollection { get; set; }
    public IList<ushort> TestUnsignedInt16Collection { get; set; }
    public uint[] TestUnsignedInt32Collection { get; set; }
    public List<ulong> TestUnsignedInt64Collection { get; set; }
    public IList<char> TestCharacterCollection { get; set; }
    public sbyte[] TestSignedByteCollection { get; set; }
    public List<int?> TestNullableInt32Collection { get; set; }
    public IList<JsonEnum> TestEnumCollection { get; set; }
    public JsonEnum[] TestEnumWithIntConverterCollection { get; set; }
    public List<JsonEnum?> TestNullableEnumCollection { get; set; }
    public IList<JsonEnum?> TestNullableEnumWithIntConverterCollection { get; set; }
    public JsonEnum?[] TestNullableEnumWithConverterThatHandlesNullsCollection { get; set; }
}
