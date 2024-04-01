// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore;

public abstract class BadDataJsonDeserializationTestBase
{
    [ConditionalTheory]
    [InlineData("""{"Prop"}""")]
    [InlineData("""{"Prop"}:127}""")]
    [InlineData("""{"Prop":"X"}""")]
    [InlineData("""{"Prop":}""")]
    public virtual void Throws_for_bad_sbyte_JSON_values(string json)
        => Throws_for_bad_JSON_value<Int8Type, sbyte>(nameof(Int8Type.Int8), json);

    protected class Int8Type
    {
        public sbyte Int8 { get; set; }
    }

    [ConditionalTheory]
    [InlineData("""{"Prop"}""")]
    [InlineData("""{"Prop"}:127}""")]
    [InlineData("""{"Prop":"X"}""")]
    [InlineData("""{"Prop":}""")]
    public virtual void Throws_for_bad_nullable_long_JSON_values(string json)
        => Throws_for_bad_JSON_value<NullableInt64Type, long?>(nameof(NullableInt64Type.Int64), json);

    protected class NullableInt64Type
    {
        public long? Int64 { get; set; }
    }

    [ConditionalTheory]
    [InlineData("""{"Prop":{"type""Point","coordinates":[2.0,4.0]}}""")]
    [InlineData("""{"Prop":{"type":["Point","coordinates":[2.0,4.0]}}""")]
    [InlineData("""{"Prop":{"type":"Point","coordinates":[2.0,,4.0]}}""")]
    [InlineData("""{"Prop":[{"type":"Point","coordinates":[2.0,4.0]}]}""")]
    [InlineData("""{"Prop":1}""")]
    [InlineData("""{"Prop":true}""")]
    [InlineData("""{"Prop":false}""")]
    [InlineData("""{"Prop":"X"}""")]
    public virtual void Throws_for_bad_point_as_GeoJson(string json)
        => Throws_for_bad_JSON_property_value<PointType, Point>(
            b => b.Metadata.SetJsonValueReaderWriterType(typeof(JsonTypesTestBase.JsonGeoJsonReaderWriter)),
            nameof(PointType.Point),
            json);

    public class PointType
    {
        public Point? Point { get; set; }
    }

    [ConditionalTheory]
    [InlineData("""{"Prop":[-128,[0,127]]}""")]
    [InlineData("""{"Prop":[-128,{"P":127}]}""")]
    [InlineData("""{"Prop":[-128,],23]}""")]
    [InlineData("""{"Prop":[-128,},23]}""")]
    [InlineData("""{"Prop":[-128,,23]}""")]
    public virtual void Throws_for_bad_collection_of_sbyte_JSON_values(string json)
        => Throws_for_bad_JSON_value<Int8CollectionType, List<sbyte>>(
            nameof(Int8CollectionType.Int8),
            json,
            mappedCollection: true);

    protected class Int8CollectionType
    {
        public sbyte[] Int8 { get; set; } = null!;
    }

    [ConditionalTheory]
    [InlineData("""{"Prop":[-128,[0,127]]}""")]
    [InlineData("""{"Prop":[-128,{"P":127}]}""")]
    [InlineData("""{"Prop":[-128,],23]}""")]
    [InlineData("""{"Prop":[-128,},23]}""")]
    [InlineData("""{"Prop":[-128,,23]}""")]
    public virtual void Throws_for_bad_collection_of_nullable_long_JSON_values(string json)
        => Throws_for_bad_JSON_value<NullableInt64CollectionType, List<long?>>(
            nameof(NullableInt64CollectionType.Int64),
            json,
            mappedCollection: true);

    protected class NullableInt64CollectionType
    {
        public IList<long?> Int64 { get; set; } = null!;
    }

    protected virtual void Throws_for_bad_JSON_value<TEntity, TModel>(
        string propertyName,
        string json,
        bool mappedCollection = false,
        object? existingObject = null)
        where TEntity : class
    {
        if (mappedCollection)
        {
            Throws_for_bad_JSON_value<TEntity, TModel>(
                b => b.Entity<TEntity>().HasNoKey().PrimitiveCollection(propertyName),
                null,
                propertyName,
                json,
                mappedCollection,
                existingObject);
        }
        else
        {
            Throws_for_bad_JSON_value<TEntity, TModel>(
                b => b.Entity<TEntity>().HasNoKey().Property(propertyName),
                null,
                propertyName,
                json,
                mappedCollection,
                existingObject);
        }
    }

    protected virtual void Throws_for_bad_JSON_property_value<TEntity, TModel>(
        Action<PropertyBuilder> buildProperty,
        string propertyName,
        string json,
        object? existingObject = null)
        where TEntity : class
        => Throws_for_bad_JSON_value<TEntity, TModel>(
            b => buildProperty(b.Entity<TEntity>().HasNoKey().Property(propertyName)),
            null,
            propertyName,
            json,
            mappedCollection: false,
            existingObject);

    protected virtual void Throws_for_bad_JSON_value<TEntity, TModel>(
        Action<ModelBuilder> buildModel,
        Action<ModelConfigurationBuilder>? configureConventions,
        string propertyName,
        string json,
        bool mappedCollection = false,
        object? existingObject = null)
        where TEntity : class
    {
        using var context = new SingleTypeDbContext(OnConfiguring, buildModel, configureConventions);
        var property = context.Model.FindEntityType(typeof(TEntity))!.GetProperty(propertyName);

        var jsonReaderWriter = property.GetJsonValueReaderWriter()
            ?? property.GetTypeMapping().JsonValueReaderWriter!;

        var buffer = Encoding.UTF8.GetBytes(json);
        var readerManager = new Utf8JsonReaderManager(new JsonReaderData(buffer), null);

        try
        {
            Assert.Equal(JsonTokenType.StartObject, readerManager.MoveNext());
            Assert.Equal(JsonTokenType.PropertyName, readerManager.MoveNext());
            readerManager.MoveNext();
            jsonReaderWriter.FromJson(ref readerManager, existingObject);
            Assert.Fail("Expected JSON deserialization to throw.");
        }
        catch (Exception e)
        {
            Assert.True(e is InvalidOperationException || e is JsonException || e is Newtonsoft.Json.JsonException);
        }
    }

    protected class SingleTypeDbContext(
            Action<DbContextOptionsBuilder> buildOptions,
            Action<ModelBuilder> buildModel,
            Action<ModelConfigurationBuilder>? configureConventions = null)
        : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => buildOptions(optionsBuilder.ReplaceService<IModelCacheKeyFactory, DegenerateCacheKeyFactory>());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => buildModel(modelBuilder);

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
            => configureConventions?.Invoke(configurationBuilder);

        private class DegenerateCacheKeyFactory : IModelCacheKeyFactory
        {
            private static int _value;

            public object Create(DbContext context, bool designTime)
                => _value++;
        }
    }

    protected virtual void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.ConfigureWarnings(
            w => w.Ignore(
                CoreEventId.MappedEntityTypeIgnoredWarning,
                CoreEventId.MappedPropertyIgnoredWarning,
                CoreEventId.MappedNavigationIgnoredWarning));
}
