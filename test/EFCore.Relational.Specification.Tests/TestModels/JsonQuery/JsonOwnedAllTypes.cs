// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonOwnedAllTypes
{
    private List<long> _testInt64CollectionX = [];
    private IList<double> _testDoubleCollectionX = new List<double>();
    private List<float> _testSingleCollectionX = [1.1f, 1.2f];
    private IList<bool> _testBooleanCollectionX = new List<bool> { true };
    private ObservableCollection<char> _testCharacterCollectionX = [];
    private ObservableCollection<int?> _testNullableInt32CollectionX = [99];
    private Collection<JsonEnum?> _testNullableEnumCollectionX = [];
    private Collection<JsonEnum?> _testNullableEnumWithIntConverterCollectionX = [JsonEnum.Three];

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
    public DateOnly TestDateOnly { get; set; }
    public TimeOnly TestTimeOnly { get; set; }
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

    public List<List<long>> TestInt64CollectionCollection { get; set; }  = [];
    public List<double[]> TestDoubleCollectionCollection { get; set; }  = new();
    public List<float[]> TestSingleCollectionCollection { get; set; }  = new([([1.1f, 1.2f])]);
    public bool[][] TestBooleanCollectionCollection { get; set; }  = [];
    public ObservableCollection<IReadOnlyCollection<char>> TestCharacterCollectionCollection { get; set; }  = [];

    public string[] TestDefaultStringCollection { get; set; }
    public ReadOnlyCollection<string> TestMaxLengthStringCollection { get; set; }
    public IReadOnlyList<short> TestInt16Collection { get; set; }

    public int[] TestInt32Collection { get; set; } = [];

    public List<long> TestInt64Collection
    {
        get => _testInt64CollectionX;
        set
        {
            _testInt64CollectionX = value;
            NewCollectionSet = true;
        }
    }

    public IList<double> TestDoubleCollection
    {
        get => _testDoubleCollectionX;
        set
        {
            _testDoubleCollectionX = value;
            NewCollectionSet = true;
        }
    }

    public decimal[] TestDecimalCollection { get; set; }
    public List<DateTime> TestDateTimeCollection { get; set; }
    public IList<DateTimeOffset> TestDateTimeOffsetCollection { get; set; }
    public TimeSpan[] TestTimeSpanCollection { get; set; } = [new(1, 1, 1)];
    public DateOnly[] TestDateOnlyCollection { get; set; }
    public TimeOnly[] TestTimeOnlyCollection { get; set; }

    public string[][] TestDefaultStringCollectionCollection { get; init; }
    public List<ReadOnlyCollection<string>> TestMaxLengthStringCollectionCollection { get; init; }
    public IList<IReadOnlyList<short>> TestInt16CollectionCollection { get; set; }

    public int[][] TestInt32CollectionCollection { get; set; } = [];

    public ObservableCollection<int?[]> TestNullableInt32CollectionCollection { get; set; } = [[99]];
    public ICollection<List<Collection<JsonEnum?>>> TestNullableEnumCollectionCollection { get; set; } = [];
    public JsonEnum?[][][] TestNullableEnumWithIntConverterCollectionCollection { get; set; } = [[[JsonEnum.Three]]];

    public List<float> TestSingleCollection
    {
        get => _testSingleCollectionX;
        set
        {
            _testSingleCollectionX = value;
            NewCollectionSet = true;
        }
    }

    public IList<bool> TestBooleanCollection
    {
        get => _testBooleanCollectionX;
        set
        {
            _testBooleanCollectionX = value;
            NewCollectionSet = true;
        }
    }

    public byte[] TestByteCollection { get; set; }
    public List<Guid> TestGuidCollection { get; set; }
    public IList<ushort> TestUnsignedInt16Collection { get; set; }
    public uint[] TestUnsignedInt32Collection { get; set; }
    public ObservableCollection<ulong> TestUnsignedInt64Collection { get; set; }

    public ObservableCollection<char> TestCharacterCollection
    {
        get => _testCharacterCollectionX;
        set
        {
            _testCharacterCollectionX = value;
            NewCollectionSet = true;
        }
    }

    public sbyte[] TestSignedByteCollection { get; set; }

    public ObservableCollection<int?> TestNullableInt32Collection
    {
        get => _testNullableInt32CollectionX;
        set
        {
            _testNullableInt32CollectionX = value;
            NewCollectionSet = true;
        }
    }

    public IList<JsonEnum> TestEnumCollection { get; set; }
    public JsonEnum[] TestEnumWithIntConverterCollection { get; set; }

    public Collection<JsonEnum?> TestNullableEnumCollection
    {
        get => _testNullableEnumCollectionX;
        set
        {
            NewCollectionSet = true;
            _testNullableEnumCollectionX = value;
        }
    }

    public Collection<JsonEnum?> TestNullableEnumWithIntConverterCollection
    {
        get => _testNullableEnumWithIntConverterCollectionX;
        set
        {
            _testNullableEnumWithIntConverterCollectionX = value;
            NewCollectionSet = true;
        }
    }

    public JsonEnum?[] TestNullableEnumWithConverterThatHandlesNullsCollection { get; set; }

    [NotMapped]
    public bool NewCollectionSet { get; private set; }
}
