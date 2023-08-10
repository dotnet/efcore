// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.SqlServer.Types;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer;

public class TypeMappingTests
{
    [ConditionalFact]
    public void Maps_int_column()
    {
        var mapping = CreateMapper().FindMapping(
            new RelationalTypeMappingInfo(
                storeTypeName: "int",
                storeTypeNameBase: "int",
                unicode: null,
                size: null,
                precision: null,
                scale: null));

        Assert.Null(mapping);
    }

    [ConditionalFact]
    public void Maps_hierarchyid_column()
    {
        var mapping = CreateMapper().FindMapping(
            new RelationalTypeMappingInfo(
                storeTypeName: "hierarchyid",
                storeTypeNameBase: "hierarchyid",
                unicode: null,
                size: null,
                precision: null,
                scale: null));

        AssertMapping<HierarchyId>(mapping);

        Assert.Same(SqlServerJsonHierarchyIdReaderWriter.Instance, mapping!.JsonValueReaderWriter);
    }

    [ConditionalFact]
    public void Convert_HierarchyId_to_and_from_JSON()
        => Convert_to_and_from_JSON(
            SqlServerJsonHierarchyIdReaderWriter.Instance,
            HierarchyId.GetRoot(), new HierarchyId("/1/"), new HierarchyId("/1/3/"),
            """{"Prop1":"/","Prop2":"/1/","Prop3":"/1/3/"}""");

    [ConditionalFact]
    public void Convert_SqlHierarchyId_to_and_from_JSON()
        => Convert_to_and_from_JSON(
            SqlServerJsonSqlHierarchyIdReaderWriter.Instance,
            SqlHierarchyId.GetRoot(), SqlHierarchyId.Parse("/1/"), SqlHierarchyId.Parse("/1/3/"),
            """{"Prop1":"/","Prop2":"/1/","Prop3":"/1/3/"}""");

    private void Convert_to_and_from_JSON<TValue>(
        JsonValueReaderWriter<TValue> jsonReaderWriter,
        TValue one,
        TValue two,
        TValue three,
        string json)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        writer.WriteStartObject();
        writer.WritePropertyName("Prop1");
        jsonReaderWriter.ToJson(writer, one);
        writer.WritePropertyName("Prop2");
        jsonReaderWriter.ToJson(writer, two);
        writer.WritePropertyName("Prop3");
        jsonReaderWriter.ToJson(writer, three);
        writer.WriteEndObject();
        writer.Flush();

        var buffer = stream.ToArray();

        var actual = Encoding.UTF8.GetString(buffer);

        Assert.Equal(json, actual);

        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(buffer), null);
        readerManager.MoveNext();
        readerManager.MoveNext();
        readerManager.MoveNext();
        Assert.Equal(one, jsonReaderWriter.FromJson(ref readerManager));
        readerManager.MoveNext();
        readerManager.MoveNext();
        Assert.Equal(two, jsonReaderWriter.FromJson(ref readerManager));
        readerManager.MoveNext();
        readerManager.MoveNext();
        Assert.Equal(three, jsonReaderWriter.FromJson(ref readerManager));
    }

    private static void AssertMapping<T>(
        RelationalTypeMapping mapping)
        => AssertMapping(typeof(T), mapping);

    private static void AssertMapping(
        Type type,
        RelationalTypeMapping mapping)
        => Assert.Same(type, mapping.ClrType);

    private static IRelationalTypeMappingSourcePlugin CreateMapper()
        => new SqlServerHierarchyIdTypeMappingSourcePlugin();
}
