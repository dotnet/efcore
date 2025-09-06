// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations;

public class SqliteModelDifferTest : MigrationsModelDifferTestBase
{
    [ConditionalFact]
    public void Add_property_with_autoincrement_strategy()
        => Execute(
            _ => { },
            target => target.Entity(
                "Person",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id");
                    x.Property<int>("Id").UseAutoincrement();
                }),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);

                var createTableOperation = Assert.IsType<CreateTableOperation>(upOps[0]);
                var idColumn = createTableOperation.Columns.Single(c => c.Name == "Id");
                Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, idColumn[SqliteAnnotationNames.ValueGenerationStrategy]);

                Assert.IsType<CreateIndexOperation>(upOps[1]);
            });

    [ConditionalFact]
    public void Alter_property_add_autoincrement_strategy()
        => Execute(
            common => common.Entity(
                "Person",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id");
                }),
            source => source.Entity("Person").Property<int>("Id"),
            target => target.Entity("Person").Property<int>("Id").UseAutoincrement(),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var alterColumnOperation = Assert.IsType<AlterColumnOperation>(upOps[0]);
                Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, alterColumnOperation[SqliteAnnotationNames.ValueGenerationStrategy]);
                Assert.Null(alterColumnOperation.OldColumn[SqliteAnnotationNames.ValueGenerationStrategy]);
            });

    [ConditionalFact]
    public void Alter_property_remove_autoincrement_strategy()
        => Execute(
            common => common.Entity(
                "Person",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id");
                }),
            source => source.Entity("Person").Property<int>("Id").UseAutoincrement(),
            target => target.Entity("Person").Property<int>("Id"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var alterColumnOperation = Assert.IsType<AlterColumnOperation>(upOps[0]);
                Assert.Null(alterColumnOperation[SqliteAnnotationNames.ValueGenerationStrategy]);
                Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, alterColumnOperation.OldColumn[SqliteAnnotationNames.ValueGenerationStrategy]);
            });

    [ConditionalFact]
    public void Autoincrement_with_value_converter_generates_consistent_migrations()
        => Execute(
            common => common.Entity<ProductWithConverter>(
                x =>
                {
                    x.Property(e => e.Id).HasConversion(
                        v => v.Value,
                        v => new ProductId(v));
                    x.HasKey(e => e.Id);
                }),
            source => source.Entity<ProductWithConverter>().Property(e => e.Id),
            target => target.Entity<ProductWithConverter>().Property(e => e.Id).UseAutoincrement(),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var alterColumnOperation = Assert.IsType<AlterColumnOperation>(upOps[0]);
                Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, alterColumnOperation[SqliteAnnotationNames.ValueGenerationStrategy]);
                Assert.Null(alterColumnOperation.OldColumn[SqliteAnnotationNames.ValueGenerationStrategy]);
            });

    [ConditionalFact]
    public void No_repeated_alter_column_for_autoincrement_with_converter()
        => Execute(
            common => common.Entity<ProductWithConverter>(
                x =>
                {
                    x.Property(e => e.Id).HasConversion(
                        v => v.Value,
                        v => new ProductId(v));
                    x.HasKey(e => e.Id);
                    x.Property(e => e.Id).UseAutoincrement();
                }),
            source => { },
            target => { },
            upOps =>
            {
                // Should have no operations since the models are the same
                Assert.Empty(upOps);
            });

    [ConditionalFact]
    public void Noop_when_changing_to_autoincrement_property_with_converter()
        => Execute(
            source => source.Entity(
                "Product",
                x =>
                {
                    x.Property<int>("Id");
                    x.HasKey("Id");
                }),
            target => target.Entity<ProductWithConverter>(
                x =>
                {
                    x.Property(e => e.Id).HasConversion(
                        v => v.Value,
                        v => new ProductId(v));
                    x.HasKey(e => e.Id);
                    x.Property(e => e.Id).UseAutoincrement();
                }),
            upOps =>
            {
                // Should have no operations since both have autoincrement strategy
                Assert.Empty(upOps);
            });

    protected override TestHelpers TestHelpers => SqliteTestHelpers.Instance;

    // Test entities
    public record struct ProductId(int Value);

    public class ProductWithConverter
    {
        public ProductId Id { get; set; }
        public required string Name { get; set; }
    }
}