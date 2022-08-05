// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

public class TableValuedDbFunctionConventionTest
{
    [ConditionalFact]
    public void Does_not_configure_return_entity_as_not_mapped()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.HasDbFunction(
            typeof(TableValuedDbFunctionConventionTest).GetMethod(
                nameof(GetKeylessEntities),
                BindingFlags.NonPublic | BindingFlags.Static));

        modelBuilder.Entity<KeylessEntity>().HasNoKey();
        var model = Finalize(modelBuilder);

        var entityType = model.FindEntityType(typeof(KeylessEntity));

        Assert.Null(entityType.FindPrimaryKey());
        Assert.Equal("KeylessEntity", entityType.GetViewOrTableMappings().Single().Table.Name);
    }

    [ConditionalFact]
    public void Finds_existing_entity_type()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<TestEntity>().ToTable("TestTable").HasKey(e => e.Name);
        modelBuilder.HasDbFunction(
            typeof(TableValuedDbFunctionConventionTest).GetMethod(
                nameof(GetEntities),
                BindingFlags.NonPublic | BindingFlags.Static));

        var model = Finalize(modelBuilder);

        var entityType = model.FindEntityType(typeof(TestEntity));

        Assert.Equal(nameof(TestEntity.Name), entityType.FindPrimaryKey().Properties.Single().Name);
        Assert.Equal("TestTable", entityType.GetViewOrTableMappings().Single().Table.Name);
    }

    [ConditionalFact]
    public void Throws_when_adding_a_function_returning_an_owned_type()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Owned<KeylessEntity>();
        modelBuilder.HasDbFunction(
            typeof(TableValuedDbFunctionConventionTest).GetMethod(
                nameof(GetKeylessEntities),
                BindingFlags.NonPublic | BindingFlags.Static));

        Assert.Equal(
            RelationalStrings.DbFunctionInvalidIQueryableOwnedReturnType(
                "Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.TableValuedDbFunctionConventionTest.GetKeylessEntities(int)",
                typeof(KeylessEntity).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => Finalize(modelBuilder)).Message);
    }

    [ConditionalFact]
    public void Throws_when_adding_a_function_returning_an_existing_owned_type()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<TestEntity>().OwnsOne(e => e.KeylessEntity);
        modelBuilder.HasDbFunction(
            typeof(TableValuedDbFunctionConventionTest).GetMethod(
                nameof(GetKeylessEntities),
                BindingFlags.NonPublic | BindingFlags.Static));

        Assert.Equal(
            RelationalStrings.DbFunctionInvalidIQueryableOwnedReturnType(
                "Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.TableValuedDbFunctionConventionTest.GetKeylessEntities(int)",
                typeof(KeylessEntity).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => Finalize(modelBuilder)).Message);
    }

    [ConditionalFact]
    public void Throws_when_adding_a_function_returning_a_scalar()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.HasDbFunction(
            typeof(TableValuedDbFunctionConventionTest).GetMethod(
                nameof(GetScalars),
                BindingFlags.NonPublic | BindingFlags.Static));

        Assert.Equal(
            RelationalStrings.DbFunctionInvalidIQueryableReturnType(
                "Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.TableValuedDbFunctionConventionTest.GetScalars(int)",
                typeof(IQueryable<int>).ShortDisplayName()),
            Assert.Throws<InvalidOperationException>(() => Finalize(modelBuilder)).Message);
    }

    private static TestHelpers.TestModelBuilder CreateModelBuilder()
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder();

    private static IModel Finalize(TestHelpers.TestModelBuilder modelBuilder)
        => modelBuilder.FinalizeModel();

    private static IQueryable<TestEntity> GetEntities(int id)
        => throw new NotImplementedException();

    private static IQueryable<KeylessEntity> GetKeylessEntities(int id)
        => throw new NotImplementedException();

    private static IQueryable<int> GetScalars(int id)
        => throw new NotImplementedException();

    private class TestEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public KeylessEntity KeylessEntity { get; set; }
    }

    private class KeylessEntity
    {
        public string Name { get; set; }
    }
}
