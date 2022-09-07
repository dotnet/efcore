// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.StoredProcedureUpdateModel;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable enable

public class StoredProcedureUpdateTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : StoredProcedureUpdateFixtureBase
{
    protected StoredProcedureUpdateTestBase(TFixture fixture)
    {
        Fixture = fixture;

        fixture.CleanData();

        ClearLog();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Insert_with_output_parameter(bool async)
    {
        await using var context = CreateContext();

        var newEntity1 = new Entity { Name = "New" };
        context.WithOutputParameter.Add(newEntity1);
        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("New", context.WithOutputParameter.Single(b => b.Id == newEntity1.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Insert_twice_with_output_parameter(bool async)
    {
        await using var context = CreateContext();

        var (newEntity1, newEntity2) = (new Entity { Name = "New1" }, new Entity { Name = "New2" });

        context.WithOutputParameter.AddRange(newEntity1, newEntity2);
        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("New1", context.WithOutputParameter.Single(b => b.Id == newEntity1.Id).Name);
            Assert.Equal("New2", context.WithOutputParameter.Single(b => b.Id == newEntity2.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Insert_with_result_column(bool async)
    {
        await using var context = CreateContext();

        var entity = new Entity { Name = "Foo" };
        context.WithResultColumn.Add(entity);
        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Foo", context.WithResultColumn.Single(b => b.Id == entity.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Insert_with_two_result_columns(bool async)
    {
        await using var context = CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Foo" };
        context.WithTwoResultColumns.Add(entity);
        await SaveChanges(context, async);

        Assert.Equal(1, entity.Id);
        Assert.Equal(8, entity.AdditionalProperty);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Foo", context.WithTwoResultColumns.Single(b => b.Id == entity.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Insert_with_output_parameter_and_result_column(bool async)
    {
        await using var context = CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Foo" };
        context.WithOutputParameterAndResultColumn.Add(entity);
        await SaveChanges(context, async);

        Assert.Equal(8, entity.AdditionalProperty);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Foo", context.WithOutputParameterAndResultColumn.Single(b => b.Id == entity.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Update(bool async)
    {
        await using var context = CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.WithOutputParameter.Add(entity);
        await SaveChanges(context, async);

        ClearLog();

        entity.Name = "Updated";
        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Updated", (await context.WithOutputParameter.SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_partial(bool async)
    {
        await using var context = CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Foo", AdditionalProperty = 8 };
        context.WithTwoOutputParameters.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "Updated";

        ClearLog();

        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var actual = await context.WithTwoOutputParameters.SingleAsync(w => w.Id == entity.Id);

            Assert.Equal("Updated", actual.Name);
            Assert.Equal(8, actual.AdditionalProperty);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_with_output_parameter_and_rows_affected_result_column(bool async)
    {
        await using var context = CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Foo" };
        context.WithOutputParameterAndRowsAffectedResultColumn.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "Updated";

        ClearLog();

        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var actual = await context.WithOutputParameterAndRowsAffectedResultColumn.SingleAsync(w => w.Id == entity.Id);

            Assert.Equal("Updated", actual.Name);
            Assert.Equal(8, actual.AdditionalProperty);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(bool async)
    {
        await using var context1 = CreateContext();

        var entity1 = new EntityWithAdditionalProperty { Name = "Initial" };
        context1.WithOutputParameterAndRowsAffectedResultColumn.Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = CreateContext())
        {
            var entity2 = await context2.WithOutputParameterAndRowsAffectedResultColumn.SingleAsync(w => w.Name == "Initial");
            context2.WithOutputParameterAndRowsAffectedResultColumn.Remove(entity2);
            await context2.SaveChangesAsync();
        }

        ClearLog();

        entity1.Name = "Updated";

        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await SaveChanges(context1, async));
        var entry = exception.Entries.Single();
        Assert.Same(entity1, entry.Entity);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete(bool async)
    {
        await using var context = CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.WithOutputParameter.Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        context.WithOutputParameter.Remove(entity);
        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(0, await context.WithOutputParameter.CountAsync(b => b.Name == "Initial"));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_and_insert(bool async)
    {
        await using var context = CreateContext();

        var entity1 = new Entity { Name = "Entity1" };
        context.WithOutputParameter.Add(entity1);
        await context.SaveChangesAsync();

        ClearLog();

        context.WithOutputParameter.Remove(entity1);
        context.WithOutputParameter.Add(new Entity { Name = "Entity2" });
        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(0, await context.WithOutputParameter.CountAsync(b => b.Name == "Entity1"));
            Assert.Equal(1, await context.WithOutputParameter.CountAsync(b => b.Name == "Entity2"));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Rows_affected_parameter(bool async)
    {
        await using var context = CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.WithRowsAffectedParameter.Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        entity.Name = "Updated";

        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Updated", (await context.WithRowsAffectedParameter.SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Rows_affected_parameter_and_concurrency_failure(bool async)
    {
        await using var context1 = CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.WithRowsAffectedParameter.Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = CreateContext())
        {
            var entity2 = await context2.WithRowsAffectedParameter.SingleAsync(w => w.Name == "Initial");
            context2.WithRowsAffectedParameter.Remove(entity2);
            await context2.SaveChangesAsync();
        }

        ClearLog();

        entity1.Name = "Updated";

        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await SaveChanges(context1, async));
        var entry = exception.Entries.Single();
        Assert.Same(entity1, entry.Entity);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Rows_affected_result_column(bool async)
    {
        await using var context = CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.WithRowsAffectedResultColumn.Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        entity.Name = "Updated";

        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Updated", (await context.WithRowsAffectedResultColumn.SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Rows_affected_result_column_and_concurrency_failure(bool async)
    {
        await using var context1 = CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.WithRowsAffectedResultColumn.Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = CreateContext())
        {
            var entity2 = await context2.WithRowsAffectedResultColumn.SingleAsync(w => w.Name == "Initial");
            context2.WithRowsAffectedResultColumn.Remove(entity2);
            await context2.SaveChangesAsync();
        }

        ClearLog();

        entity1.Name = "Updated";

        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await SaveChanges(context1, async));
        var entry = exception.Entries.Single();
        Assert.Same(entity1, entry.Entity);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Rows_affected_return_value(bool async)
    {
        await using var context = CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.WithRowsAffectedReturnValue.Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        entity.Name = "Updated";

        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Updated", (await context.WithRowsAffectedReturnValue.SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Rows_affected_return_value_and_concurrency_failure(bool async)
    {
        await using var context1 = CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.WithRowsAffectedReturnValue.Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = CreateContext())
        {
            var entity2 = await context2.WithRowsAffectedReturnValue.SingleAsync(w => w.Name == "Initial");
            context2.WithRowsAffectedReturnValue.Remove(entity2);
            await SaveChanges(context2, async);
        }

        ClearLog();

        entity1.Name = "Updated";

        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await SaveChanges(context1, async));
        var entry = exception.Entries.Single();
        Assert.Same(entity1, entry.Entity);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Store_generated_concurrency_token_as_inout_parameter(bool async)
    {
        await using var context1 = CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.WithStoreGeneratedConcurrencyTokenAsInoutParameter.Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = CreateContext())
        {
            var entity2 = await context2.WithStoreGeneratedConcurrencyTokenAsInoutParameter.SingleAsync(w => w.Name == "Initial");
            entity2.Name = "Preempted";
            await SaveChanges(context2, async);
        }

        ClearLog();

        entity1.Name = "Updated";

        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await SaveChanges(context1, async));
        var entry = exception.Entries.Single();
        Assert.Same(entity1, entry.Entity);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Store_generated_concurrency_token_as_two_parameters(bool async)
    {
        await using var context1 = CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.WithStoreGeneratedConcurrencyTokenAsTwoParameters.Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = CreateContext())
        {
            var entity2 = await context2.WithStoreGeneratedConcurrencyTokenAsTwoParameters.SingleAsync(w => w.Name == "Initial");
            entity2.Name = "Preempted";
            await SaveChanges(context2, async);
        }

        ClearLog();

        entity1.Name = "Updated";

        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await SaveChanges(context1, async));
        var entry = exception.Entries.Single();
        Assert.Same(entity1, entry.Entity);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task User_managed_concurrency_token(bool async)
    {
        await using var context1 = CreateContext();

        var entity1 = new EntityWithAdditionalProperty
        {
            Name = "Initial",
            AdditionalProperty = 8 // The concurrency token
        };

        context1.WithUserManagedConcurrencyToken.Add(entity1);
        await context1.SaveChangesAsync();

        entity1.Name = "Updated";
        entity1.AdditionalProperty = 9;

        await using (var context2 = CreateContext())
        {
            var entity2 = await context2.WithUserManagedConcurrencyToken.SingleAsync(w => w.Name == "Initial");
            entity2.Name = "Preempted";
            entity2.AdditionalProperty = 999;
            await SaveChanges(context2, async);
        }

        ClearLog();

        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () => await SaveChanges(context1, async));
        Assert.Same(entity1, Assert.Single(exception.Entries).Entity);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Original_and_current_value_on_non_concurrency_token(bool async)
    {
        await using var context = CreateContext();

        var entity = new Entity
        {
            Name = "Initial",
        };

        context.WithOriginalAndCurrentValueOnNonConcurrencyToken.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "Updated";

        ClearLog();

        await SaveChanges(context, async);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(
                "Updated",
                (await context.WithOriginalAndCurrentValueOnNonConcurrencyToken.SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Input_output_parameter_on_non_concurrency_token(bool async)
    {
        await using var context = CreateContext();

        var entity = new Entity { Name = "Initial", };
        context.WithInputOutputParameterOnNonConcurrencyToken.Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "Updated";

        ClearLog();

        await SaveChanges(context, async);

        // TODO: This (and below) should be UpdatedWithSuffix. Reference issue tracking this.
        Assert.Equal("Updated", entity.Name);

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(
                "Updated", (await context.WithInputOutputParameterOnNonConcurrencyToken.SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tph(bool async)
    {
        await using var context = CreateContext();

        var entity1 = new TphChild1 { Name = "Child", Child1Property = 8 };
        context.TphChild.Add(entity1);
        await SaveChanges(context, async);

        context.ChangeTracker.Clear();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var entity2 = context.TphChild.Single(b => b.Id == entity1.Id);

            Assert.Equal("Child", entity2.Name);
            Assert.Equal(8, entity2.Child1Property);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt(bool async)
    {
        await using var context = CreateContext();

        var entity1 = new TptChild { Name = "Child", ChildProperty = 8 };
        context.TptChild.Add(entity1);
        await SaveChanges(context, async);

        context.ChangeTracker.Clear();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var entity2 = context.TptChild.Single(b => b.Id == entity1.Id);

            Assert.Equal("Child", entity2.Name);
            Assert.Equal(8, entity2.ChildProperty);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpt_mixed_sproc_and_non_sproc(bool async)
    {
        await using var context = CreateContext();

        var entity1 = new TptMixedChild { Name = "Child", ChildProperty = 8 };
        context.TptMixedChild.Add(entity1);
        await SaveChanges(context, async);

        context.ChangeTracker.Clear();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var entity2 = context.TptMixedChild.Single(b => b.Id == entity1.Id);

            Assert.Equal("Child", entity2.Name);
            Assert.Equal(8, entity2.ChildProperty);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Tpc(bool async)
    {
        await using var context = CreateContext();

        var entity1 = new TpcChild { Name = "Child", ChildProperty = 8 };
        context.TpcChild.Add(entity1);
        await SaveChanges(context, async);

        context.ChangeTracker.Clear();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var entity2 = context.TpcChild.Single(b => b.Id == entity1.Id);

            Assert.Equal("Child", entity2.Name);
            Assert.Equal(8, entity2.ChildProperty);
        }
    }

    private async Task SaveChanges(StoredProcedureUpdateContext context, bool async)
    {
        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }
    }

    protected StoredProcedureUpdateContext CreateContext()
        => Fixture.CreateContext();

    public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };

    protected virtual void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected virtual void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected TFixture Fixture { get; }
}
