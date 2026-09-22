// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.InMemory.Internal;

namespace Microsoft.EntityFrameworkCore;

public class NullabilityCheckInMemoryTest(InMemoryFixture fixture) : IClassFixture<InMemoryFixture>
{
    protected InMemoryFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public void IsRequired_for_property_throws_while_inserting_null_value()
        => Assert.Equal(
            InMemoryStrings.NullabilityErrorException($"{{'{nameof(SomeEntity.Property)}'}}", nameof(SomeEntity)),
            Assert.Throws<DbUpdateException>(
                () =>
                {
                    var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
                    modelBuilder.Entity<SomeEntity>(eb => eb.Property(p => p.Property).IsRequired());

                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseModel(modelBuilder.FinalizeModel())
                        .UseInMemoryDatabase(nameof(NullabilityCheckInMemoryTest), b => b.EnableNullChecks())
                        .UseInternalServiceProvider(Fixture.ServiceProvider);

                    using var context = new DbContext(optionsBuilder.Options);
                    context.Add(new SomeEntity { Id = 1 });
                    context.SaveChanges();
                }).Message);

    [ConditionalFact]
    public void IsRequired_for_property_throws_while_inserting_null_value_sensitive()
        => Assert.Equal(
            InMemoryStrings.NullabilityErrorExceptionSensitive($"{{'{nameof(SomeEntity.Property)}'}}", nameof(SomeEntity), "{Id: 1}"),
            Assert.Throws<DbUpdateException>(
                () =>
                {
                    var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
                    modelBuilder.Entity<SomeEntity>(eb => eb.Property(p => p.Property).IsRequired());

                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseModel(modelBuilder.FinalizeModel())
                        .UseInMemoryDatabase(nameof(NullabilityCheckInMemoryTest), b => b.EnableNullChecks())
                        .UseInternalServiceProvider(InMemoryFixture.DefaultNullabilitySensitiveCheckProvider)
                        .EnableSensitiveDataLogging();

                    using var context = new DbContext(optionsBuilder.Options);
                    context.Add(new SomeEntity { Id = 1 });
                    context.SaveChanges();
                }).Message);

    [ConditionalFact]
    public void IsRequired_for_property_throws_while_inserting_null_value_sensitive_with_composite_keys()
        => Assert.Equal(
            InMemoryStrings.NullabilityErrorExceptionSensitive(
                $"{{'{nameof(AnotherEntityWithCompositeKeys.Property)}'}}", nameof(AnotherEntityWithCompositeKeys),
                "{Id: 1, SecondId: 2}"),
            Assert.Throws<DbUpdateException>(
                () =>
                {
                    var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
                    modelBuilder.Entity<AnotherEntityWithCompositeKeys>(
                        eb =>
                        {
                            eb.Property(p => p.Property).IsRequired();
                            eb.HasKey(c => new { c.Id, c.SecondId });
                        });

                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseModel(modelBuilder.FinalizeModel())
                        .UseInMemoryDatabase(nameof(NullabilityCheckInMemoryTest), b => b.EnableNullChecks())
                        .UseInternalServiceProvider(InMemoryFixture.DefaultNullabilitySensitiveCheckProvider)
                        .EnableSensitiveDataLogging();

                    using var context = new DbContext(optionsBuilder.Options);
                    context.Add(new AnotherEntityWithCompositeKeys { Id = 1, SecondId = 2 });
                    context.SaveChanges();
                }).Message);

    [ConditionalFact]
    public void RequiredAttribute_for_property_throws_while_inserting_null_value()
        => Assert.Equal(
            InMemoryStrings.NullabilityErrorException(
                $"{{'{nameof(EntityWithRequiredAttribute.RequiredProperty)}'}}", nameof(EntityWithRequiredAttribute)),
            Assert.Throws<DbUpdateException>(
                () =>
                {
                    var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
                    modelBuilder.Entity<EntityWithRequiredAttribute>();

                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseModel(modelBuilder.FinalizeModel())
                        .UseInMemoryDatabase(nameof(NullabilityCheckInMemoryTest), b => b.EnableNullChecks())
                        .UseInternalServiceProvider(Fixture.ServiceProvider);

                    using var context = new DbContext(optionsBuilder.Options);
                    context.Add(new EntityWithRequiredAttribute { Id = 1 });
                    context.SaveChanges();
                }).Message);

