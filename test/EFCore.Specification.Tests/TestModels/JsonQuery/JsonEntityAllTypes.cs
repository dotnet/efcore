// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

#nullable disable

public class JsonEntityAllTypes
{
    private ObservableCollection<int?> _testNullableInt32CollectionX = [99];
    private Collection<JsonEnum?> _testNullableEnumCollectionX = [];
    private Collection<JsonEnum?> _testNullableEnumWithIntConverterCollectionX = [JsonEnum.Three];

    public List<List<long>> TestInt64CollectionCollection { get; set; }  = [];
    public IReadOnlyList<double[]> TestDoubleCollectionCollection { get; set; }  = new List<double[]>();
    public List<float>[] TestSingleCollectionCollection { get; set; }  = [[1.1f, 1.2f]];
    public bool[][] TestBooleanCollectionCollection { get; set; }  = [];
    public ObservableCollection<ReadOnlyCollection<char>> TestCharacterCollectionCollection { get; set; }  = [];

    public int Id { get; set; }
    public JsonOwnedAllTypes Reference { get; init; }
    public List<JsonOwnedAllTypes> Collection { get; init; }

    public string[] TestDefaultStringCollection { get; init; }
    public List<string> TestMaxLengthStringCollection { get; init; }
    public IList<short> TestInt16Collection { get; set; }

    public string[][] TestDefaultStringCollectionCollection { get; init; }
    public List<IReadOnlyList<string>> TestMaxLengthStringCollectionCollection { get; init; }
    public IReadOnlyList<IReadOnlyList<short>> TestInt16CollectionCollection { get; set; }

    public int[] TestInt32Collection { get; set; } = [];

    public int[][] TestInt32CollectionCollection { get; set; } = [];

    public decimal[] TestDecimalCollection { get; set; }
    public List<DateTime> TestDateTimeCollection { get; set; }
    public IList<DateTimeOffset> TestDateTimeOffsetCollection { get; set; }
    public TimeSpan[] TestTimeSpanCollection { get; set; } = [new(1, 1, 1)];

    public ReadOnlyCollection<long> TestInt64Collection { get; set; } = new ReadOnlyCollection<long>([]);
    public IList<double> TestDoubleCollection { get; set; } = new List<double>();
    public IReadOnlyList<float> TestSingleCollection { get; set; } = [1.1f, 1.2f];
    public IList<bool> TestBooleanCollection { get; set; } = new List<bool> { true };
    public ObservableCollection<char> TestCharacterCollection { get; set; } = [];
    public byte[] TestByteCollection { get; set; }

    [Required]
    public ReadOnlyCollection<Guid> TestGuidCollection { get; set; }

    public IList<ushort> TestUnsignedInt16Collection { get; set; }
    public uint[] TestUnsignedInt32Collection { get; set; }
    public ObservableCollection<ulong> TestUnsignedInt64Collection { get; set; }

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

    public ObservableCollection<int?[]> TestNullableInt32CollectionCollection { get; set; } = [[99]];
    public ICollection<List<Collection<JsonEnum?>>> TestNullableEnumCollectionCollection { get; set; } = [];
    public JsonEnum?[][][] TestNullableEnumWithIntConverterCollectionCollection { get; set; } = [[[JsonEnum.Three]]];
    public JsonEnum?[] TestNullableEnumWithConverterThatHandlesNullsCollection { get; set; }

    [NotMapped]
    public bool NewCollectionSet { get; private set; }
}
