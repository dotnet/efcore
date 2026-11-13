// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

public class JsonCollectionReaderWriterTest
{
    public static TheoryData<JsonValueReaderWriter, string> CollectionReaderWriters
        => new()
        {
            { new JsonCollectionOfStructsReaderWriter<List<int>, int>(JsonInt32ReaderWriter.Instance), "int" },
            { new JsonCollectionOfNullableStructsReaderWriter<List<int?>, int>(JsonInt32ReaderWriter.Instance), "int" },
            { new JsonCollectionOfReferencesReaderWriter<List<string>, string>(JsonStringReaderWriter.Instance), "string" }
        };

    [Theory, MemberData(nameof(CollectionReaderWriters))]
    public void Reading_non_array_throws(JsonValueReaderWriter readerWriter, string elementType)
        => Assert.Equal(
            CoreStrings.JsonCollectionExpectedArray(elementType, nameof(JsonTokenType.StartObject)),
            Assert.Throws<InvalidOperationException>(() => readerWriter.FromJsonString("{}")).Message);

    [Fact]
    public void Reading_object_element_in_collection_of_structs_throws()
        => Assert.Equal(
            CoreStrings.JsonCollectionInvalidElement("int", nameof(JsonTokenType.StartObject)),
            Assert.Throws<InvalidOperationException>(
                () => new JsonCollectionOfStructsReaderWriter<List<int>, int>(JsonInt32ReaderWriter.Instance)
                    .FromJsonString("[1,{}]")).Message);

    [Fact]
    public void Reading_array_element_in_collection_of_nullable_structs_throws()
        => Assert.Equal(
            CoreStrings.JsonCollectionInvalidElement("int", nameof(JsonTokenType.StartArray)),
            Assert.Throws<InvalidOperationException>(
                () => new JsonCollectionOfNullableStructsReaderWriter<List<int?>, int>(JsonInt32ReaderWriter.Instance)
                    .FromJsonString("[null,[1]]")).Message);
}
