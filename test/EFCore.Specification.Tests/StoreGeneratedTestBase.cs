// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public abstract class StoreGeneratedTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : StoreGeneratedTestBase<TFixture>.StoreGeneratedFixtureBase, new()
{
    protected StoreGeneratedTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalFact]
    public virtual async Task Value_generation_works_for_common_GUID_conversions()
    {
        await ValueGenerationPositive<Guid, GuidToString>(Fixture.GuidSentinel);
        await ValueGenerationPositive<Guid, GuidToBytes>(Fixture.GuidSentinel);
    }

    private Task ValueGenerationPositive<TKey, TEntity>(TKey? sentinel)
        where TEntity : WithConverter<TKey>, new()
    {
        TKey? id = default;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(new TEntity { Id = sentinel }).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            }, async context =>
            {
                Assert.Equal(id, (await context.Set<TEntity>().SingleAsync(e => e.Id!.Equals(id))).Id);
            });
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.NeverThrowBeforeUseAfter))]
    [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter))]
    [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter))]
    [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter))]
    [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter))]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter))]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter))]
    public virtual Task Before_save_throw_always_throws_if_value_set(string propertyName)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Add(WithValue(propertyName, Fixture.IntSentinel, Fixture.StringSentinel));

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyBeforeSave(propertyName, "Anais"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalTheory]
    [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), null)]
    [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
    [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter), null)]
    [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter), "Rabbit")]
    public virtual Task Before_save_throw_ignores_value_if_not_set(string propertyName, string? expectedValue)
    {
        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Anais.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            }, async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.Never))]
    [InlineData(nameof(Anais.OnAdd))]
    [InlineData(nameof(Anais.OnUpdate))]
    [InlineData(nameof(Anais.NeverUseBeforeUseAfter))]
    [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter))]
    [InlineData(nameof(Anais.NeverUseBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddUseBeforeUseAfter))]
    [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter))]
    [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter))]
    [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter))]
    [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter))]
    public virtual Task Before_save_use_always_uses_value_if_set(string propertyName)
    {
        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(WithValue(propertyName, Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            }, async context => Assert.Equal("Pink", GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.Never), "S")]
    [InlineData(nameof(Anais.OnAdd), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdate), "S")]
    [InlineData(nameof(Anais.NeverUseBeforeUseAfter), "S")]
    [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), "S")]
    [InlineData(nameof(Anais.NeverUseBeforeThrowAfter), "S")]
    [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), "S")]
    [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), "S")]
    [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter), "S")]
    public virtual Task Before_save_use_ignores_value_if_not_set(string propertyName, string? expectedValue)
    {
        if (expectedValue == "S")
        {
            expectedValue = Fixture.StringSentinel;
        }

        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Anais.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            }, async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
    [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
    [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
    [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
    public virtual Task Before_save_ignore_ignores_value_if_not_set(string propertyName, string? expectedValue)
    {
        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Anais.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            }, async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
    [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
    [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
    [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
    public virtual Task Before_save_ignore_ignores_value_even_if_set(string propertyName, string? expectedValue)
    {
        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(WithValue(propertyName, Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            }, async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.NeverUseBeforeThrowAfter))]
    [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter))]
    [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter))]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter))]
    public virtual Task After_save_throw_always_throws_if_value_modified(string propertyName)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Attach(WithValue(propertyName, 1, Fixture.StringSentinel)).Property(propertyName).IsModified = true;

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyAfterSave(propertyName, "Anais"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalTheory]
    [InlineData(nameof(Anais.NeverUseBeforeThrowAfter), "S")]
    [InlineData(nameof(Anais.NeverIgnoreBeforeThrowAfter), null)]
    [InlineData(nameof(Anais.NeverThrowBeforeThrowAfter), null)]
    [InlineData(nameof(Anais.OnAddUseBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddThrowBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateUseBeforeThrowAfter), "S")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeThrowAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeThrowAfter), "Rabbit")]
    public virtual Task After_save_throw_ignores_value_if_not_modified(string propertyName, string? expectedValue)
    {
        if (expectedValue == "S")
        {
            expectedValue = Fixture.StringSentinel;
        }

        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Anais.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            },
            async context =>
            {
                var entry = context.Entry((await context.Set<Anais>().FindAsync(id))!);
                entry.State = EntityState.Modified;
                entry.Property(propertyName).CurrentValue = "Daisy";
                entry.Property(propertyName).IsModified = false;

                await context.SaveChangesAsync();
            },
            async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdate), "S")]
    [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), "S")]
    [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
    [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
    [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), "S")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
    public virtual Task After_save_ignore_ignores_value_if_not_modified(string propertyName, string? expectedValue)
    {
        if (expectedValue == "S")
        {
            expectedValue = Fixture.StringSentinel;
        }

        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Anais.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            },
            async context =>
            {
                var entry = context.Entry((await context.Set<Anais>().FindAsync(id))!);
                entry.State = EntityState.Modified;
                entry.Property(propertyName).CurrentValue = "Daisy";
                entry.Property(propertyName).IsModified = false;

                await context.SaveChangesAsync();
            },
            async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdate), "S")]
    [InlineData(nameof(Anais.NeverUseBeforeIgnoreAfter), "S")]
    [InlineData(nameof(Anais.NeverIgnoreBeforeIgnoreAfter), null)]
    [InlineData(nameof(Anais.NeverThrowBeforeIgnoreAfter), null)]
    [InlineData(nameof(Anais.OnAddUseBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddThrowBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateUseBeforeIgnoreAfter), "S")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeIgnoreAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeIgnoreAfter), "Rabbit")]
    public virtual Task After_save_ignore_ignores_value_even_if_modified(string propertyName, string? expectedValue)
    {
        if (expectedValue == "S")
        {
            expectedValue = Fixture.StringSentinel;
        }

        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Anais.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            },
            async context =>
            {
                var entry = context.Entry((await context.Set<Anais>().FindAsync(id))!);
                entry.State = EntityState.Modified;
                entry.Property(propertyName).CurrentValue = "Daisy";
                entry.Property(propertyName).IsModified = true;

                await context.SaveChangesAsync();
            },
            async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.Never), "S")]
    [InlineData(nameof(Anais.OnAdd), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdate), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdate), "S")]
    [InlineData(nameof(Anais.NeverUseBeforeUseAfter), "S")]
    [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), null)]
    [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), null)]
    [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), "S")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Rabbit")]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Rabbit")]
    public virtual Task After_save_use_ignores_value_if_not_modified(string propertyName, string? expectedValue)
    {
        if (expectedValue == "S")
        {
            expectedValue = Fixture.StringSentinel;
        }

        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Anais.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            },
            async context =>
            {
                var entry = context.Entry((await context.Set<Anais>().FindAsync(id))!);
                entry.State = EntityState.Modified;
                entry.Property(propertyName).CurrentValue = "Daisy";
                entry.Property(propertyName).IsModified = false;

                await context.SaveChangesAsync();
            },
            async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    [ConditionalTheory]
    [InlineData(nameof(Anais.Never), "Daisy")]
    [InlineData(nameof(Anais.OnAdd), "Daisy")]
    [InlineData(nameof(Anais.NeverUseBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.NeverIgnoreBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.NeverThrowBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnAddUseBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnAddIgnoreBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnAddThrowBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnAddOrUpdateUseBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnAddOrUpdateIgnoreBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnAddOrUpdateThrowBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnUpdateUseBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnUpdateIgnoreBeforeUseAfter), "Daisy")]
    [InlineData(nameof(Anais.OnUpdateThrowBeforeUseAfter), "Daisy")]
    public virtual Task After_save_use_uses_value_if_modified(string propertyName, string expectedValue)
    {
        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Anais.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();

                id = entity.Id;
            },
            async context =>
            {
                var entry = context.Entry((await context.Set<Anais>().FindAsync(id))!);
                entry.State = EntityState.Modified;
                entry.Property(propertyName).CurrentValue = "Daisy";

                await context.SaveChangesAsync();
            },
            async context => Assert.Equal(expectedValue, GetValue((await context.Set<Anais>().FindAsync(id))!, propertyName)));
    }

    private static Anais WithValue(string propertyName, int id, string? sentinel)
        => SetValue(Anais.Create(id, sentinel), propertyName);

    private static Anais SetValue(Anais entity, string propertyName)
    {
        entity.GetType().GetTypeInfo().GetDeclaredProperty(propertyName)!.SetValue(entity, "Pink");
        return entity;
    }

    private static string? GetValue(Anais entity, string propertyName)
        => (string?)entity.GetType().GetTypeInfo().GetDeclaredProperty(propertyName)!.GetValue(entity);

    [ConditionalFact]
    public virtual Task Identity_key_with_read_only_before_save_throws_if_explicit_values_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Add(Gumball.Create(Fixture.IntSentinel + 1, Fixture.StringSentinel));

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyBeforeSave("Id", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.Identity = "Masami";
                var entry = context.Add(gumball);
                entry.Property(e => e.Identity).IsTemporary = true;

                await context.SaveChangesAsync();
                id = entry.Entity.Id;

                Assert.Equal("Banana Joe", entry.Entity.Identity);
                Assert.False(entry.Property(e => e.Identity).IsTemporary);
            },
            async context => Assert.Equal("Banana Joe", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Identity));
    }

    protected class CompositePrincipal
    {
        public int Id { get; set; }
        public int? CurrentNumber { get; set; }
        public CompositeDependent? Current { get; set; }
        public ICollection<CompositeDependent> Periods { get; } = new HashSet<CompositeDependent>();
    }

    protected class CompositeDependent
    {
        public int PrincipalId { get; set; }
        public int Number { get; set; }
        public CompositePrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual Task Store_generated_values_are_propagated_with_composite_key_cycles()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var period = new CompositeDependent
                {
                    PrincipalId = Fixture.IntSentinel,
                    Number = 1,
                    Principal = new CompositePrincipal { Id = Fixture.IntSentinel }
                };

                context.Add(period);
                await context.SaveChangesAsync();

                id = period.PrincipalId;
            },
            async context => Assert.Equal(1, (await context.Set<CompositeDependent>().SingleAsync(e => e.PrincipalId == id)).Number));
    }

    protected class NonStoreGenDependent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int? StoreGenPrincipalId { get; set; }
        public int HasTemp { get; set; }
        public StoreGenPrincipal StoreGenPrincipal { get; set; } = null!;
    }

    protected class StoreGenPrincipal
    {
        public int Id { get; set; }
    }

    [ConditionalTheory] // Issue #22027 #14192
    [InlineData(EntityState.Modified)]
    [InlineData(EntityState.Deleted)]
    public Task Change_state_of_entity_with_temp_non_key_does_not_throw(EntityState targetState)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var dependent = new NonStoreGenDependent { Id = 89 };

                context.Add(dependent);

                Assert.True(context.Entry(dependent).Property(e => e.HasTemp).IsTemporary);

                await context.SaveChangesAsync();

                Assert.False(context.Entry(dependent).Property(e => e.HasTemp).IsTemporary);
                Assert.Equal(777, dependent.HasTemp);
            },
            async context =>
            {
                var principal = new StoreGenPrincipal { Id = Fixture.IntSentinel };
                var dependent = new NonStoreGenDependent { Id = 89, StoreGenPrincipal = principal };

                context.Add(dependent);

                context.Entry(dependent).State = targetState;

                Assert.Equal(EntityState.Added, context.Entry(principal).State);
                Assert.True(context.Entry(principal).Property(e => e.Id).IsTemporary);
                Assert.True(context.Entry(dependent).Property(e => e.HasTemp).IsTemporary);
                Assert.True(context.Entry(dependent).Property(e => e.StoreGenPrincipalId).IsTemporary);

                await context.SaveChangesAsync();

                Assert.Equal(EntityState.Unchanged, context.Entry(principal).State);

                Assert.Equal(
                    targetState == EntityState.Modified ? EntityState.Unchanged : EntityState.Detached,
                    context.Entry(dependent).State);

                Assert.False(context.Entry(principal).Property(e => e.Id).IsTemporary);
                Assert.False(context.Entry(dependent).Property(e => e.HasTemp).IsTemporary);
                Assert.False(context.Entry(dependent).Property(e => e.StoreGenPrincipalId).IsTemporary);
            });

    [ConditionalFact] // Issue #19137
    public Task Clearing_optional_FK_does_not_leave_temporary_value()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var product = new OptionalProduct { Id = Fixture.IntSentinel };
                context.Add(product);

                Assert.True(context.ChangeTracker.HasChanges());

                var productEntry = context.Entry(product);
                Assert.Equal(EntityState.Added, productEntry.State);

                Assert.Equal(Fixture.IntSentinel, product.Id);
                Assert.True(productEntry.Property(e => e.Id).CurrentValue < 0);
                Assert.True(productEntry.Property(e => e.Id).IsTemporary);

                Assert.Null(product.CategoryId);
                Assert.Null(productEntry.Property(e => e.CategoryId).CurrentValue);
                Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                productEntry = context.Entry(product);
                Assert.Equal(EntityState.Unchanged, productEntry.State);

                Assert.Equal(1, product.Id);
                Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                Assert.Null(product.CategoryId);
                Assert.Null(productEntry.Property(e => e.CategoryId).CurrentValue);
                Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                var category = new OptionalCategory();
                product.Category = category;

                Assert.True(context.ChangeTracker.HasChanges());

                productEntry = context.Entry(product);
                Assert.Equal(EntityState.Modified, productEntry.State);

                Assert.Equal(1, product.Id);
                Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                Assert.Null(product.CategoryId);
                Assert.True(productEntry.Property(e => e.CategoryId).CurrentValue < 0);
                Assert.True(productEntry.Property(e => e.CategoryId).IsTemporary);

                var categoryEntry = context.Entry(category);
                Assert.Equal(EntityState.Added, categoryEntry.State);
                Assert.Equal(0, category.Id);
                Assert.True(categoryEntry.Property(e => e.Id).CurrentValue < 0);
                Assert.True(categoryEntry.Property(e => e.Id).IsTemporary);

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                productEntry = context.Entry(product);
                Assert.Equal(EntityState.Unchanged, productEntry.State);

                Assert.Equal(1, product.Id);
                Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                Assert.Equal(1, product.CategoryId);
                Assert.Equal(1, productEntry.Property(e => e.CategoryId).CurrentValue);
                Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                categoryEntry = context.Entry(category);
                Assert.Equal(EntityState.Unchanged, categoryEntry.State);
                Assert.Equal(1, category.Id);
                Assert.Equal(1, categoryEntry.Property(e => e.Id).CurrentValue);
                Assert.False(categoryEntry.Property(e => e.Id).IsTemporary);

                product.Category = null;

                productEntry = context.Entry(product);
                Assert.Equal(EntityState.Modified, productEntry.State);

                Assert.Equal(1, product.Id);
                Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                Assert.Null(product.CategoryId);
                Assert.Null(productEntry.Property(e => e.CategoryId).CurrentValue);
                Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                categoryEntry = context.Entry(category);
                Assert.Equal(EntityState.Unchanged, categoryEntry.State);
                Assert.Equal(1, category.Id);
                Assert.Equal(1, categoryEntry.Property(e => e.Id).CurrentValue);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                productEntry = context.Entry(product);
                Assert.Equal(EntityState.Unchanged, productEntry.State);

                Assert.Equal(1, product.Id);
                Assert.Null(product.CategoryId);
                Assert.False(productEntry.Property(e => e.Id).IsTemporary);

                Assert.Equal(1, productEntry.Property(e => e.Id).CurrentValue);
                Assert.Null(productEntry.Property(e => e.CategoryId).CurrentValue);
                Assert.False(productEntry.Property(e => e.CategoryId).IsTemporary);

                categoryEntry = context.Entry(category);
                Assert.Equal(EntityState.Unchanged, categoryEntry.State);
                Assert.Equal(1, category.Id);
                Assert.Equal(1, categoryEntry.Property(e => e.Id).CurrentValue);
                Assert.False(categoryEntry.Property(e => e.Id).IsTemporary);
            });

    protected class OptionalProduct
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public OptionalCategory? Category { get; set; }
    }

    protected class OptionalCategory
    {
        public int Id { get; set; }
    }

    [ConditionalFact]
    public virtual Task Identity_property_on_Added_entity_with_temporary_value_gets_value_from_store_even_if_same()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.Identity = "Banana Joe";
                var entry = context.Add(gumball);
                entry.Property(e => e.Identity).IsTemporary = true;

                await context.SaveChangesAsync();
                id = entry.Entity.Id;

                Assert.Equal("Banana Joe", entry.Entity.Identity);
                Assert.False(entry.Property(e => e.Identity).IsTemporary);
            },
            async context => Assert.Equal("Banana Joe", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Identity));
    }

    [ConditionalFact]
    public virtual Task Identity_property_on_Added_entity_with_default_value_gets_value_from_store()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;

                Assert.Equal("Banana Joe", entity.Identity);
            },
            async context => Assert.Equal("Banana Joe", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Identity));
    }

    [ConditionalFact]
    public virtual Task Identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.IdentityReadOnlyBeforeSave = "Masami";
                context.Add(gumball);

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyBeforeSave("IdentityReadOnlyBeforeSave", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Identity_property_on_Added_entity_can_have_value_set_explicitly()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.Identity = "Masami";
                var entity = context.Add(gumball).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;

                Assert.Equal("Masami", entity.Identity);
            },
            async context => Assert.Equal("Masami", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Identity));
    }

    [ConditionalFact]
    public virtual Task Identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Anton", gumball.IdentityReadOnlyAfterSave);

                gumball.IdentityReadOnlyAfterSave = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyAfterSave("IdentityReadOnlyAfterSave", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });
    }

    [ConditionalFact]
    public virtual Task Identity_property_on_Modified_entity_is_included_in_update_when_modified()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Banana Joe", gumball.Identity);

                gumball.Identity = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                await context.SaveChangesAsync();

                Assert.Equal("Masami", gumball.Identity);
            },
            async context => Assert.Equal("Masami", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Identity));
    }

    [ConditionalFact]
    public virtual Task Identity_property_on_Modified_entity_is_not_included_in_update_when_not_modified()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Banana Joe", gumball.Identity);

                gumball.Identity = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.Entry(gumball).Property(e => e.Identity).OriginalValue = "Masami";
                context.Entry(gumball).Property(e => e.Identity).IsModified = false;

                await context.SaveChangesAsync();

                Assert.Equal("Masami", gumball.Identity);
            },
            async context => Assert.Equal("Banana Joe", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Identity));
    }

    [ConditionalFact]
    public virtual Task Always_identity_property_on_Added_entity_with_temporary_value_gets_value_from_store()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.AlwaysIdentity = "Masami";
                var entry = context.Add(gumball);
                entry.Property(e => e.AlwaysIdentity).IsTemporary = true;

                await context.SaveChangesAsync();
                id = entry.Entity.Id;

                Assert.Equal("Banana Joe", entry.Entity.AlwaysIdentity);
            },
            async context => Assert.Equal("Banana Joe", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).AlwaysIdentity));
    }

    [ConditionalFact]
    public virtual Task Always_identity_property_on_Added_entity_with_default_value_gets_value_from_store()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;

                Assert.Equal("Banana Joe", entity.AlwaysIdentity);
            },
            async context => Assert.Equal("Banana Joe", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).AlwaysIdentity));
    }

    [ConditionalFact]
    public virtual Task Always_identity_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.AlwaysIdentityReadOnlyBeforeSave = "Masami";
                context.Add(gumball);

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyBeforeSave("AlwaysIdentityReadOnlyBeforeSave", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Always_identity_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Anton", gumball.AlwaysIdentityReadOnlyAfterSave);

                gumball.AlwaysIdentityReadOnlyAfterSave = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyAfterSave("AlwaysIdentityReadOnlyAfterSave", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });
    }

    [ConditionalFact]
    public virtual Task Always_identity_property_on_Modified_entity_is_not_included_in_the_update_when_not_modified()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Banana Joe", gumball.AlwaysIdentity);

                gumball.AlwaysIdentity = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.Entry(gumball).Property(e => e.AlwaysIdentity).OriginalValue = "Masami";
                context.Entry(gumball).Property(e => e.AlwaysIdentity).IsModified = false;

                await context.SaveChangesAsync();

                Assert.Equal("Masami", gumball.AlwaysIdentity);
            }, async context => Assert.Equal("Banana Joe", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).AlwaysIdentity));
    }

    [ConditionalFact]
    public virtual Task Computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.Computed = "Masami";
                var entry = context.Add(gumball);
                entry.Property(e => e.Computed).IsTemporary = true;

                await context.SaveChangesAsync();
                id = entry.Entity.Id;

                Assert.Equal("Alan", entry.Entity.Computed);
            },
            async context => Assert.Equal("Alan", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Computed));
    }

    [ConditionalFact]
    public virtual Task Computed_property_on_Added_entity_with_default_value_gets_value_from_store()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;

                Assert.Equal("Alan", entity.Computed);
            },
            async context => Assert.Equal("Alan", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Computed));
    }

    [ConditionalFact]
    public virtual Task Computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.ComputedReadOnlyBeforeSave = "Masami";
                context.Add(gumball);

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyBeforeSave("ComputedReadOnlyBeforeSave", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Computed_property_on_Added_entity_can_have_value_set_explicitly()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.Computed = "Masami";
                var entity = context.Add(gumball).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;

                Assert.Equal("Masami", entity.Computed);
            },
            async context => Assert.Equal("Masami", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Computed));
    }

    [ConditionalFact]
    public virtual Task Computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Tina Rex", gumball.ComputedReadOnlyAfterSave);

                gumball.ComputedReadOnlyAfterSave = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyAfterSave("ComputedReadOnlyAfterSave", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });
    }

    [ConditionalFact]
    public virtual Task Computed_property_on_Modified_entity_is_included_in_update_when_modified()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Alan", gumball.Computed);

                gumball.Computed = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                await context.SaveChangesAsync();

                Assert.Equal("Masami", gumball.Computed);
            },
            async context => Assert.Equal("Masami", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Computed));
    }

    [ConditionalFact]
    public virtual Task Computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Alan", gumball.Computed);

                gumball.Computed = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.Entry(gumball).Property(e => e.Computed).OriginalValue = "Masami";
                context.Entry(gumball).Property(e => e.Computed).IsModified = false;

                await context.SaveChangesAsync();

                Assert.Equal("Alan", gumball.Computed);
            },
            async context => Assert.Equal("Alan", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).Computed));
    }

    [ConditionalFact]
    public virtual Task Always_computed_property_on_Added_entity_with_temporary_value_gets_value_from_store()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.AlwaysComputed = "Masami";
                var entry = context.Add(gumball);
                entry.Property(e => e.AlwaysComputed).IsTemporary = true;

                await context.SaveChangesAsync();
                id = entry.Entity.Id;

                Assert.Equal("Alan", entry.Entity.AlwaysComputed);
            },
            async context => Assert.Equal("Alan", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).AlwaysComputed));
    }

    [ConditionalFact]
    public virtual Task Always_computed_property_on_Added_entity_with_default_value_gets_value_from_store()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;

                Assert.Equal("Alan", entity.AlwaysComputed);
            },
            async context => Assert.Equal("Alan", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).AlwaysComputed));
    }

    [ConditionalFact]
    public virtual Task Always_computed_property_on_Added_entity_with_read_only_before_save_throws_if_explicit_values_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gumball = Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel);
                gumball.AlwaysComputedReadOnlyBeforeSave = "Masami";
                context.Add(gumball);

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyBeforeSave("AlwaysComputedReadOnlyBeforeSave", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Always_computed_property_on_Modified_entity_with_read_only_after_save_throws_if_value_is_in_modified_state()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Tina Rex", gumball.AlwaysComputedReadOnlyAfterSave);

                gumball.AlwaysComputedReadOnlyAfterSave = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyAfterSave("AlwaysComputedReadOnlyAfterSave", "Gumball"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });
    }

    [ConditionalFact]
    public virtual Task Always_computed_property_on_Modified_entity_is_read_from_store_when_not_modified()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(Gumball.Create(Fixture.IntSentinel, Fixture.StringSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var gumball = await context.Set<Gumball>().SingleAsync(e => e.Id == id);

                Assert.Equal("Alan", gumball.AlwaysComputed);

                gumball.AlwaysComputed = "Masami";
                gumball.NotStoreGenerated = "Larry Needlemeye";

                context.Entry(gumball).Property(e => e.AlwaysComputed).OriginalValue = "Masami";
                context.Entry(gumball).Property(e => e.AlwaysComputed).IsModified = false;

                await context.SaveChangesAsync();

                Assert.Equal("Alan", gumball.AlwaysComputed);
            },
            async context => Assert.Equal("Alan", (await context.Set<Gumball>().SingleAsync(e => e.Id == id)).AlwaysComputed));
    }

    [ConditionalFact]
    public virtual Task Fields_used_correctly_for_store_generated_values()
    {
        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(WithBackingFields.Create(Fixture.IntSentinel, Fixture.NullableIntSentinel)).Entity;

                await context.SaveChangesAsync();
                id = entity.Id;
            },
            async context =>
            {
                var entity = await context.Set<WithBackingFields>().SingleAsync(e => e.Id.Equals(id));
                Assert.Equal(1, entity.NullableAsNonNullable);
                Assert.Equal(1, entity.NonNullableAsNullable);
            });
    }

    [ConditionalFact]
    public virtual Task Nullable_fields_get_defaults_when_not_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Add(WithNullableBackingFields.Create(Fixture.NullableIntSentinel, Fixture.NullableBoolSentinel))
                    .Entity;

                await context.SaveChangesAsync();

                Assert.NotEqual(0, entity.Id);
                Assert.True(entity.NullableBackedBoolTrueDefault);
                Assert.Equal(-1, entity.NullableBackedIntNonZeroDefault);
                Assert.False(entity.NullableBackedBoolFalseDefault);
                Assert.Equal(0, entity.NullableBackedIntZeroDefault);
            },
            async context =>
            {
                var entity = await context.Set<WithNullableBackingFields>().SingleAsync();
                Assert.True(entity.NullableBackedBoolTrueDefault);
                Assert.Equal(-1, entity.NullableBackedIntNonZeroDefault);
                Assert.False(entity.NullableBackedBoolFalseDefault);
                Assert.Equal(0, entity.NullableBackedIntZeroDefault);
            });

    [ConditionalFact]
    public virtual Task Properties_get_database_defaults_when_set_to_sentinel_values()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = new WithNoBackingFields
                {
                    Id = Fixture.IntSentinel,
                    TrueDefault = true, // Bool sentinel is always true by convention when default value is true
                    NonZeroDefault = Fixture.IntSentinel,
                    FalseDefault = Fixture.BoolSentinel,
                    ZeroDefault = Fixture.IntSentinel
                };

                context.Add(entity);

                await context.SaveChangesAsync();

                Assert.NotEqual(0, entity.Id);
                Assert.True(entity.TrueDefault);
                Assert.Equal(-1, entity.NonZeroDefault);
                Assert.False(entity.FalseDefault);
                Assert.Equal(0, entity.ZeroDefault);
            },
            async context =>
            {
                var entity = await context.Set<WithNoBackingFields>().SingleAsync();
                Assert.True(entity.TrueDefault);
                Assert.Equal(-1, entity.NonZeroDefault);
                Assert.False(entity.FalseDefault);
                Assert.Equal(0, entity.ZeroDefault);
            });

    [ConditionalFact]
    public virtual Task Properties_get_set_values_when_not_set_to_sentinel_values()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = new WithNoBackingFields
                {
                    Id = Fixture.IntSentinel,
                    TrueDefault = false, // Bool sentinel is always true by convention when default value is true
                    NonZeroDefault = 3,
                    FalseDefault = !Fixture.BoolSentinel,
                    ZeroDefault = 5
                };

                context.Add(entity);

                await context.SaveChangesAsync();

                Assert.NotEqual(0, entity.Id);
                Assert.False(entity.TrueDefault);
                Assert.Equal(3, entity.NonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.FalseDefault);
                Assert.Equal(5, entity.ZeroDefault);
            },
            async context =>
            {
                var entity = await context.Set<WithNoBackingFields>().SingleAsync();
                Assert.False(entity.TrueDefault);
                Assert.Equal(3, entity.NonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.FalseDefault);
                Assert.Equal(5, entity.ZeroDefault);
            });

    [ConditionalFact]
    public virtual Task Nullable_fields_store_non_defaults_when_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = WithNullableBackingFields.Create(Fixture.NullableIntSentinel, Fixture.NullableBoolSentinel);
                entity.NullableBackedBoolTrueDefault = Fixture.BoolSentinel;
                entity.NullableBackedIntNonZeroDefault = Fixture.IntSentinel;
                entity.NullableBackedBoolFalseDefault = !Fixture.BoolSentinel;
                entity.NullableBackedIntZeroDefault = Fixture.IntSentinel + 1;

                context.Add(entity);

                await context.SaveChangesAsync();

                Assert.NotEqual(0, entity.Id);
                Assert.Equal(Fixture.BoolSentinel, entity.NullableBackedBoolTrueDefault);
                Assert.Equal(Fixture.IntSentinel, entity.NullableBackedIntNonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolFalseDefault);
                Assert.Equal(Fixture.IntSentinel + 1, entity.NullableBackedIntZeroDefault);
            },
            async context =>
            {
                var entity = await context.Set<WithNullableBackingFields>().SingleAsync();
                Assert.Equal(Fixture.BoolSentinel, entity.NullableBackedBoolTrueDefault);
                Assert.Equal(Fixture.IntSentinel, entity.NullableBackedIntNonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolFalseDefault);
                Assert.Equal(Fixture.IntSentinel + 1, entity.NullableBackedIntZeroDefault);
            });

    [ConditionalFact]
    public virtual Task Nullable_fields_store_any_value_when_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = WithNullableBackingFields.Create(Fixture.NullableIntSentinel, Fixture.NullableBoolSentinel);
                entity.NullableBackedBoolTrueDefault = !Fixture.BoolSentinel;
                entity.NullableBackedIntNonZeroDefault = 3;
                entity.NullableBackedBoolFalseDefault = !Fixture.BoolSentinel;
                entity.NullableBackedIntZeroDefault = 5;

                context.Add(entity);

                await context.SaveChangesAsync();

                Assert.NotEqual(0, entity.Id);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolTrueDefault);
                Assert.Equal(3, entity.NullableBackedIntNonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolFalseDefault);
                Assert.Equal(5, entity.NullableBackedIntZeroDefault);
            },
            async context =>
            {
                var entity = await context.Set<WithNullableBackingFields>().SingleAsync();
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolTrueDefault);
                Assert.Equal(3, entity.NullableBackedIntNonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolFalseDefault);
                Assert.Equal(5, entity.NullableBackedIntZeroDefault);
            });

    [ConditionalFact]
    public virtual Task Object_fields_get_defaults_when_not_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = WithObjectBackingFields.Create(Fixture.NullableIntSentinel, Fixture.NullableBoolSentinel);
                context.Add(entity);

                await context.SaveChangesAsync();

                Assert.NotEqual(0, entity.Id);
                Assert.True(entity.NullableBackedBoolTrueDefault);
                Assert.Equal(-1, entity.NullableBackedIntNonZeroDefault);
                Assert.False(entity.NullableBackedBoolFalseDefault);
                Assert.Equal(0, entity.NullableBackedIntZeroDefault);
            },
            async context =>
            {
                var entity = await context.Set<WithObjectBackingFields>().SingleAsync();
                Assert.True(entity.NullableBackedBoolTrueDefault);
                Assert.Equal(-1, entity.NullableBackedIntNonZeroDefault);
                Assert.False(entity.NullableBackedBoolFalseDefault);
                Assert.Equal(0, entity.NullableBackedIntZeroDefault);
            });

    [ConditionalFact]
    public virtual Task Object_fields_store_non_defaults_when_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = WithObjectBackingFields.Create(Fixture.NullableIntSentinel, Fixture.NullableBoolSentinel);
                entity.NullableBackedBoolTrueDefault = Fixture.BoolSentinel;
                entity.NullableBackedIntNonZeroDefault = Fixture.IntSentinel;
                entity.NullableBackedBoolFalseDefault = !Fixture.BoolSentinel;
                entity.NullableBackedIntZeroDefault = Fixture.IntSentinel + 1;

                context.Add(entity);

                await context.SaveChangesAsync();

                Assert.NotEqual(0, entity.Id);
                Assert.Equal(Fixture.BoolSentinel, entity.NullableBackedBoolTrueDefault);
                Assert.Equal(Fixture.IntSentinel, entity.NullableBackedIntNonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolFalseDefault);
                Assert.Equal(Fixture.IntSentinel + 1, entity.NullableBackedIntZeroDefault);
            },
            async context =>
            {
                var entity = await context.Set<WithObjectBackingFields>().SingleAsync();
                Assert.Equal(Fixture.BoolSentinel, entity.NullableBackedBoolTrueDefault);
                Assert.Equal(Fixture.IntSentinel, entity.NullableBackedIntNonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolFalseDefault);
                Assert.Equal(Fixture.IntSentinel + 1, entity.NullableBackedIntZeroDefault);
            });

    [ConditionalFact]
    public virtual Task Object_fields_store_any_value_when_set()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = WithObjectBackingFields.Create(Fixture.NullableIntSentinel, Fixture.NullableBoolSentinel);
                entity.NullableBackedBoolTrueDefault = !Fixture.BoolSentinel;
                entity.NullableBackedIntNonZeroDefault = 3;
                entity.NullableBackedBoolFalseDefault = !Fixture.BoolSentinel;
                entity.NullableBackedIntZeroDefault = 5;

                context.Add(entity);

                await context.SaveChangesAsync();

                Assert.NotEqual(0, entity.Id);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolTrueDefault);
                Assert.Equal(3, entity.NullableBackedIntNonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolFalseDefault);
                Assert.Equal(5, entity.NullableBackedIntZeroDefault);
            },
            async context =>
            {
                var entity = await context.Set<WithObjectBackingFields>().SingleAsync();
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolTrueDefault);
                Assert.Equal(3, entity.NullableBackedIntNonZeroDefault);
                Assert.Equal(!Fixture.BoolSentinel, entity.NullableBackedBoolFalseDefault);
                Assert.Equal(5, entity.NullableBackedIntZeroDefault);
            });

    protected class Darwin
    {
        public int? _id;

        public int Id
        {
            get => _id ?? 0;
            set => _id = value;
        }

        public string? Name { get; set; }

        public ICollection<Species> MixedMetaphors { get; set; } = null!;
        public Species? Species { get; set; }
    }

    protected class Species
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public int? DarwinId { get; set; }
        public int? MetaphoricId { get; set; }
    }

    protected class Gumball
    {
        public static Gumball Create(int intSentinel, string? stringSentinel)
            => new()
            {
                Id = intSentinel,
                NotStoreGenerated = stringSentinel,
                Identity = stringSentinel,
                IdentityReadOnlyBeforeSave = stringSentinel,
                IdentityReadOnlyAfterSave = stringSentinel,
                AlwaysIdentity = stringSentinel,
                AlwaysIdentityReadOnlyBeforeSave = stringSentinel,
                AlwaysIdentityReadOnlyAfterSave = stringSentinel,
                Computed = stringSentinel,
                ComputedReadOnlyBeforeSave = stringSentinel,
                ComputedReadOnlyAfterSave = stringSentinel,
                AlwaysComputed = stringSentinel,
                AlwaysComputedReadOnlyBeforeSave = stringSentinel,
                AlwaysComputedReadOnlyAfterSave = stringSentinel
            };

        private Gumball()
        {
        }

        public int Id { get; set; }
        public string? NotStoreGenerated { get; set; }

        public string? Identity { get; set; }
        public string? IdentityReadOnlyBeforeSave { get; set; }
        public string? IdentityReadOnlyAfterSave { get; set; }

        public string? AlwaysIdentity { get; set; }
        public string? AlwaysIdentityReadOnlyBeforeSave { get; set; }
        public string? AlwaysIdentityReadOnlyAfterSave { get; set; }

        public string? Computed { get; set; }
        public string? ComputedReadOnlyBeforeSave { get; set; }
        public string? ComputedReadOnlyAfterSave { get; set; }

        public string? AlwaysComputed { get; set; }
        public string? AlwaysComputedReadOnlyBeforeSave { get; set; }
        public string? AlwaysComputedReadOnlyAfterSave { get; set; }
    }

    protected class Anais
    {
        public static Anais Create(int intSentinel, string? stringSentinel)
            => new()
            {
                Id = intSentinel,
                Never = stringSentinel,
                NeverUseBeforeUseAfter = stringSentinel,
                NeverIgnoreBeforeUseAfter = stringSentinel,
                NeverThrowBeforeUseAfter = stringSentinel,
                NeverUseBeforeIgnoreAfter = stringSentinel,
                NeverIgnoreBeforeIgnoreAfter = stringSentinel,
                NeverThrowBeforeIgnoreAfter = stringSentinel,
                NeverUseBeforeThrowAfter = stringSentinel,
                NeverIgnoreBeforeThrowAfter = stringSentinel,
                NeverThrowBeforeThrowAfter = stringSentinel,
                OnAdd = stringSentinel,
                OnAddUseBeforeUseAfter = stringSentinel,
                OnAddIgnoreBeforeUseAfter = stringSentinel,
                OnAddThrowBeforeUseAfter = stringSentinel,
                OnAddUseBeforeIgnoreAfter = stringSentinel,
                OnAddIgnoreBeforeIgnoreAfter = stringSentinel,
                OnAddThrowBeforeIgnoreAfter = stringSentinel,
                OnAddUseBeforeThrowAfter = stringSentinel,
                OnAddIgnoreBeforeThrowAfter = stringSentinel,
                OnAddThrowBeforeThrowAfter = stringSentinel,
                OnAddOrUpdate = stringSentinel,
                OnAddOrUpdateUseBeforeUseAfter = stringSentinel,
                OnAddOrUpdateIgnoreBeforeUseAfter = stringSentinel,
                OnAddOrUpdateThrowBeforeUseAfter = stringSentinel,
                OnAddOrUpdateUseBeforeIgnoreAfter = stringSentinel,
                OnAddOrUpdateIgnoreBeforeIgnoreAfter = stringSentinel,
                OnAddOrUpdateThrowBeforeIgnoreAfter = stringSentinel,
                OnAddOrUpdateUseBeforeThrowAfter = stringSentinel,
                OnAddOrUpdateIgnoreBeforeThrowAfter = stringSentinel,
                OnAddOrUpdateThrowBeforeThrowAfter = stringSentinel,
                OnUpdate = stringSentinel,
                OnUpdateUseBeforeUseAfter = stringSentinel,
                OnUpdateIgnoreBeforeUseAfter = stringSentinel,
                OnUpdateThrowBeforeUseAfter = stringSentinel,
                OnUpdateUseBeforeIgnoreAfter = stringSentinel,
                OnUpdateIgnoreBeforeIgnoreAfter = stringSentinel,
                OnUpdateThrowBeforeIgnoreAfter = stringSentinel,
                OnUpdateUseBeforeThrowAfter = stringSentinel,
                OnUpdateIgnoreBeforeThrowAfter = stringSentinel,
                OnUpdateThrowBeforeThrowAfter = stringSentinel,
            };

        private Anais()
        {
        }

        public int Id { get; set; }
        public string? Never { get; set; }
        public string? NeverUseBeforeUseAfter { get; set; }
        public string? NeverIgnoreBeforeUseAfter { get; set; }
        public string? NeverThrowBeforeUseAfter { get; set; }
        public string? NeverUseBeforeIgnoreAfter { get; set; }
        public string? NeverIgnoreBeforeIgnoreAfter { get; set; }
        public string? NeverThrowBeforeIgnoreAfter { get; set; }
        public string? NeverUseBeforeThrowAfter { get; set; }
        public string? NeverIgnoreBeforeThrowAfter { get; set; }
        public string? NeverThrowBeforeThrowAfter { get; set; }

        public string? OnAdd { get; set; }
        public string? OnAddUseBeforeUseAfter { get; set; }
        public string? OnAddIgnoreBeforeUseAfter { get; set; }
        public string? OnAddThrowBeforeUseAfter { get; set; }
        public string? OnAddUseBeforeIgnoreAfter { get; set; }
        public string? OnAddIgnoreBeforeIgnoreAfter { get; set; }
        public string? OnAddThrowBeforeIgnoreAfter { get; set; }
        public string? OnAddUseBeforeThrowAfter { get; set; }
        public string? OnAddIgnoreBeforeThrowAfter { get; set; }
        public string? OnAddThrowBeforeThrowAfter { get; set; }

        public string? OnAddOrUpdate { get; set; }
        public string? OnAddOrUpdateUseBeforeUseAfter { get; set; }
        public string? OnAddOrUpdateIgnoreBeforeUseAfter { get; set; }
        public string? OnAddOrUpdateThrowBeforeUseAfter { get; set; }
        public string? OnAddOrUpdateUseBeforeIgnoreAfter { get; set; }
        public string? OnAddOrUpdateIgnoreBeforeIgnoreAfter { get; set; }
        public string? OnAddOrUpdateThrowBeforeIgnoreAfter { get; set; }
        public string? OnAddOrUpdateUseBeforeThrowAfter { get; set; }
        public string? OnAddOrUpdateIgnoreBeforeThrowAfter { get; set; }
        public string? OnAddOrUpdateThrowBeforeThrowAfter { get; set; }

        public string? OnUpdate { get; set; }
        public string? OnUpdateUseBeforeUseAfter { get; set; }
        public string? OnUpdateIgnoreBeforeUseAfter { get; set; }
        public string? OnUpdateThrowBeforeUseAfter { get; set; }
        public string? OnUpdateUseBeforeIgnoreAfter { get; set; }
        public string? OnUpdateIgnoreBeforeIgnoreAfter { get; set; }
        public string? OnUpdateThrowBeforeIgnoreAfter { get; set; }
        public string? OnUpdateUseBeforeThrowAfter { get; set; }
        public string? OnUpdateIgnoreBeforeThrowAfter { get; set; }
        public string? OnUpdateThrowBeforeThrowAfter { get; set; }
    }

    protected class WithBackingFields
    {
        public static WithBackingFields Create(int intSentinel, int? nullableIntSentinel)
        {
            var entity = new WithBackingFields();
            entity._id = intSentinel;
            entity._nullableAsNonNullable = nullableIntSentinel;
            entity._nonNullableAsNullable = intSentinel;
            return entity;
        }

        private WithBackingFields()
        {
        }

#pragma warning disable RCS1085 // Use auto-implemented property.
        // ReSharper disable ConvertToAutoProperty
        private int _id;

        public int Id
        {
            get => _id;
            set => _id = value;
        }
        // ReSharper restore ConvertToAutoProperty
#pragma warning restore RCS1085 // Use auto-implemented property.

        private int? _nullableAsNonNullable = 0;

        public int NullableAsNonNullable
        {
            get => (int)_nullableAsNonNullable!;
            set => _nullableAsNonNullable = value;
        }

        private int _nonNullableAsNullable;

        public int? NonNullableAsNullable
        {
            get => _nonNullableAsNullable;
            set => _nonNullableAsNullable = value ?? 0;
        }
    }

    protected class WithNoBackingFields
    {
        public int Id { get; set; }
        public bool TrueDefault { get; set; }
        public int NonZeroDefault { get; set; }
        public bool FalseDefault { get; set; }
        public int ZeroDefault { get; set; }
    }

    protected class WithNullableBackingFields
    {
        public static WithNullableBackingFields Create(int? intSentinel, bool? boolSentinel)
        {
            var entity = new WithNullableBackingFields();
            entity._id = intSentinel;
            entity._nullableBackedBoolTrueDefault = boolSentinel;
            entity._nullableBackedIntNonZeroDefault = intSentinel;
            entity._nullableBackedBoolFalseDefault = boolSentinel;
            entity._nullableBackedIntZeroDefault = intSentinel;
            return entity;
        }

        private WithNullableBackingFields()
        {
        }

        private int? _id;

        public int Id
        {
            get => _id ?? throw new Exception("Bang!");
            set => _id = value;
        }

        private bool? _nullableBackedBoolTrueDefault;

        public bool NullableBackedBoolTrueDefault
        {
            get => _nullableBackedBoolTrueDefault ?? true;
            set => _nullableBackedBoolTrueDefault = value;
        }

        private int? _nullableBackedIntNonZeroDefault;

        public int NullableBackedIntNonZeroDefault
        {
            get => _nullableBackedIntNonZeroDefault ?? -1;
            set => _nullableBackedIntNonZeroDefault = value;
        }

        private bool? _nullableBackedBoolFalseDefault;

        public bool NullableBackedBoolFalseDefault
        {
            get => _nullableBackedBoolFalseDefault ?? false;
            set => _nullableBackedBoolFalseDefault = value;
        }

        private int? _nullableBackedIntZeroDefault;

        public int NullableBackedIntZeroDefault
        {
            get => _nullableBackedIntZeroDefault ?? 0;
            set => _nullableBackedIntZeroDefault = value;
        }
    }

    protected class WithObjectBackingFields
    {
        public static WithObjectBackingFields Create(int? intSentinel, bool? boolSentinel)
        {
            var entity = new WithObjectBackingFields();
            entity._id = intSentinel;
            entity._nullableBackedBoolTrueDefault = boolSentinel;
            entity._nullableBackedIntNonZeroDefault = intSentinel;
            entity._nullableBackedBoolFalseDefault = boolSentinel;
            entity._nullableBackedIntZeroDefault = intSentinel;
            return entity;
        }

        private WithObjectBackingFields()
        {
        }

        private object? _id;

        public int Id
        {
            get => (int)(_id ?? 0);
            set => _id = value;
        }

        private object? _nullableBackedBoolTrueDefault;

        public bool NullableBackedBoolTrueDefault
        {
            get => (bool)(_nullableBackedBoolTrueDefault ?? throw new Exception("Bang!"));
            set => _nullableBackedBoolTrueDefault = value;
        }

        private object? _nullableBackedIntNonZeroDefault;

        public int NullableBackedIntNonZeroDefault
        {
            get => (int)(_nullableBackedIntNonZeroDefault ?? throw new Exception("Bang!"));
            set => _nullableBackedIntNonZeroDefault = value;
        }

        private object? _nullableBackedBoolFalseDefault;

        public bool NullableBackedBoolFalseDefault
        {
            get => (bool)(_nullableBackedBoolFalseDefault ?? false);
            set => _nullableBackedBoolFalseDefault = value;
        }

        private object? _nullableBackedIntZeroDefault;

        public int NullableBackedIntZeroDefault
        {
            get => (int)(_nullableBackedIntZeroDefault ?? 0);
            set => _nullableBackedIntZeroDefault = value;
        }
    }

    protected class WithConverter<TKey>
    {
        public TKey? Id { get; set; }
    }

    protected class IntToString : WithConverter<int>;

    protected class GuidToString : WithConverter<Guid>;

    protected class GuidToBytes : WithConverter<Guid>;

    protected class ShortToBytes : WithConverter<short>;

    public class WrappedIntClass
    {
        public int Value { get; set; }
    }

    protected class WrappedIntClassConverter : ValueConverter<WrappedIntClass, int>
    {
        public WrappedIntClassConverter()
            : base(
                v => v.Value,
                v => new WrappedIntClass { Value = v })
        {
        }
    }

    protected class WrappedIntClassComparer : ValueComparer<WrappedIntClass?>
    {
        public WrappedIntClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value : 0,
                v => v == null ? null : new WrappedIntClass { Value = v.Value })
        {
        }
    }

    protected class WrappedIntClassValueGenerator : ValueGenerator<WrappedIntClass>
    {
        public override WrappedIntClass Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public struct WrappedIntStruct
    {
        public int Value { get; set; }
    }

    protected class WrappedIntStructConverter : ValueConverter<WrappedIntStruct, int>
    {
        public WrappedIntStructConverter()
            : base(
                v => v.Value,
                v => new WrappedIntStruct { Value = v })
        {
        }
    }

    protected class WrappedIntStructValueGenerator : ValueGenerator<WrappedIntStruct>
    {
        public override WrappedIntStruct Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public record WrappedIntRecord
    {
        public int Value { get; set; }
    }

    protected class WrappedIntRecordConverter : ValueConverter<WrappedIntRecord, int>
    {
        public WrappedIntRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedIntRecord { Value = v })
        {
        }
    }

    protected class WrappedIntRecordValueGenerator : ValueGenerator<WrappedIntRecord>
    {
        public override WrappedIntRecord Next(EntityEntry entry)
            => new() { Value = 66 };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public class WrappedIntKeyClass
    {
        public int Value { get; set; }
    }

    protected class WrappedIntKeyClassConverter : ValueConverter<WrappedIntKeyClass, int>
    {
        public WrappedIntKeyClassConverter()
            : base(
                v => v.Value,
                v => new WrappedIntKeyClass { Value = v })
        {
        }
    }

    protected class WrappedIntKeyClassComparer : ValueComparer<WrappedIntKeyClass?>
    {
        public WrappedIntKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value : 0,
                v => v == null ? null : new WrappedIntKeyClass { Value = v.Value })
        {
        }
    }

    public struct WrappedIntKeyStruct
    {
        public int Value { get; set; }

        public override bool Equals(object? obj)
            => obj is WrappedIntKeyStruct other && Value == other.Value;

        public override int GetHashCode()
            => Value;

        public static bool operator ==(WrappedIntKeyStruct left, WrappedIntKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedIntKeyStruct left, WrappedIntKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedIntKeyStructConverter : ValueConverter<WrappedIntKeyStruct, int>
    {
        public WrappedIntKeyStructConverter()
            : base(
                v => v.Value,
                v => new WrappedIntKeyStruct { Value = v })
        {
        }
    }

    public record WrappedIntKeyRecord
    {
        public int Value { get; set; }
    }

    protected class WrappedIntKeyRecordConverter : ValueConverter<WrappedIntKeyRecord, int>
    {
        public WrappedIntKeyRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedIntKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedIntClassPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntKeyClass Id { get; set; } = null!;

        public WrappedIntClass? NonKey { get; set; }
        public ICollection<WrappedIntClassDependentShadow> Dependents { get; } = new List<WrappedIntClassDependentShadow>();
        public ICollection<WrappedIntClassDependentRequired> RequiredDependents { get; } = new List<WrappedIntClassDependentRequired>();
        public ICollection<WrappedIntClassDependentOptional> OptionalDependents { get; } = new List<WrappedIntClassDependentOptional>();
    }

    protected class WrappedIntClassDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntClass Id { get; set; } = null!;

        public WrappedIntClassPrincipal? Principal { get; set; }
    }

    protected class WrappedIntClassDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntClass Id { get; set; } = null!;

        public WrappedIntKeyClass PrincipalId { get; set; } = null!;
        public WrappedIntClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedIntClassDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntClass Id { get; set; } = null!;

        public WrappedIntKeyClass? PrincipalId { get; set; }
        public WrappedIntClassPrincipal? Principal { get; set; }
    }

    protected class WrappedIntStructPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntKeyStruct Id { get; set; }

        public WrappedIntStruct NonKey { get; set; }
        public ICollection<WrappedIntStructDependentShadow> Dependents { get; } = new List<WrappedIntStructDependentShadow>();
        public ICollection<WrappedIntStructDependentOptional> OptionalDependents { get; } = new List<WrappedIntStructDependentOptional>();
        public ICollection<WrappedIntStructDependentRequired> RequiredDependents { get; } = new List<WrappedIntStructDependentRequired>();
    }

    protected class WrappedIntStructDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntStruct Id { get; set; }

        public WrappedIntStructPrincipal? Principal { get; set; }
    }

    protected class WrappedIntStructDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntStruct Id { get; set; }

        public WrappedIntKeyStruct? PrincipalId { get; set; }
        public WrappedIntStructPrincipal? Principal { get; set; }
    }

    protected class WrappedIntStructDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntStruct Id { get; set; }

        public WrappedIntKeyStruct PrincipalId { get; set; }
        public WrappedIntStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedIntRecordPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntKeyRecord Id { get; set; } = null!;

        public WrappedIntRecord? NonKey { get; set; }
        public ICollection<WrappedIntRecordDependentShadow> Dependents { get; } = new List<WrappedIntRecordDependentShadow>();
        public ICollection<WrappedIntRecordDependentOptional> OptionalDependents { get; } = new List<WrappedIntRecordDependentOptional>();
        public ICollection<WrappedIntRecordDependentRequired> RequiredDependents { get; } = new List<WrappedIntRecordDependentRequired>();
    }

    protected class WrappedIntRecordDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntRecord Id { get; set; } = null!;

        public WrappedIntRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedIntRecordDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntRecord Id { get; set; } = null!;

        public WrappedIntKeyRecord? PrincipalId { get; set; }
        public WrappedIntRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedIntRecordDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedIntRecord Id { get; set; } = null!;

        public WrappedIntKeyRecord PrincipalId { get; set; } = null!;
        public WrappedIntRecordPrincipal Principal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_wrapped_int_key()
    {
        var id1 = 0;
        var id2 = 0;
        var id3 = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new WrappedIntClassPrincipal
                    {
                        Id = Fixture.WrappedIntKeyClassSentinel!,
                        NonKey = Fixture.WrappedIntClassSentinel,
                        Dependents = { new WrappedIntClassDependentShadow(), new WrappedIntClassDependentShadow() },
                        OptionalDependents = { new WrappedIntClassDependentOptional(), new WrappedIntClassDependentOptional() },
                        RequiredDependents = { new WrappedIntClassDependentRequired(), new WrappedIntClassDependentRequired() }
                    }).Entity;

                var principal2 = context.Add(
                    new WrappedIntStructPrincipal
                    {
                        Id = Fixture.WrappedIntKeyStructSentinel,
                        NonKey = Fixture.WrappedIntStructSentinel,
                        Dependents = { new WrappedIntStructDependentShadow(), new WrappedIntStructDependentShadow() },
                        OptionalDependents = { new WrappedIntStructDependentOptional(), new WrappedIntStructDependentOptional() },
                        RequiredDependents = { new WrappedIntStructDependentRequired(), new WrappedIntStructDependentRequired() }
                    }).Entity;

                var principal3 = context.Add(
                    new WrappedIntRecordPrincipal
                    {
                        Id = Fixture.WrappedIntKeyRecordSentinel!,
                        NonKey = Fixture.WrappedIntRecordSentinel,
                        Dependents = { new WrappedIntRecordDependentShadow(), new WrappedIntRecordDependentShadow() },
                        OptionalDependents = { new WrappedIntRecordDependentOptional(), new WrappedIntRecordDependentOptional() },
                        RequiredDependents = { new WrappedIntRecordDependentRequired(), new WrappedIntRecordDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id.Value;
                Assert.NotEqual(0, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedIntKeyClass?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId.Value);
                }

                Assert.Equal(66, principal1.NonKey!.Value);

                id2 = principal2.Id.Value;
                Assert.NotEqual(0, id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedIntKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
                }

                foreach (var dependent in principal2.OptionalDependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
                }

                foreach (var dependent in principal2.RequiredDependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId.Value);
                }

                Assert.Equal(66, principal2.NonKey.Value);

                id3 = principal3.Id.Value;
                Assert.NotEqual(0, id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedIntKeyRecord?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal3.OptionalDependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal3.RequiredDependents)
                {
                    Assert.NotEqual(0, dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId.Value);
                }

                Assert.Equal(66, principal3.NonKey!.Value);
            },
            async context =>
            {
                var principal1 = await context.Set<WrappedIntClassPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id.Value, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedIntKeyClass?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId.Value);
                }

                var principal2 = await context.Set<WrappedIntStructPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal2.Id.Value, id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedIntKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
                }

                foreach (var dependent in principal2.OptionalDependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
                }

                foreach (var dependent in principal2.RequiredDependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId.Value);
                }

                var principal3 = await context.Set<WrappedIntRecordPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal3.Id.Value, id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedIntKeyRecord?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal3.OptionalDependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal3.RequiredDependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId.Value);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal2.Dependents.Remove(principal2.Dependents.First());
                principal3.Dependents.Remove(principal3.Dependents.First());

                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal2.OptionalDependents.Remove(principal2.OptionalDependents.First());
                principal3.OptionalDependents.Remove(principal3.OptionalDependents.First());

                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
                principal2.RequiredDependents.Remove(principal2.RequiredDependents.First());
                principal3.RequiredDependents.Remove(principal3.RequiredDependents.First());

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<WrappedIntClassDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(
                    context.Entry(dependents1.Single(e => e.Principal == null))
                        .Property<WrappedIntKeyClass?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<WrappedIntClassDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<WrappedIntClassDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                var dependents2 = await context.Set<WrappedIntStructDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents2.Count);
                Assert.Null(
                    context.Entry(dependents2.Single(e => e.Principal == null))
                        .Property<WrappedIntKeyStruct?>("PrincipalId").CurrentValue);

                var optionalDependents2 = await context.Set<WrappedIntStructDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents2.Count);
                Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents2 = await context.Set<WrappedIntStructDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents2);

                var dependents3 = await context.Set<WrappedIntRecordDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents3.Count);
                Assert.Null(
                    context.Entry(dependents3.Single(e => e.Principal == null))
                        .Property<WrappedIntKeyRecord?>("PrincipalId").CurrentValue);

                var optionalDependents3 = await context.Set<WrappedIntRecordDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents3.Count);
                Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents3 = await context.Set<WrappedIntRecordDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents3);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                context.Remove(dependents2.Single(e => e.Principal != null));
                context.Remove(optionalDependents2.Single(e => e.Principal != null));
                context.Remove(requiredDependents2.Single());
                context.Remove(requiredDependents2.Single().Principal);

                context.Remove(dependents3.Single(e => e.Principal != null));
                context.Remove(optionalDependents3.Single(e => e.Principal != null));
                context.Remove(requiredDependents3.Single());
                context.Remove(requiredDependents3.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<WrappedIntClassDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedIntStructDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedIntRecordDependentShadow>().CountAsync());

                Assert.Equal(1, await context.Set<WrappedIntClassDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedIntStructDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedIntRecordDependentOptional>().CountAsync());

                Assert.Equal(0, await context.Set<WrappedIntClassDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedIntStructDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedIntRecordDependentRequired>().CountAsync());
            });
    }

    protected class LongToIntPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public ICollection<LongToIntDependentShadow> Dependents { get; } = new List<LongToIntDependentShadow>();
        public ICollection<LongToIntDependentRequired> RequiredDependents { get; } = new List<LongToIntDependentRequired>();
        public ICollection<LongToIntDependentOptional> OptionalDependents { get; } = new List<LongToIntDependentOptional>();
    }

    protected class LongToIntDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public LongToIntPrincipal? Principal { get; set; }
    }

    protected class LongToIntDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long PrincipalId { get; set; }
        public LongToIntPrincipal Principal { get; set; } = null!;
    }

    protected class LongToIntDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? PrincipalId { get; set; }
        public LongToIntPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_long_to_int_conversion()
    {
        var id1 = 0L;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new LongToIntPrincipal
                    {
                        Id = Fixture.LongSentinel,
                        Dependents = { new LongToIntDependentShadow(), new LongToIntDependentShadow() },
                        OptionalDependents = { new LongToIntDependentOptional(), new LongToIntDependentOptional() },
                        RequiredDependents = { new LongToIntDependentRequired(), new LongToIntDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id;
                Assert.NotEqual(0L, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.NotEqual(0L, dependent.Id);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<long?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.NotEqual(0L, dependent.Id);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.NotEqual(0L, dependent.Id);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId);
                }
            },
            async context =>
            {
                var principal1 = await context.Set<LongToIntPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<long?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<LongToIntDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(
                    context.Entry(dependents1.Single(e => e.Principal == null))
                        .Property<long?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<LongToIntDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<LongToIntDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<LongToIntDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<LongToIntDependentOptional>().CountAsync());
                Assert.Equal(0, await context.Set<LongToIntDependentRequired>().CountAsync());
            });
    }

    public class WrappedStringClass
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringClassConverter : ValueConverter<WrappedStringClass, string>
    {
        public WrappedStringClassConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringClass { Value = v })
        {
        }
    }

    protected class WrappedStringClassComparer : ValueComparer<WrappedStringClass?>
    {
        public WrappedStringClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value!.Equals(v2.Value)),
                v => v != null ? v.Value!.GetHashCode() : 0,
                v => v == null ? null : new WrappedStringClass { Value = v.Value })
        {
        }
    }

    protected class WrappedStringClassValueGenerator : ValueGenerator<WrappedStringClass>
    {
        public override WrappedStringClass Next(EntityEntry entry)
            => new() { Value = "66" };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public struct WrappedStringStruct
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringStructConverter : ValueConverter<WrappedStringStruct, string>
    {
        public WrappedStringStructConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringStruct { Value = v })
        {
        }
    }

    protected class WrappedStringStructValueGenerator : ValueGenerator<WrappedStringStruct>
    {
        public override WrappedStringStruct Next(EntityEntry entry)
            => new() { Value = "66" };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public record WrappedStringRecord
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringRecordConverter : ValueConverter<WrappedStringRecord, string>
    {
        public WrappedStringRecordConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringRecord { Value = v })
        {
        }
    }

    protected class WrappedStringRecordValueGenerator : ValueGenerator<WrappedStringRecord>
    {
        public override WrappedStringRecord Next(EntityEntry entry)
            => new() { Value = "66" };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public class WrappedStringKeyClass
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringKeyClassConverter : ValueConverter<WrappedStringKeyClass, string>
    {
        public WrappedStringKeyClassConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringKeyClass { Value = v })
        {
        }
    }

    protected class WrappedStringKeyClassComparer : ValueComparer<WrappedStringKeyClass?>
    {
        public WrappedStringKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value!.Equals(v2.Value)),
                v => v != null ? v.Value!.GetHashCode() : 0,
                v => v == null ? null : new WrappedStringKeyClass { Value = v.Value })
        {
        }
    }

    public struct WrappedStringKeyStruct
    {
        public string Value { get; set; }

        public override bool Equals(object? obj)
            => obj is WrappedStringKeyStruct other && Value == other.Value;

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(WrappedStringKeyStruct left, WrappedStringKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedStringKeyStruct left, WrappedStringKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedStringKeyStructConverter : ValueConverter<WrappedStringKeyStruct, string>
    {
        public WrappedStringKeyStructConverter()
            : base(
                v => v.Value,
                v => new WrappedStringKeyStruct { Value = v })
        {
        }
    }

    public record WrappedStringKeyRecord
    {
        public string? Value { get; set; }
    }

    protected class WrappedStringKeyRecordConverter : ValueConverter<WrappedStringKeyRecord, string>
    {
        public WrappedStringKeyRecordConverter()
            : base(
                v => v.Value!,
                v => new WrappedStringKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedStringClassPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringKeyClass Id { get; set; } = null!;

        public WrappedStringClass? NonKey { get; set; }
        public ICollection<WrappedStringClassDependentShadow> Dependents { get; } = new List<WrappedStringClassDependentShadow>();

        public ICollection<WrappedStringClassDependentRequired> RequiredDependents { get; } =
            new List<WrappedStringClassDependentRequired>();

        public ICollection<WrappedStringClassDependentOptional> OptionalDependents { get; } =
            new List<WrappedStringClassDependentOptional>();
    }

    protected class WrappedStringClassDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringClass Id { get; set; } = null!;

        public WrappedStringClassPrincipal? Principal { get; set; }
    }

    protected class WrappedStringClassDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringClass Id { get; set; } = null!;

        public WrappedStringKeyClass PrincipalId { get; set; } = null!;
        public WrappedStringClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedStringClassDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringClass Id { get; set; } = null!;

        public WrappedStringKeyClass? PrincipalId { get; set; }
        public WrappedStringClassPrincipal? Principal { get; set; }
    }

    protected class WrappedStringStructPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringKeyStruct Id { get; set; }

        public WrappedStringStruct NonKey { get; set; }
        public ICollection<WrappedStringStructDependentShadow> Dependents { get; } = new List<WrappedStringStructDependentShadow>();

        public ICollection<WrappedStringStructDependentOptional> OptionalDependents { get; } =
            new List<WrappedStringStructDependentOptional>();

        public ICollection<WrappedStringStructDependentRequired> RequiredDependents { get; } =
            new List<WrappedStringStructDependentRequired>();
    }

    protected class WrappedStringStructDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringStruct Id { get; set; }

        public WrappedStringStructPrincipal? Principal { get; set; }
    }

    protected class WrappedStringStructDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringStruct Id { get; set; }

        public WrappedStringKeyStruct? PrincipalId { get; set; }
        public WrappedStringStructPrincipal? Principal { get; set; }
    }

    protected class WrappedStringStructDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringStruct Id { get; set; }

        public WrappedStringKeyStruct PrincipalId { get; set; }
        public WrappedStringStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedStringRecordPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringKeyRecord Id { get; set; } = null!;

        public WrappedStringRecord? NonKey { get; set; }
        public ICollection<WrappedStringRecordDependentShadow> Dependents { get; } = new List<WrappedStringRecordDependentShadow>();

        public ICollection<WrappedStringRecordDependentOptional> OptionalDependents { get; } =
            new List<WrappedStringRecordDependentOptional>();

        public ICollection<WrappedStringRecordDependentRequired> RequiredDependents { get; } =
            new List<WrappedStringRecordDependentRequired>();
    }

    protected class WrappedStringRecordDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringRecord Id { get; set; } = null!;

        public WrappedStringRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedStringRecordDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringRecord Id { get; set; } = null!;

        public WrappedStringKeyRecord? PrincipalId { get; set; }
        public WrappedStringRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedStringRecordDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedStringRecord Id { get; set; } = null!;

        public WrappedStringKeyRecord PrincipalId { get; set; } = null!;
        public WrappedStringRecordPrincipal Principal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_wrapped_string_key()
    {
        string? id1 = null;
        string? id2 = null;
        string? id3 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new WrappedStringClassPrincipal
                    {
                        Id = Fixture.WrappedStringKeyClassSentinel!,
                        NonKey = Fixture.WrappedStringClassSentinel,
                        Dependents = { new WrappedStringClassDependentShadow(), new WrappedStringClassDependentShadow() },
                        OptionalDependents = { new WrappedStringClassDependentOptional(), new WrappedStringClassDependentOptional() },
                        RequiredDependents = { new WrappedStringClassDependentRequired(), new WrappedStringClassDependentRequired() }
                    }).Entity;

                var principal2 = context.Add(
                    new WrappedStringStructPrincipal
                    {
                        Id = Fixture.WrappedStringKeyStructSentinel,
                        NonKey = Fixture.WrappedStringStructSentinel,
                        Dependents = { new WrappedStringStructDependentShadow(), new WrappedStringStructDependentShadow() },
                        OptionalDependents = { new WrappedStringStructDependentOptional(), new WrappedStringStructDependentOptional() },
                        RequiredDependents = { new WrappedStringStructDependentRequired(), new WrappedStringStructDependentRequired() }
                    }).Entity;

                var principal3 = context.Add(
                    new WrappedStringRecordPrincipal
                    {
                        Id = Fixture.WrappedStringKeyRecordSentinel!,
                        NonKey = Fixture.WrappedStringRecordSentinel,
                        Dependents = { new WrappedStringRecordDependentShadow(), new WrappedStringRecordDependentShadow() },
                        OptionalDependents = { new WrappedStringRecordDependentOptional(), new WrappedStringRecordDependentOptional() },
                        RequiredDependents = { new WrappedStringRecordDependentRequired(), new WrappedStringRecordDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id.Value;
                Assert.NotNull(id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedStringKeyClass?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId.Value);
                }

                Assert.Equal("66", principal1.NonKey!.Value);

                id2 = principal2.Id.Value;
                Assert.NotNull(id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedStringKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
                }

                foreach (var dependent in principal2.OptionalDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
                }

                foreach (var dependent in principal2.RequiredDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId.Value);
                }

                Assert.Equal("66", principal2.NonKey.Value);

                id3 = principal3.Id.Value;
                Assert.NotNull(id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedStringKeyRecord?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal3.OptionalDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal3.RequiredDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId.Value);
                }

                Assert.Equal("66", principal3.NonKey!.Value);
            },
            async context =>
            {
                var principal1 = await context.Set<WrappedStringClassPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id.Value, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedStringKeyClass?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId.Value);
                }

                var principal2 = await context.Set<WrappedStringStructPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal2.Id.Value, id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedStringKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
                }

                foreach (var dependent in principal2.OptionalDependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
                }

                foreach (var dependent in principal2.RequiredDependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId.Value);
                }

                var principal3 = await context.Set<WrappedStringRecordPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal3.Id.Value, id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedStringKeyRecord?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal3.OptionalDependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal3.RequiredDependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId.Value);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal2.Dependents.Remove(principal2.Dependents.First());
                principal3.Dependents.Remove(principal3.Dependents.First());

                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal2.OptionalDependents.Remove(principal2.OptionalDependents.First());
                principal3.OptionalDependents.Remove(principal3.OptionalDependents.First());

                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
                principal2.RequiredDependents.Remove(principal2.RequiredDependents.First());
                principal3.RequiredDependents.Remove(principal3.RequiredDependents.First());

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<WrappedStringClassDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(
                    context.Entry(dependents1.Single(e => e.Principal == null))
                        .Property<WrappedStringKeyClass?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<WrappedStringClassDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<WrappedStringClassDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                var dependents2 = await context.Set<WrappedStringStructDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents2.Count);
                Assert.Null(
                    context.Entry(dependents2.Single(e => e.Principal == null))
                        .Property<WrappedStringKeyStruct?>("PrincipalId").CurrentValue);

                var optionalDependents2 = await context.Set<WrappedStringStructDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents2.Count);
                Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents2 = await context.Set<WrappedStringStructDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents2);

                var dependents3 = await context.Set<WrappedStringRecordDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents3.Count);
                Assert.Null(
                    context.Entry(dependents3.Single(e => e.Principal == null))
                        .Property<WrappedStringKeyRecord?>("PrincipalId").CurrentValue);

                var optionalDependents3 = await context.Set<WrappedStringRecordDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents3.Count);
                Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents3 = await context.Set<WrappedStringRecordDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents3);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                context.Remove(dependents2.Single(e => e.Principal != null));
                context.Remove(optionalDependents2.Single(e => e.Principal != null));
                context.Remove(requiredDependents2.Single());
                context.Remove(requiredDependents2.Single().Principal);

                context.Remove(dependents3.Single(e => e.Principal != null));
                context.Remove(optionalDependents3.Single(e => e.Principal != null));
                context.Remove(requiredDependents3.Single());
                context.Remove(requiredDependents3.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<WrappedStringClassDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedStringStructDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedStringRecordDependentShadow>().CountAsync());

                Assert.Equal(1, await context.Set<WrappedStringClassDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedStringStructDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedStringRecordDependentOptional>().CountAsync());

                Assert.Equal(0, await context.Set<WrappedStringClassDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedStringStructDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedStringRecordDependentRequired>().CountAsync());
            });
    }

    // ReSharper disable once StaticMemberInGenericType
    protected static readonly Guid KnownGuid = Guid.Parse("E871CEA4-8DBE-4269-99F4-87F7128AF399");

    public class WrappedGuidClass
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidClassConverter : ValueConverter<WrappedGuidClass, Guid>
    {
        public WrappedGuidClassConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidClass { Value = v })
        {
        }
    }

    protected class WrappedGuidClassComparer : ValueComparer<WrappedGuidClass?>
    {
        public WrappedGuidClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value.GetHashCode() : 0,
                v => v == null ? null : new WrappedGuidClass { Value = v.Value })
        {
        }
    }

    protected class WrappedGuidClassValueGenerator : ValueGenerator<WrappedGuidClass>
    {
        public override WrappedGuidClass Next(EntityEntry entry)
            => new() { Value = KnownGuid };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public struct WrappedGuidStruct
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidStructConverter : ValueConverter<WrappedGuidStruct, Guid>
    {
        public WrappedGuidStructConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidStruct { Value = v })
        {
        }
    }

    protected class WrappedGuidStructValueGenerator : ValueGenerator<WrappedGuidStruct>
    {
        public override WrappedGuidStruct Next(EntityEntry entry)
            => new() { Value = KnownGuid };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public record WrappedGuidRecord
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidRecordConverter : ValueConverter<WrappedGuidRecord, Guid>
    {
        public WrappedGuidRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidRecord { Value = v })
        {
        }
    }

    protected class WrappedGuidRecordValueGenerator : ValueGenerator<WrappedGuidRecord>
    {
        public override WrappedGuidRecord Next(EntityEntry entry)
            => new() { Value = KnownGuid };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public class WrappedGuidKeyClass
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidKeyClassConverter : ValueConverter<WrappedGuidKeyClass, Guid>
    {
        public WrappedGuidKeyClassConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidKeyClass { Value = v })
        {
        }
    }

    protected class WrappedGuidKeyClassComparer : ValueComparer<WrappedGuidKeyClass?>
    {
        public WrappedGuidKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value.Equals(v2.Value)),
                v => v != null ? v.Value.GetHashCode() : 0,
                v => v == null ? null : new WrappedGuidKeyClass { Value = v.Value })
        {
        }
    }

    public struct WrappedGuidKeyStruct
    {
        public Guid Value { get; set; }

        public override bool Equals(object? obj)
            => obj is WrappedGuidKeyStruct other && Value.Equals(other.Value);

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(WrappedGuidKeyStruct left, WrappedGuidKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedGuidKeyStruct left, WrappedGuidKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedGuidKeyStructConverter : ValueConverter<WrappedGuidKeyStruct, Guid>
    {
        public WrappedGuidKeyStructConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidKeyStruct { Value = v })
        {
        }
    }

    public record WrappedGuidKeyRecord
    {
        public Guid Value { get; set; }
    }

    protected class WrappedGuidKeyRecordConverter : ValueConverter<WrappedGuidKeyRecord, Guid>
    {
        public WrappedGuidKeyRecordConverter()
            : base(
                v => v.Value,
                v => new WrappedGuidKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedGuidClassPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidKeyClass Id { get; set; } = null!;

        public WrappedGuidClass? NonKey { get; set; }
        public ICollection<WrappedGuidClassDependentShadow> Dependents { get; } = new List<WrappedGuidClassDependentShadow>();
        public ICollection<WrappedGuidClassDependentRequired> RequiredDependents { get; } = new List<WrappedGuidClassDependentRequired>();
        public ICollection<WrappedGuidClassDependentOptional> OptionalDependents { get; } = new List<WrappedGuidClassDependentOptional>();
    }

    protected class WrappedGuidClassDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidClass Id { get; set; } = null!;

        public WrappedGuidClassPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidClassDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidClass Id { get; set; } = null!;

        public WrappedGuidKeyClass PrincipalId { get; set; } = null!;
        public WrappedGuidClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedGuidClassDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidClass Id { get; set; } = null!;

        public WrappedGuidKeyClass? PrincipalId { get; set; }
        public WrappedGuidClassPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidStructPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidKeyStruct Id { get; set; }

        public WrappedGuidStruct NonKey { get; set; }
        public ICollection<WrappedGuidStructDependentShadow> Dependents { get; } = new List<WrappedGuidStructDependentShadow>();
        public ICollection<WrappedGuidStructDependentOptional> OptionalDependents { get; } = new List<WrappedGuidStructDependentOptional>();
        public ICollection<WrappedGuidStructDependentRequired> RequiredDependents { get; } = new List<WrappedGuidStructDependentRequired>();
    }

    protected class WrappedGuidStructDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidStruct Id { get; set; }

        public WrappedGuidStructPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidStructDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidStruct Id { get; set; }

        public WrappedGuidKeyStruct? PrincipalId { get; set; }
        public WrappedGuidStructPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidStructDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidStruct Id { get; set; }

        public WrappedGuidKeyStruct PrincipalId { get; set; }
        public WrappedGuidStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedGuidRecordPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidKeyRecord Id { get; set; } = null!;

        public WrappedGuidRecord? NonKey { get; set; }
        public ICollection<WrappedGuidRecordDependentShadow> Dependents { get; } = new List<WrappedGuidRecordDependentShadow>();
        public ICollection<WrappedGuidRecordDependentOptional> OptionalDependents { get; } = new List<WrappedGuidRecordDependentOptional>();
        public ICollection<WrappedGuidRecordDependentRequired> RequiredDependents { get; } = new List<WrappedGuidRecordDependentRequired>();
    }

    protected class WrappedGuidRecordDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidRecord Id { get; set; } = null!;

        public WrappedGuidRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidRecordDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidRecord Id { get; set; } = null!;

        public WrappedGuidKeyRecord? PrincipalId { get; set; }
        public WrappedGuidRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedGuidRecordDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedGuidRecord Id { get; set; } = null!;

        public WrappedGuidKeyRecord PrincipalId { get; set; } = null!;
        public WrappedGuidRecordPrincipal Principal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_wrapped_Guid_key()
    {
        var id1 = Guid.Empty;
        var id2 = Guid.Empty;
        var id3 = Guid.Empty;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new WrappedGuidClassPrincipal
                    {
                        Id = Fixture.WrappedGuidKeyClassSentinel!,
                        NonKey = Fixture.WrappedGuidClassSentinel,
                        Dependents = { new WrappedGuidClassDependentShadow(), new WrappedGuidClassDependentShadow() },
                        OptionalDependents = { new WrappedGuidClassDependentOptional(), new WrappedGuidClassDependentOptional() },
                        RequiredDependents = { new WrappedGuidClassDependentRequired(), new WrappedGuidClassDependentRequired() }
                    }).Entity;

                var principal2 = context.Add(
                    new WrappedGuidStructPrincipal
                    {
                        Id = Fixture.WrappedGuidKeyStructSentinel,
                        NonKey = Fixture.WrappedGuidStructSentinel,
                        Dependents = { new WrappedGuidStructDependentShadow(), new WrappedGuidStructDependentShadow() },
                        OptionalDependents = { new WrappedGuidStructDependentOptional(), new WrappedGuidStructDependentOptional() },
                        RequiredDependents = { new WrappedGuidStructDependentRequired(), new WrappedGuidStructDependentRequired() }
                    }).Entity;

                var principal3 = context.Add(
                    new WrappedGuidRecordPrincipal
                    {
                        Id = Fixture.WrappedGuidKeyRecordSentinel!,
                        NonKey = Fixture.WrappedGuidRecordSentinel,
                        Dependents = { new WrappedGuidRecordDependentShadow(), new WrappedGuidRecordDependentShadow() },
                        OptionalDependents = { new WrappedGuidRecordDependentOptional(), new WrappedGuidRecordDependentOptional() },
                        RequiredDependents = { new WrappedGuidRecordDependentRequired(), new WrappedGuidRecordDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id.Value;
                Assert.NotEqual(Guid.Empty, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedGuidKeyClass?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId.Value);
                }

                Assert.Equal(KnownGuid, principal1.NonKey!.Value);

                id2 = principal2.Id.Value;
                Assert.NotEqual(Guid.Empty, id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedGuidKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
                }

                foreach (var dependent in principal2.OptionalDependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
                }

                foreach (var dependent in principal2.RequiredDependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId.Value);
                }

                Assert.Equal(KnownGuid, principal2.NonKey.Value);

                id3 = principal3.Id.Value;
                Assert.NotEqual(Guid.Empty, id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedGuidKeyRecord?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal3.OptionalDependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal3.RequiredDependents)
                {
                    Assert.NotEqual(Guid.Empty, dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId.Value);
                }

                Assert.Equal(KnownGuid, principal3.NonKey!.Value);
            },
            async context =>
            {
                var principal1 = await context.Set<WrappedGuidClassPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id.Value, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedGuidKeyClass?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId.Value);
                }

                var principal2 = await context.Set<WrappedGuidStructPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal2.Id.Value, id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedGuidKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
                }

                foreach (var dependent in principal2.OptionalDependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
                }

                foreach (var dependent in principal2.RequiredDependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId.Value);
                }

                var principal3 = await context.Set<WrappedGuidRecordPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal3.Id.Value, id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedGuidKeyRecord?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal3.OptionalDependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal3.RequiredDependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId.Value);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal2.Dependents.Remove(principal2.Dependents.First());
                principal3.Dependents.Remove(principal3.Dependents.First());

                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal2.OptionalDependents.Remove(principal2.OptionalDependents.First());
                principal3.OptionalDependents.Remove(principal3.OptionalDependents.First());

                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
                principal2.RequiredDependents.Remove(principal2.RequiredDependents.First());
                principal3.RequiredDependents.Remove(principal3.RequiredDependents.First());

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<WrappedGuidClassDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(
                    context.Entry(dependents1.Single(e => e.Principal == null))
                        .Property<WrappedGuidKeyClass?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<WrappedGuidClassDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<WrappedGuidClassDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                var dependents2 = await context.Set<WrappedGuidStructDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents2.Count);
                Assert.Null(
                    context.Entry(dependents2.Single(e => e.Principal == null))
                        .Property<WrappedGuidKeyStruct?>("PrincipalId").CurrentValue);

                var optionalDependents2 = await context.Set<WrappedGuidStructDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents2.Count);
                Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents2 = await context.Set<WrappedGuidStructDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents2);

                var dependents3 = await context.Set<WrappedGuidRecordDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents3.Count);
                Assert.Null(
                    context.Entry(dependents3.Single(e => e.Principal == null))
                        .Property<WrappedGuidKeyRecord?>("PrincipalId").CurrentValue);

                var optionalDependents3 = await context.Set<WrappedGuidRecordDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents3.Count);
                Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents3 = await context.Set<WrappedGuidRecordDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents3);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                context.Remove(dependents2.Single(e => e.Principal != null));
                context.Remove(optionalDependents2.Single(e => e.Principal != null));
                context.Remove(requiredDependents2.Single());
                context.Remove(requiredDependents2.Single().Principal);

                context.Remove(dependents3.Single(e => e.Principal != null));
                context.Remove(optionalDependents3.Single(e => e.Principal != null));
                context.Remove(requiredDependents3.Single());
                context.Remove(requiredDependents3.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<WrappedGuidClassDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedGuidStructDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedGuidRecordDependentShadow>().CountAsync());

                Assert.Equal(1, await context.Set<WrappedGuidClassDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedGuidStructDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedGuidRecordDependentOptional>().CountAsync());

                Assert.Equal(0, await context.Set<WrappedGuidClassDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedGuidStructDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedGuidRecordDependentRequired>().CountAsync());
            });
    }

    public class WrappedUriClass
    {
        public Uri? Value { get; set; }
    }

    protected class WrappedUriClassConverter : ValueConverter<WrappedUriClass, Uri>
    {
        public WrappedUriClassConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriClass { Value = v })
        {
        }
    }

    protected class WrappedUriClassComparer : ValueComparer<WrappedUriClass?>
    {
        public WrappedUriClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value!.Equals(v2.Value)),
                v => v != null ? v.Value!.GetHashCode() : 0,
                v => v == null ? null : new WrappedUriClass { Value = v.Value })
        {
        }
    }

    protected class WrappedUriClassValueGenerator : ValueGenerator<WrappedUriClass>
    {
        public override WrappedUriClass Next(EntityEntry entry)
            => new() { Value = new Uri("https://www.example.com") };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public struct WrappedUriStruct
    {
        public Uri Value { get; set; }
    }

    protected class WrappedUriStructConverter : ValueConverter<WrappedUriStruct, Uri>
    {
        public WrappedUriStructConverter()
            : base(
                v => v.Value,
                v => new WrappedUriStruct { Value = v })
        {
        }
    }

    protected class WrappedUriStructValueGenerator : ValueGenerator<WrappedUriStruct>
    {
        public override WrappedUriStruct Next(EntityEntry entry)
            => new() { Value = new Uri("https://www.example.com") };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public record WrappedUriRecord
    {
        public Uri? Value { get; set; }
    }

    protected class WrappedUriRecordConverter : ValueConverter<WrappedUriRecord, Uri>
    {
        public WrappedUriRecordConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriRecord { Value = v })
        {
        }
    }

    protected class WrappedUriRecordValueGenerator : ValueGenerator<WrappedUriRecord>
    {
        public override WrappedUriRecord Next(EntityEntry entry)
            => new() { Value = new Uri("https://www.example.com") };

        public override bool GeneratesTemporaryValues
            => false;
    }

    public class WrappedUriKeyClass
    {
        public Uri? Value { get; set; }
    }

    protected class WrappedUriKeyClassConverter : ValueConverter<WrappedUriKeyClass, Uri>
    {
        public WrappedUriKeyClassConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriKeyClass { Value = v })
        {
        }
    }

    protected class WrappedUriKeyClassComparer : ValueComparer<WrappedUriKeyClass?>
    {
        public WrappedUriKeyClassComparer()
            : base(
                (v1, v2) => (v1 == null && v2 == null) || (v1 != null && v2 != null && v1.Value!.Equals(v2.Value)),
                v => v != null ? v.Value!.GetHashCode() : 0,
                v => v == null ? null : new WrappedUriKeyClass { Value = v.Value })
        {
        }
    }

    public struct WrappedUriKeyStruct
    {
        public Uri? Value { get; set; }

        public bool Equals(WrappedUriKeyStruct other)
            => Equals(Value, other.Value);

        public override bool Equals(object? obj)
            => obj is WrappedUriKeyStruct other && Equals(other);

        public override int GetHashCode()
            => (Value != null ? Value.GetHashCode() : 0);

        public static bool operator ==(WrappedUriKeyStruct left, WrappedUriKeyStruct right)
            => left.Equals(right);

        public static bool operator !=(WrappedUriKeyStruct left, WrappedUriKeyStruct right)
            => !left.Equals(right);
    }

    protected class WrappedUriKeyStructConverter : ValueConverter<WrappedUriKeyStruct, Uri>
    {
        public WrappedUriKeyStructConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriKeyStruct { Value = v })
        {
        }
    }

    public record WrappedUriKeyRecord
    {
        public Uri? Value { get; set; }
    }

    protected class WrappedUriKeyRecordConverter : ValueConverter<WrappedUriKeyRecord, Uri>
    {
        public WrappedUriKeyRecordConverter()
            : base(
                v => v.Value!,
                v => new WrappedUriKeyRecord { Value = v })
        {
        }
    }

    protected class WrappedUriClassPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriKeyClass Id { get; set; } = null!;

        public WrappedUriClass? NonKey { get; set; }
        public ICollection<WrappedUriClassDependentShadow> Dependents { get; } = new List<WrappedUriClassDependentShadow>();
        public ICollection<WrappedUriClassDependentRequired> RequiredDependents { get; } = new List<WrappedUriClassDependentRequired>();
        public ICollection<WrappedUriClassDependentOptional> OptionalDependents { get; } = new List<WrappedUriClassDependentOptional>();
    }

    protected class WrappedUriClassDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriClass Id { get; set; } = null!;

        public WrappedUriClassPrincipal? Principal { get; set; }
    }

    protected class WrappedUriClassDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriClass Id { get; set; } = null!;

        public WrappedUriKeyClass PrincipalId { get; set; } = null!;
        public WrappedUriClassPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedUriClassDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriClass Id { get; set; } = null!;

        public WrappedUriKeyClass? PrincipalId { get; set; }
        public WrappedUriClassPrincipal? Principal { get; set; }
    }

    protected class WrappedUriStructPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriKeyStruct Id { get; set; }

        public WrappedUriStruct NonKey { get; set; }
        public ICollection<WrappedUriStructDependentShadow> Dependents { get; } = new List<WrappedUriStructDependentShadow>();
        public ICollection<WrappedUriStructDependentOptional> OptionalDependents { get; } = new List<WrappedUriStructDependentOptional>();
        public ICollection<WrappedUriStructDependentRequired> RequiredDependents { get; } = new List<WrappedUriStructDependentRequired>();
    }

    protected class WrappedUriStructDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriStruct Id { get; set; }

        public WrappedUriStructPrincipal? Principal { get; set; }
    }

    protected class WrappedUriStructDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriStruct Id { get; set; }

        public WrappedUriKeyStruct? PrincipalId { get; set; }
        public WrappedUriStructPrincipal? Principal { get; set; }
    }

    protected class WrappedUriStructDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriStruct Id { get; set; }

        public WrappedUriKeyStruct PrincipalId { get; set; }
        public WrappedUriStructPrincipal Principal { get; set; } = null!;
    }

    protected class WrappedUriRecordPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriKeyRecord Id { get; set; } = null!;

        public WrappedUriRecord? NonKey { get; set; }
        public ICollection<WrappedUriRecordDependentShadow> Dependents { get; } = new List<WrappedUriRecordDependentShadow>();
        public ICollection<WrappedUriRecordDependentOptional> OptionalDependents { get; } = new List<WrappedUriRecordDependentOptional>();
        public ICollection<WrappedUriRecordDependentRequired> RequiredDependents { get; } = new List<WrappedUriRecordDependentRequired>();
    }

    protected class WrappedUriRecordDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriRecord Id { get; set; } = null!;

        public WrappedUriRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedUriRecordDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriRecord Id { get; set; } = null!;

        public WrappedUriKeyRecord? PrincipalId { get; set; }
        public WrappedUriRecordPrincipal? Principal { get; set; }
    }

    protected class WrappedUriRecordDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public WrappedUriRecord Id { get; set; } = null!;

        public WrappedUriKeyRecord PrincipalId { get; set; } = null!;
        public WrappedUriRecordPrincipal Principal { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_wrapped_Uri_key()
    {
        Uri? id1 = null;
        Uri? id2 = null;
        Uri? id3 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new WrappedUriClassPrincipal
                    {
                        Id = Fixture.WrappedUriKeyClassSentinel!,
                        NonKey = Fixture.WrappedUriClassSentinel,
                        Dependents = { new WrappedUriClassDependentShadow(), new WrappedUriClassDependentShadow() },
                        OptionalDependents = { new WrappedUriClassDependentOptional(), new WrappedUriClassDependentOptional() },
                        RequiredDependents = { new WrappedUriClassDependentRequired(), new WrappedUriClassDependentRequired() }
                    }).Entity;

                var principal2 = context.Add(
                    new WrappedUriStructPrincipal
                    {
                        Id = Fixture.WrappedUriKeyStructSentinel,
                        NonKey = Fixture.WrappedUriStructSentinel,
                        Dependents = { new WrappedUriStructDependentShadow(), new WrappedUriStructDependentShadow() },
                        OptionalDependents = { new WrappedUriStructDependentOptional(), new WrappedUriStructDependentOptional() },
                        RequiredDependents = { new WrappedUriStructDependentRequired(), new WrappedUriStructDependentRequired() }
                    }).Entity;

                var principal3 = context.Add(
                    new WrappedUriRecordPrincipal
                    {
                        Id = Fixture.WrappedUriKeyRecordSentinel!,
                        NonKey = Fixture.WrappedUriRecordSentinel,
                        Dependents = { new WrappedUriRecordDependentShadow(), new WrappedUriRecordDependentShadow() },
                        OptionalDependents = { new WrappedUriRecordDependentOptional(), new WrappedUriRecordDependentOptional() },
                        RequiredDependents = { new WrappedUriRecordDependentRequired(), new WrappedUriRecordDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id.Value;
                Assert.NotNull(id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedUriKeyClass?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId.Value);
                }

                Assert.Equal(new Uri("https://www.example.com"), principal1.NonKey!.Value);

                id2 = principal2.Id.Value;
                Assert.NotNull(id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedUriKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
                }

                foreach (var dependent in principal2.OptionalDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
                }

                foreach (var dependent in principal2.RequiredDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId.Value);
                }

                Assert.Equal(new Uri("https://www.example.com"), principal2.NonKey.Value);

                id3 = principal3.Id.Value;
                Assert.NotNull(id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedUriKeyRecord?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal3.OptionalDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal3.RequiredDependents)
                {
                    Assert.NotNull(dependent.Id.Value);
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId.Value);
                }

                Assert.Equal(new Uri("https://www.example.com"), principal3.NonKey!.Value);
            },
            async context =>
            {
                var principal1 = await context.Set<WrappedUriClassPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id.Value, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<WrappedUriKeyClass?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal1.OptionalDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal1.RequiredDependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, dependent.PrincipalId.Value);
                }

                var principal2 = await context.Set<WrappedUriStructPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal2.Id.Value, id2);
                foreach (var dependent in principal2.Dependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, context.Entry(dependent).Property<WrappedUriKeyStruct?>("PrincipalId").CurrentValue!.Value.Value);
                }

                foreach (var dependent in principal2.OptionalDependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId!.Value.Value);
                }

                foreach (var dependent in principal2.RequiredDependents)
                {
                    Assert.Same(principal2, dependent.Principal);
                    Assert.Equal(id2, dependent.PrincipalId.Value);
                }

                var principal3 = await context.Set<WrappedUriRecordPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal3.Id.Value, id3);
                foreach (var dependent in principal3.Dependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, context.Entry(dependent).Property<WrappedUriKeyRecord?>("PrincipalId").CurrentValue!.Value);
                }

                foreach (var dependent in principal3.OptionalDependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId!.Value);
                }

                foreach (var dependent in principal3.RequiredDependents)
                {
                    Assert.Same(principal3, dependent.Principal);
                    Assert.Equal(id3, dependent.PrincipalId.Value);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal2.Dependents.Remove(principal2.Dependents.First());
                principal3.Dependents.Remove(principal3.Dependents.First());

                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal2.OptionalDependents.Remove(principal2.OptionalDependents.First());
                principal3.OptionalDependents.Remove(principal3.OptionalDependents.First());

                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
                principal2.RequiredDependents.Remove(principal2.RequiredDependents.First());
                principal3.RequiredDependents.Remove(principal3.RequiredDependents.First());

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<WrappedUriClassDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(
                    context.Entry(dependents1.Single(e => e.Principal == null))
                        .Property<WrappedUriKeyClass?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<WrappedUriClassDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<WrappedUriClassDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                var dependents2 = await context.Set<WrappedUriStructDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents2.Count);
                Assert.Null(
                    context.Entry(dependents2.Single(e => e.Principal == null))
                        .Property<WrappedUriKeyStruct?>("PrincipalId").CurrentValue);

                var optionalDependents2 = await context.Set<WrappedUriStructDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents2.Count);
                Assert.Null(optionalDependents2.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents2 = await context.Set<WrappedUriStructDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents2);

                var dependents3 = await context.Set<WrappedUriRecordDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents3.Count);
                Assert.Null(
                    context.Entry(dependents3.Single(e => e.Principal == null))
                        .Property<WrappedUriKeyRecord?>("PrincipalId").CurrentValue);

                var optionalDependents3 = await context.Set<WrappedUriRecordDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents3.Count);
                Assert.Null(optionalDependents3.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents3 = await context.Set<WrappedUriRecordDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents3);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                context.Remove(dependents2.Single(e => e.Principal != null));
                context.Remove(optionalDependents2.Single(e => e.Principal != null));
                context.Remove(requiredDependents2.Single());
                context.Remove(requiredDependents2.Single().Principal);

                context.Remove(dependents3.Single(e => e.Principal != null));
                context.Remove(optionalDependents3.Single(e => e.Principal != null));
                context.Remove(requiredDependents3.Single());
                context.Remove(requiredDependents3.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<WrappedUriClassDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedUriStructDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedUriRecordDependentShadow>().CountAsync());

                Assert.Equal(1, await context.Set<WrappedUriClassDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedUriStructDependentOptional>().CountAsync());
                Assert.Equal(1, await context.Set<WrappedUriRecordDependentOptional>().CountAsync());

                Assert.Equal(0, await context.Set<WrappedUriClassDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedUriStructDependentRequired>().CountAsync());
                Assert.Equal(0, await context.Set<WrappedUriRecordDependentRequired>().CountAsync());
            });
    }

    protected class UriPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Uri Id { get; set; } = null!;

        public ICollection<UriDependentShadow> Dependents { get; } = new List<UriDependentShadow>();
        public ICollection<UriDependentRequired> RequiredDependents { get; } = new List<UriDependentRequired>();
        public ICollection<UriDependentOptional> OptionalDependents { get; } = new List<UriDependentOptional>();
    }

    protected class UriDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Uri Id { get; set; } = null!;

        public UriPrincipal? Principal { get; set; }
    }

    protected class UriDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Uri Id { get; set; } = null!;

        public Uri PrincipalId { get; set; } = null!;
        public UriPrincipal Principal { get; set; } = null!;
    }

    protected class UriDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Uri Id { get; set; } = null!;

        public Uri? PrincipalId { get; set; }
        public UriPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_Uri_key()
    {
        Uri? id1 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new UriPrincipal
                    {
                        Id = Fixture.UriSentinel!,
                        Dependents = { new UriDependentShadow(), new UriDependentShadow() },
                        OptionalDependents = { new UriDependentOptional(), new UriDependentOptional() },
                        RequiredDependents = { new UriDependentRequired(), new UriDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id;
                Assert.NotNull(id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.NotNull(dependent.Id);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<Uri?>("PrincipalId").CurrentValue);
                }
            },
            async context =>
            {
                var principal1 = await context.Set<UriPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<Uri?>("PrincipalId").CurrentValue);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<UriDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(context.Entry(dependents1.Single(e => e.Principal == null)).Property<Uri?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<UriDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<UriDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<UriDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<UriDependentOptional>().CountAsync());
                Assert.Equal(0, await context.Set<UriDependentRequired>().CountAsync());
            });
    }

    public enum KeyEnum
    {
        A,
        B,
        C,
        D,
        E
    }

    protected class EnumPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public KeyEnum Id { get; set; }

        public ICollection<EnumDependentShadow> Dependents { get; } = new List<EnumDependentShadow>();
        public ICollection<EnumDependentRequired> RequiredDependents { get; } = new List<EnumDependentRequired>();
        public ICollection<EnumDependentOptional> OptionalDependents { get; } = new List<EnumDependentOptional>();
    }

    protected class EnumDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public KeyEnum Id { get; set; }

        public EnumPrincipal? Principal { get; set; }
    }

    protected class EnumDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public KeyEnum Id { get; set; }

        public KeyEnum PrincipalId { get; set; }
        public EnumPrincipal Principal { get; set; } = null!;
    }

    protected class EnumDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public KeyEnum Id { get; set; }

        public KeyEnum? PrincipalId { get; set; }
        public EnumPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_enum_key()
    {
        KeyEnum? id1 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new EnumPrincipal
                    {
                        Id = Fixture.KeyEnumSentinel,
                        Dependents = { new EnumDependentShadow(), new EnumDependentShadow() },
                        OptionalDependents = { new EnumDependentOptional(), new EnumDependentOptional() },
                        RequiredDependents = { new EnumDependentRequired(), new EnumDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id;
                Assert.NotNull(id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<KeyEnum?>("PrincipalId").CurrentValue);
                }
            },
            async context =>
            {
                var principal1 = await context.Set<EnumPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<KeyEnum?>("PrincipalId").CurrentValue);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<EnumDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(context.Entry(dependents1.Single(e => e.Principal == null)).Property<KeyEnum?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<EnumDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<EnumDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<EnumDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<EnumDependentOptional>().CountAsync());
                Assert.Equal(0, await context.Set<EnumDependentRequired>().CountAsync());
            });
    }

    protected class GuidAsStringPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public ICollection<GuidAsStringDependentShadow> Dependents { get; } = new List<GuidAsStringDependentShadow>();
        public ICollection<GuidAsStringDependentRequired> RequiredDependents { get; } = new List<GuidAsStringDependentRequired>();
        public ICollection<GuidAsStringDependentOptional> OptionalDependents { get; } = new List<GuidAsStringDependentOptional>();
    }

    protected class GuidAsStringDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public GuidAsStringPrincipal? Principal { get; set; }
    }

    protected class GuidAsStringDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid PrincipalId { get; set; }
        public GuidAsStringPrincipal Principal { get; set; } = null!;
    }

    protected class GuidAsStringDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid? PrincipalId { get; set; }
        public GuidAsStringPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_GuidAsString_key()
    {
        Guid? id1 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new GuidAsStringPrincipal
                    {
                        Id = Fixture.GuidAsStringSentinel,
                        Dependents = { new GuidAsStringDependentShadow(), new GuidAsStringDependentShadow() },
                        OptionalDependents = { new GuidAsStringDependentOptional(), new GuidAsStringDependentOptional() },
                        RequiredDependents = { new GuidAsStringDependentRequired(), new GuidAsStringDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id;
                Assert.NotNull(id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<Guid?>("PrincipalId").CurrentValue);
                }
            },
            async context =>
            {
                var principal1 = await context.Set<GuidAsStringPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<Guid?>("PrincipalId").CurrentValue);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<GuidAsStringDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(context.Entry(dependents1.Single(e => e.Principal == null)).Property<Guid?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<GuidAsStringDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<GuidAsStringDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<GuidAsStringDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<GuidAsStringDependentOptional>().CountAsync());
                Assert.Equal(0, await context.Set<GuidAsStringDependentRequired>().CountAsync());
            });
    }

    protected class StringAsGuidPrincipal
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public ICollection<StringAsGuidDependentShadow> Dependents { get; } = new List<StringAsGuidDependentShadow>();
        public ICollection<StringAsGuidDependentRequired> RequiredDependents { get; } = new List<StringAsGuidDependentRequired>();
        public ICollection<StringAsGuidDependentOptional> OptionalDependents { get; } = new List<StringAsGuidDependentOptional>();
    }

    protected class StringAsGuidDependentShadow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public StringAsGuidPrincipal? Principal { get; set; }
    }

    protected class StringAsGuidDependentRequired
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public string PrincipalId { get; set; } = null!;
        public StringAsGuidPrincipal Principal { get; set; } = null!;
    }

    protected class StringAsGuidDependentOptional
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; } = null!;

        public string? PrincipalId { get; set; }
        public StringAsGuidPrincipal? Principal { get; set; }
    }

    [ConditionalFact]
    public virtual Task Insert_update_and_delete_with_StringAsGuid_key()
    {
        string? id1 = null;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal1 = context.Add(
                    new StringAsGuidPrincipal
                    {
                        Id = Fixture.StringAsGuidSentinel!,
                        Dependents = { new StringAsGuidDependentShadow(), new StringAsGuidDependentShadow() },
                        OptionalDependents = { new StringAsGuidDependentOptional(), new StringAsGuidDependentOptional() },
                        RequiredDependents = { new StringAsGuidDependentRequired(), new StringAsGuidDependentRequired() }
                    }).Entity;

                await context.SaveChangesAsync();

                id1 = principal1.Id;
                Assert.NotNull(id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.NotNull(dependent.Id);
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<string?>("PrincipalId").CurrentValue);
                }
            },
            async context =>
            {
                var principal1 = await context.Set<StringAsGuidPrincipal>()
                    .Include(e => e.Dependents)
                    .Include(e => e.OptionalDependents)
                    .Include(e => e.RequiredDependents)
                    .SingleAsync();

                Assert.Equal(principal1.Id, id1);
                foreach (var dependent in principal1.Dependents)
                {
                    Assert.Same(principal1, dependent.Principal);
                    Assert.Equal(id1, context.Entry(dependent).Property<string?>("PrincipalId").CurrentValue);
                }

                principal1.Dependents.Remove(principal1.Dependents.First());
                principal1.OptionalDependents.Remove(principal1.OptionalDependents.First());
                principal1.RequiredDependents.Remove(principal1.RequiredDependents.First());
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var dependents1 = await context.Set<StringAsGuidDependentShadow>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, dependents1.Count);
                Assert.Null(context.Entry(dependents1.Single(e => e.Principal == null)).Property<string?>("PrincipalId").CurrentValue);

                var optionalDependents1 = await context.Set<StringAsGuidDependentOptional>().Include(e => e.Principal).ToListAsync();
                Assert.Equal(2, optionalDependents1.Count);
                Assert.Null(optionalDependents1.Single(e => e.Principal == null).PrincipalId);

                var requiredDependents1 = await context.Set<StringAsGuidDependentRequired>().Include(e => e.Principal).ToListAsync();
                Assert.Single(requiredDependents1);

                context.Remove(dependents1.Single(e => e.Principal != null));
                context.Remove(optionalDependents1.Single(e => e.Principal != null));
                context.Remove(requiredDependents1.Single());
                context.Remove(requiredDependents1.Single().Principal);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                Assert.Equal(1, await context.Set<StringAsGuidDependentShadow>().CountAsync());
                Assert.Equal(1, await context.Set<StringAsGuidDependentOptional>().CountAsync());
                Assert.Equal(0, await context.Set<StringAsGuidDependentRequired>().CountAsync());
            });
    }

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<DbContext, Task> testOperation,
        Func<DbContext, Task>? nestedTestOperation1 = null,
        Func<DbContext, Task>? nestedTestOperation2 = null,
        Func<DbContext, Task>? nestedTestOperation3 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected DbContext CreateContext()
        => Fixture.CreateContext();

    public abstract class StoreGeneratedFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        public virtual Guid GuidSentinel
            => default;

        public virtual int IntSentinel
            => default;

        public virtual short ShortSentinel
            => default;

        public virtual long LongSentinel
            => default;

        public virtual int? NullableIntSentinel
            => default;

        public virtual bool BoolSentinel
            => default;

        public virtual bool? NullableBoolSentinel
            => default;

        public virtual string? StringSentinel
            => default;

        public virtual Uri? UriSentinel
            => default;

        public virtual KeyEnum KeyEnumSentinel
            => default;

        public virtual string? StringAsGuidSentinel
            => default;

        public virtual Guid GuidAsStringSentinel
            => default;

        public virtual WrappedIntKeyClass? WrappedIntKeyClassSentinel
            => default;

        public virtual WrappedIntClass? WrappedIntClassSentinel
            => default;

        public virtual WrappedIntKeyStruct WrappedIntKeyStructSentinel
            => default;

        public virtual WrappedIntStruct WrappedIntStructSentinel
            => default;

        public virtual WrappedIntKeyRecord? WrappedIntKeyRecordSentinel
            => default;

        public virtual WrappedIntRecord? WrappedIntRecordSentinel
            => default;

        public virtual WrappedStringKeyClass? WrappedStringKeyClassSentinel
            => default;

        public virtual WrappedStringClass? WrappedStringClassSentinel
            => default;

        public virtual WrappedStringKeyStruct WrappedStringKeyStructSentinel
            => default;

        public virtual WrappedStringStruct WrappedStringStructSentinel
            => default;

        public virtual WrappedStringKeyRecord? WrappedStringKeyRecordSentinel
            => default;

        public virtual WrappedStringRecord? WrappedStringRecordSentinel
            => default;

        public virtual WrappedGuidKeyClass? WrappedGuidKeyClassSentinel
            => default;

        public virtual WrappedGuidClass? WrappedGuidClassSentinel
            => default;

        public virtual WrappedGuidKeyStruct WrappedGuidKeyStructSentinel
            => default;

        public virtual WrappedGuidStruct WrappedGuidStructSentinel
            => default;

        public virtual WrappedGuidKeyRecord? WrappedGuidKeyRecordSentinel
            => default;

        public virtual WrappedGuidRecord? WrappedGuidRecordSentinel
            => default;

        public virtual WrappedUriKeyClass? WrappedUriKeyClassSentinel
            => default;

        public virtual WrappedUriClass? WrappedUriClassSentinel
            => default;

        public virtual WrappedUriKeyStruct WrappedUriKeyStructSentinel
            => default;

        public virtual WrappedUriStruct WrappedUriStructSentinel
            => default;

        public virtual WrappedUriKeyRecord? WrappedUriKeyRecordSentinel
            => default;

        public virtual WrappedUriRecord? WrappedUriRecordSentinel
            => default;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<IntToString>().Property(e => e.Id).HasConversion<string>();
            modelBuilder.Entity<GuidToString>().Property(e => e.Id).HasConversion<string>();
            modelBuilder.Entity<GuidToBytes>().Property(e => e.Id).HasConversion<byte[]>();
            modelBuilder.Entity<ShortToBytes>().Property(e => e.Id).HasConversion<byte[]>();

            modelBuilder.Entity<Gumball>(
                b =>
                {
                    var property = b.Property(e => e.Id).ValueGeneratedOnAdd().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.Identity).ValueGeneratedOnAdd().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.IdentityReadOnlyBeforeSave).ValueGeneratedOnAdd().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.IdentityReadOnlyAfterSave).ValueGeneratedOnAdd().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.AlwaysIdentity).ValueGeneratedOnAdd().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.AlwaysIdentityReadOnlyBeforeSave).ValueGeneratedOnAdd().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.AlwaysIdentityReadOnlyAfterSave).ValueGeneratedOnAdd().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.Computed).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.ComputedReadOnlyBeforeSave).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.ComputedReadOnlyAfterSave).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.AlwaysComputed).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.AlwaysComputedReadOnlyBeforeSave).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.AlwaysComputedReadOnlyAfterSave).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                });

            modelBuilder.Entity<Anais>(
                b =>
                {
                    b.Property(e => e.Never).ValueGeneratedNever();

                    var property = b.Property(e => e.NeverUseBeforeUseAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.NeverIgnoreBeforeUseAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.NeverThrowBeforeUseAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.NeverUseBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.NeverIgnoreBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.NeverThrowBeforeIgnoreAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.NeverUseBeforeThrowAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.NeverIgnoreBeforeThrowAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.NeverThrowBeforeThrowAfter).ValueGeneratedNever().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    b.Property(e => e.OnAdd).ValueGeneratedOnAdd();

                    property = b.Property(e => e.OnAddUseBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnAddIgnoreBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnAddThrowBeforeUseAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnAddUseBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnAddIgnoreBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnAddThrowBeforeIgnoreAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnAddUseBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.OnAddIgnoreBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.OnAddThrowBeforeThrowAfter).ValueGeneratedOnAdd().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    b.Property(e => e.OnAddOrUpdate).ValueGeneratedOnAddOrUpdate();

                    property = b.Property(e => e.OnAddOrUpdateUseBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnAddOrUpdateThrowBeforeUseAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnAddOrUpdateUseBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnAddOrUpdateThrowBeforeIgnoreAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnAddOrUpdateUseBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.OnAddOrUpdateIgnoreBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.OnAddOrUpdateThrowBeforeThrowAfter).ValueGeneratedOnAddOrUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    b.Property(e => e.OnUpdate).ValueGeneratedOnUpdate();

                    property = b.Property(e => e.OnUpdateUseBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnUpdateIgnoreBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnUpdateThrowBeforeUseAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Save);

                    property = b.Property(e => e.OnUpdateUseBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnUpdateIgnoreBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnUpdateThrowBeforeIgnoreAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                    property = b.Property(e => e.OnUpdateUseBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Save);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.OnUpdateIgnoreBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);

                    property = b.Property(e => e.OnUpdateThrowBeforeThrowAfter).ValueGeneratedOnUpdate().Metadata;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                });

            modelBuilder.Entity<Darwin>(
                b =>
                {
                    b.HasOne(e => e.Species).WithOne().HasForeignKey<Species>(e => e.DarwinId);
                    b.HasMany(e => e.MixedMetaphors).WithOne().HasForeignKey(e => e.MetaphoricId);
                });

            modelBuilder.Entity<WithBackingFields>(
                b =>
                {
                    b.Property(e => e.Id).HasField("_id");
                    b.Property(e => e.NullableAsNonNullable).HasField("_nullableAsNonNullable").ValueGeneratedOnAddOrUpdate();
                    b.Property(e => e.NonNullableAsNullable)
                        .HasField("_nonNullableAsNullable")
                        .ValueGeneratedOnAddOrUpdate()
                        .UsePropertyAccessMode(PropertyAccessMode.Property);
                });

            modelBuilder.Entity<OptionalProduct>();
            modelBuilder.Entity<StoreGenPrincipal>();

            modelBuilder.Entity<NonStoreGenDependent>(
                eb =>
                {
                    eb.Property(e => e.HasTemp)
                        .ValueGeneratedOnAddOrUpdate()
                        .HasValueGenerator<TemporaryIntValueGenerator>();
                });

            modelBuilder.Entity<CompositePrincipal>(
                entity =>
                {
                    entity.HasKey(x => x.Id);
                    entity.Property(x => x.Id)
                        .ValueGeneratedOnAdd();
                    entity.HasOne(x => x.Current)
                        .WithOne()
                        .HasForeignKey<CompositePrincipal>(x => new { x.Id, x.CurrentNumber });
                });

            modelBuilder.Entity<CompositeDependent>(
                entity =>
                {
                    entity.HasKey(x => new { x.PrincipalId, x.Number });
                    entity.HasOne(x => x.Principal)
                        .WithMany(x => x.Periods)
                        .HasForeignKey(x => x.PrincipalId);
                });

            modelBuilder.Entity<WrappedIntClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedIntClassValueGenerator>();
                });
            modelBuilder.Entity<WrappedIntStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedIntStructValueGenerator>();
                });
            modelBuilder.Entity<WrappedIntRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedIntRecordValueGenerator>();
                });

            modelBuilder.Entity<LongToIntPrincipal>(
                entity =>
                {
                    var keyConverter = new ValueConverter<long, int>(
                        v => (int)v,
                        v => v);

                    entity.Property(e => e.Id).HasConversion(keyConverter);
                });

            modelBuilder.Entity<WrappedGuidClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedGuidClassValueGenerator>();
                });
            modelBuilder.Entity<WrappedGuidStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedGuidStructValueGenerator>();
                });
            modelBuilder.Entity<WrappedGuidRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedGuidRecordValueGenerator>();
                });

            modelBuilder.Entity<WrappedStringClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedStringClassValueGenerator>();
                });
            modelBuilder.Entity<WrappedStringStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedStringStructValueGenerator>();
                });
            modelBuilder.Entity<WrappedStringRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedStringRecordValueGenerator>();
                });

            modelBuilder.Entity<WrappedUriClassPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedUriClassValueGenerator>();
                });
            modelBuilder.Entity<WrappedUriStructPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedUriStructValueGenerator>();
                });
            modelBuilder.Entity<WrappedUriRecordPrincipal>(
                entity =>
                {
                    entity.Property(e => e.NonKey).HasValueGenerator<WrappedUriRecordValueGenerator>();
                });

            modelBuilder.Entity<UriPrincipal>();
            modelBuilder.Entity<EnumPrincipal>();

            modelBuilder.Entity<GuidAsStringPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion<string>();
                });
            modelBuilder.Entity<GuidAsStringDependentShadow>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion<string>();
                });
            modelBuilder.Entity<GuidAsStringDependentOptional>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion<string>();
                    entity.Property(e => e.PrincipalId).HasConversion<string?>();
                });
            modelBuilder.Entity<GuidAsStringDependentRequired>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion<string>();
                    entity.Property(e => e.PrincipalId).HasConversion<string>();
                });

            var stringToGuidConverter = new ValueConverter<string, Guid>(
                v => new Guid(v),
                v => v.ToString());

            modelBuilder.Entity<StringAsGuidPrincipal>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion(stringToGuidConverter);
                });
            modelBuilder.Entity<StringAsGuidDependentShadow>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion(stringToGuidConverter);
                });
            modelBuilder.Entity<StringAsGuidDependentOptional>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion(stringToGuidConverter);
                    entity.Property(e => e.PrincipalId).HasConversion(stringToGuidConverter!);
                });
            modelBuilder.Entity<StringAsGuidDependentRequired>(
                entity =>
                {
                    entity.Property(e => e.Id).HasConversion(stringToGuidConverter);
                    entity.Property(e => e.PrincipalId).HasConversion(stringToGuidConverter);
                });
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<WrappedIntClass>().HaveConversion<WrappedIntClassConverter, WrappedIntClassComparer>();
            configurationBuilder.Properties<WrappedIntKeyClass>().HaveConversion<WrappedIntKeyClassConverter, WrappedIntKeyClassComparer>();
            configurationBuilder.Properties<WrappedIntStruct>().HaveConversion<WrappedIntStructConverter>();
            configurationBuilder.Properties<WrappedIntKeyStruct>().HaveConversion<WrappedIntKeyStructConverter>();
            configurationBuilder.Properties<WrappedIntRecord>().HaveConversion<WrappedIntRecordConverter>();
            configurationBuilder.Properties<WrappedIntKeyRecord>().HaveConversion<WrappedIntKeyRecordConverter>();

            configurationBuilder.Properties<WrappedGuidClass>().HaveConversion<WrappedGuidClassConverter, WrappedGuidClassComparer>();
            configurationBuilder.Properties<WrappedGuidKeyClass>()
                .HaveConversion<WrappedGuidKeyClassConverter, WrappedGuidKeyClassComparer>();
            configurationBuilder.Properties<WrappedGuidStruct>().HaveConversion<WrappedGuidStructConverter>();
            configurationBuilder.Properties<WrappedGuidKeyStruct>().HaveConversion<WrappedGuidKeyStructConverter>();
            configurationBuilder.Properties<WrappedGuidRecord>().HaveConversion<WrappedGuidRecordConverter>();
            configurationBuilder.Properties<WrappedGuidKeyRecord>().HaveConversion<WrappedGuidKeyRecordConverter>();

            configurationBuilder.Properties<WrappedStringClass>().HaveConversion<WrappedStringClassConverter, WrappedStringClassComparer>();
            configurationBuilder.Properties<WrappedStringKeyClass>()
                .HaveConversion<WrappedStringKeyClassConverter, WrappedStringKeyClassComparer>();
            configurationBuilder.Properties<WrappedStringStruct>().HaveConversion<WrappedStringStructConverter>();
            configurationBuilder.Properties<WrappedStringKeyStruct>().HaveConversion<WrappedStringKeyStructConverter>();
            configurationBuilder.Properties<WrappedStringRecord>().HaveConversion<WrappedStringRecordConverter>();
            configurationBuilder.Properties<WrappedStringKeyRecord>().HaveConversion<WrappedStringKeyRecordConverter>();

            configurationBuilder.Properties<WrappedUriClass>().HaveConversion<WrappedUriClassConverter, WrappedUriClassComparer>();
            configurationBuilder.Properties<WrappedUriKeyClass>()
                .HaveConversion<WrappedUriKeyClassConverter, WrappedUriKeyClassComparer>();
            configurationBuilder.Properties<WrappedUriStruct>().HaveConversion<WrappedUriStructConverter>();
            configurationBuilder.Properties<WrappedUriKeyStruct>().HaveConversion<WrappedUriKeyStructConverter>();
            configurationBuilder.Properties<WrappedUriRecord>().HaveConversion<WrappedUriRecordConverter>();
            configurationBuilder.Properties<WrappedUriKeyRecord>().HaveConversion<WrappedUriKeyRecordConverter>();
        }
    }
}
