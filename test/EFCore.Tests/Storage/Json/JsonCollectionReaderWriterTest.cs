// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Storage;

public class JsonCollectionReaderWriterTest
{
    [Fact]
    public void Reads_JSON_null_as_null_for_collection_of_references()
    {
        var readerWriter = new JsonCollectionOfReferencesReaderWriter<List<string>, string>(JsonStringReaderWriter.Instance);

        Assert.Null(readerWriter.FromJsonString("null"));
        Assert.Equal(["a", "b"], (IEnumerable<string>)readerWriter.FromJsonString("""["a","b"]"""));
    }

    [Fact]
    public void Reads_JSON_null_as_null_for_read_only_collection_of_references()
    {
        var readerWriter = new JsonCollectionOfReferencesReaderWriter<ReadOnlyCollection<string>, string>(
            JsonStringReaderWriter.Instance);

        Assert.Null(readerWriter.FromJsonString("null"));
    }

    [Fact]
    public void Reads_JSON_null_as_null_for_array_of_references()
    {
        var readerWriter = new JsonCollectionOfReferencesReaderWriter<string[], string>(JsonStringReaderWriter.Instance);

        Assert.Null(readerWriter.FromJsonString("null"));
    }

    [Fact]
    public void Reads_JSON_null_as_null_for_collection_of_structs()
    {
        var readerWriter = new JsonCollectionOfStructsReaderWriter<List<int>, int>(JsonInt32ReaderWriter.Instance);

        Assert.Null(readerWriter.FromJsonString("null"));
        Assert.Equal([1, 2], (IEnumerable<int>)readerWriter.FromJsonString("[1,2]"));
    }

    [Fact]
    public void Reads_JSON_null_as_null_for_collection_of_nullable_structs()
    {
        var readerWriter = new JsonCollectionOfNullableStructsReaderWriter<List<int?>, int>(JsonInt32ReaderWriter.Instance);

        Assert.Null(readerWriter.FromJsonString("null"));
        Assert.Equal([1, null, 2], (IEnumerable<int?>)readerWriter.FromJsonString("[1,null,2]"));
    }
}
