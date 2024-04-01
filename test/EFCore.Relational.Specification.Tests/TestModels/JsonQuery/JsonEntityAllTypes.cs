// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonEntityAllTypes
{
    private List<long> _testInt64CollectionX = [];
    private IList<double> _testDoubleCollectionX = new List<double>();
    private List<float> _testSingleCollectionX = [1.1f, 1.2f];
    private IList<bool> _testBooleanCollectionX = new List<bool> { true };
    private ObservableCollection<char> _testCharacterCollectionX = [];
    private ObservableCollection<int?> _testNullableInt32CollectionX = [99];
    private Collection<JsonEnum?> _testNullableEnumCollectionX = [];
    private Collection<JsonEnum?> _testNullableEnumWithIntConverterCollectionX = [JsonEnum.Three];

    public int Id { get; set; }
    public JsonOwnedAllTypes Reference { get; init; }
    public List<JsonOwnedAllTypes> Collection { get; init; }

    public string[] TestDefaultStringCollection { get; init; }
    public List<string> TestMaxLengthStringCollection { get; init; }
    public IList<short> TestInt16Collection { get; set; }

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

    [Required]
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
