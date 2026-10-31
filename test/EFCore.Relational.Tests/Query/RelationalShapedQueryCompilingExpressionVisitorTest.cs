// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Query;

public class RelationalShapedQueryCompilingExpressionVisitorTest
{
    [Fact]
    public void Materializes_primitive_collection_with_native_collection_mapping()
    {
        using var context = CreateContext(
            ["Id", "Enums"],
            [1, new[] { 1, 2 }]);

        var entity = context.Entities.Single();

        Assert.Equal([TestEnum.One, TestEnum.Two], entity.Enums);
    }

    [Fact]
    public void Projects_primitive_collection_with_native_collection_mapping()
    {
        using var context = CreateContext(
            ["Enums"],
            [new[] { 1, 2 }]);

        var enums = context.Entities.Select(e => e.Enums).Single();

        Assert.Equal([TestEnum.One, TestEnum.Two], enums);
    }

    [Fact]
    public void Native_collection_mapping_default_provider_value_is_empty_collection()
        => Assert.Empty(Assert.IsType<int[]>(NativeCollectionTypeMapping.Default.GetDefaultProviderValue()));

    private static NativeCollectionContext CreateContext(string[] columnNames, object[] row)
    {
        var connection = new FakeDbConnection(
            "Database=Fake",
            new FakeCommandExecutor(
                executeReader: (_, _) => new FakeDbDataReader(columnNames, [row])));

        return new NativeCollectionContext(
            new DbContextOptionsBuilder<NativeCollectionContext>()
                .UseFakeRelational(connection)
                .ReplaceService<IRelationalTypeMappingSource, NativeCollectionTypeMappingSource>()
                .Options);
    }

    private sealed class NativeCollectionContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<NativeCollectionEntity> Entities
            => Set<NativeCollectionEntity>();
    }

    private sealed class NativeCollectionEntity
    {
        public int Id { get; set; }
        public TestEnum[] Enums { get; set; } = [];
    }

    private enum TestEnum
    {
        One = 1,
        Two
    }

    private sealed class NativeCollectionTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies)
        : TestRelationalTypeMappingSource(dependencies, relationalDependencies)
    {
        protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => mappingInfo.ClrType == typeof(TestEnum[])
                ? NativeCollectionTypeMapping.Default
                : base.FindMapping(mappingInfo);
    }

    private sealed class NativeCollectionTypeMapping : RelationalTypeMapping
    {
        public static NativeCollectionTypeMapping Default { get; } = new();

        private NativeCollectionTypeMapping()
            : base(CreateParameters())
        {
        }

        private NativeCollectionTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        private static RelationalTypeMappingParameters CreateParameters()
        {
            var elementConverter = new EnumToNumberConverter<TestEnum, int>();
            var elementMapping = new IntTypeMapping("int").WithComposedConverter(elementConverter);

            return new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(TestEnum[]),
                    new ValueConverter<TestEnum[], int[]>(
                        values => values.Select(value => (int)value).ToArray(),
                        values => values.Select(value => (TestEnum)value).ToArray()),
                    new ValueComparer<TestEnum[]>(
                        (left, right) => left.SequenceEqual(right),
                        values => values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value)),
                        values => values.ToArray()),
                    elementMapping: elementMapping,
                    jsonValueReaderWriter: new JsonCollectionOfStructsReaderWriter<TestEnum[], TestEnum>(
                        new JsonConvertedValueReaderWriter<TestEnum, int>(
                            JsonInt32ReaderWriter.Instance,
                            elementConverter))),
                "int[]");
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NativeCollectionTypeMapping(parameters);
    }
}
