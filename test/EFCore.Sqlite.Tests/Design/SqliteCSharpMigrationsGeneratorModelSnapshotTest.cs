// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Sqlite.Design;

public class CSharpMigrationsGeneratorSqliteTest
{
    [ConditionalFact]
    public void Autoincrement_annotation_is_replaced_by_extension_method_call_in_snapshot()
    {
        Test(
            builder =>
            {
                builder.Entity<EntityWithAutoincrement>(e =>
                {
                    e.Property(p => p.Id).UseAutoincrement();
                });
            },
            "SqlitePropertyBuilderExtensions.UseAutoincrement(b.Property<int>(\"Id\"));",
            model =>
            {
                var entity = model.FindEntityType(typeof(EntityWithAutoincrement));
                var property = entity!.FindProperty("Id");
                Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, property!.GetValueGenerationStrategy());
            });
    }

    [ConditionalFact]
    public void Autoincrement_works_with_value_converter_to_int()
    {
        Test(
            builder =>
            {
                builder.Entity<EntityWithConverterPk>(e =>
                {
                    e.Property(p => p.Id)
                        .HasConversion<int>()
                        .UseAutoincrement();
                });
            },
            "SqlitePropertyBuilderExtensions.UseAutoincrement(b.Property<int>(\"Id\"));",
            model =>
            {
                var entity = model.FindEntityType(typeof(EntityWithConverterPk));
                var property = entity!.FindProperty("Id");
                // Should have autoincrement strategy even with value converter
                Assert.Equal(SqliteValueGenerationStrategy.Autoincrement, property!.GetValueGenerationStrategy());
            });
    }

    [ConditionalFact]
    public void No_autoincrement_method_call_when_strategy_is_none()
    {
        Test(
            builder =>
            {
                builder.Entity<EntityWithAutoincrement>(e =>
                {
                    e.Property(p => p.Id).ValueGeneratedNever();
                });
            },
            "b.Property<int>(\"Id\")",
            model =>
            {
                var entity = model.FindEntityType(typeof(EntityWithAutoincrement));
                var property = entity!.FindProperty("Id");
                Assert.Equal(SqliteValueGenerationStrategy.None, property!.GetValueGenerationStrategy());
            });
    }

    private class EntityWithAutoincrement
    {
        public int Id { get; set; }
    }

    private class EntityWithConverterPk
    {
        public long Id { get; set; }
    }

    private class EntityWithStringKey
    {
        public string Id { get; set; } = null!;
    }

    protected void Test(Action<ModelBuilder> buildModel, string expectedCodeFragment, Action<IModel> assert)
    {
        var modelBuilder = CreateConventionalModelBuilder();
        modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
        buildModel(modelBuilder);

        var model = modelBuilder.FinalizeModel(designTime: true);

        var generator = CreateMigrationsGenerator();
        var code = generator.GenerateSnapshot("RootNamespace", typeof(DbContext), "Snapshot", model);

        assert(model);

        Assert.Contains(expectedCodeFragment, code);
    }

    protected SqliteTestHelpers.TestModelBuilder CreateConventionalModelBuilder()
        => SqliteTestHelpers.Instance.CreateConventionBuilder();

    protected CSharpMigrationsGenerator CreateMigrationsGenerator()
    {
        var sqliteTypeMappingSource = new SqliteTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var codeHelper = new CSharpHelper(sqliteTypeMappingSource);

        var sqliteAnnotationCodeGenerator = new SqliteAnnotationCodeGenerator(
            new AnnotationCodeGeneratorDependencies(sqliteTypeMappingSource));

        var generator = new CSharpMigrationsGenerator(
            new MigrationsCodeGeneratorDependencies(
                sqliteTypeMappingSource,
                sqliteAnnotationCodeGenerator),
            new CSharpMigrationsGeneratorDependencies(
                codeHelper,
                new CSharpMigrationOperationGenerator(
                    new CSharpMigrationOperationGeneratorDependencies(
                        codeHelper)),
                new CSharpSnapshotGenerator(
                    new CSharpSnapshotGeneratorDependencies(
                        codeHelper, sqliteTypeMappingSource, sqliteAnnotationCodeGenerator))));

        return generator;
    }
}