    [ConditionalFact]
    public void RequiredAttribute_And_IsRequired_for_properties_throws_while_inserting_null_values()
        => Assert.Equal(
            InMemoryStrings.NullabilityErrorException(
                $"{{'{nameof(AnotherEntityWithRequiredAttribute.Property)}', '{nameof(AnotherEntityWithRequiredAttribute.RequiredProperty)}'}}",
                nameof(AnotherEntityWithRequiredAttribute)),
            Assert.Throws<DbUpdateException>(
                () =>
                {
                    var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
                    modelBuilder.Entity<AnotherEntityWithRequiredAttribute>(eb => eb.Property(p => p.Property).IsRequired());

                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseModel(modelBuilder.FinalizeModel())
                        .UseInMemoryDatabase(nameof(NullabilityCheckInMemoryTest), b => b.EnableNullChecks())
                        .UseInternalServiceProvider(Fixture.ServiceProvider);

                    using var context = new DbContext(optionsBuilder.Options);
                    context.Add(new AnotherEntityWithRequiredAttribute { Id = 1 });
                    context.SaveChanges();
                }).Message);

    [ConditionalFact]
    public void Can_insert_null_value_with_IsRequired_for_property_if_nullability_check_is_disabled()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<SomeEntity>(eb => eb.Property(p => p.Property).IsRequired());

        var optionsBuilder = new DbContextOptionsBuilder()
            .UseModel(modelBuilder.FinalizeModel())
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .UseInternalServiceProvider(InMemoryFixture.DefaultNullabilityCheckProvider);

        using var context = new DbContext(optionsBuilder.Options);
        context.Add(new SomeEntity { Id = 1 });
        context.SaveChanges();

        Assert.NotNull(context.Set<SomeEntity>().SingleOrDefault());
    }

    [ConditionalFact]
    public void Can_insert_null_value_with_RequiredAttribute_for_property_if_nullability_check_is_disabled()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<EntityWithRequiredAttribute>();

        var optionsBuilder = new DbContextOptionsBuilder()
            .UseModel(modelBuilder.FinalizeModel())
            .UseInMemoryDatabase(nameof(NullabilityCheckInMemoryTest), b => b.EnableNullChecks(false))
            .UseInternalServiceProvider(InMemoryFixture.DefaultNullabilityCheckProvider);

        using var context = new DbContext(optionsBuilder.Options);
        context.Add(new EntityWithRequiredAttribute { Id = 1 });
        context.SaveChanges();

        Assert.NotNull(context.Set<EntityWithRequiredAttribute>().SingleOrDefault());
    }

    [ConditionalFact]
    public void Can_insert_null_values_with_RequiredAttribute_and_IsRequired_for_properties_if_nullability_check_is_disabled()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<AnotherEntityWithRequiredAttribute>(eb => eb.Property(p => p.Property).IsRequired());

        var optionsBuilder = new DbContextOptionsBuilder()
            .UseModel(modelBuilder.FinalizeModel())
            .UseInMemoryDatabase(nameof(NullabilityCheckInMemoryTest), b => b.EnableNullChecks(false))
            .UseInternalServiceProvider(InMemoryFixture.DefaultNullabilityCheckProvider);

        using var context = new DbContext(optionsBuilder.Options);
        context.Add(new AnotherEntityWithRequiredAttribute { Id = 1 });
        context.SaveChanges();

        Assert.NotNull(context.Set<AnotherEntityWithRequiredAttribute>().SingleOrDefault());
    }

    private class EntityWithRequiredAttribute
    {
        public int Id { get; set; }

        [Required]
        public string RequiredProperty { get; set; }
    }

    private class SomeEntity
    {
        public int Id { get; set; }

        public string Property { get; set; }
    }

    private class AnotherEntityWithRequiredAttribute
    {
        public int Id { get; set; }

        [Required]
        public string RequiredProperty { get; set; }

        public string Property { get; set; }
    }

    private class AnotherEntityWithCompositeKeys
    {
        public int Id { get; set; }

        public int SecondId { get; set; }

        public string Property { get; set; }
    }
}
