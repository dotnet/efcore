// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

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
                Assert.Equal(1, upOps.Count);

                var createTableOperation = Assert.IsType<CreateTableOperation>(upOps[0]);
                var idColumn = createTableOperation.Columns.Single(c => c.Name == "Id");
                Assert.Equal(true, idColumn[SqliteAnnotationNames.Autoincrement]);
            });

    [ConditionalFact]
    public void Alter_property_add_autoincrement_strategy()
        => Execute(
            common => common.Entity(
                "Person",
                x =>
                {
                    x.Property<int>("Id").ValueGeneratedNever();
                    x.HasKey("Id");
                }),
            source => { },
            target => target.Entity("Person").Property<int>("Id").ValueGeneratedOnAdd().UseAutoincrement(),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var alterColumnOperation = Assert.IsType<AlterColumnOperation>(upOps[0]);
                Assert.Equal(true, alterColumnOperation[SqliteAnnotationNames.Autoincrement]);
                Assert.Null(alterColumnOperation.OldColumn[SqliteAnnotationNames.Autoincrement]);
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
            source => { },
            target => target.Entity("Person").Property<int>("Id").ValueGeneratedNever(),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var alterColumnOperation = Assert.IsType<AlterColumnOperation>(upOps[0]);
                Assert.Null(alterColumnOperation[SqliteAnnotationNames.Autoincrement]);
                Assert.Equal(true, alterColumnOperation.OldColumn[SqliteAnnotationNames.Autoincrement]);
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
            source => { },
            target => target.Entity<ProductWithConverter>().Property(e => e.Id).UseAutoincrement(),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);

                var alterColumnOperation = Assert.IsType<AlterColumnOperation>(upOps[0]);
                Assert.Equal(true, alterColumnOperation[SqliteAnnotationNames.Autoincrement]);
                Assert.Null(alterColumnOperation.OldColumn[SqliteAnnotationNames.Autoincrement]);
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
            Assert.Empty);

    [ConditionalFact]
    public void Noop_when_changing_to_autoincrement_property_with_converter()
        => Execute(
            source => source.Entity(
                "ProductWithConverter",
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
                        v => new ProductId(v))
                        .UseAutoincrement();
                    x.HasKey(e => e.Id);
                    x.Ignore(e => e.Name);
                }),
            Assert.Empty);

    protected override TestHelpers TestHelpers => SqliteTestHelpers.Instance;

    // Test entities
    public record struct ProductId(int Value);

    public class ProductWithConverter
    {
        public ProductId Id { get; set; }
        public required string Name { get; set; }
    }
}
