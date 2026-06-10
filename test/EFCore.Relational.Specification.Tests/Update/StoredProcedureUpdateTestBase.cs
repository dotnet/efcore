// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public abstract class StoredProcedureUpdateTestBase : NonSharedModelTestBase
{
    protected override string StoreName
        => "StoredProcedureUpdateTest";

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Insert_with_output_parameter(bool async);

    protected async Task Insert_with_output_parameter(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .InsertUsingStoredProcedure(
                    nameof(Entity) + "_Insert",
                    spb => spb
                        .HasParameter(w => w.Name)
                        .HasParameter(w => w.Id, pb => pb.IsOutput())),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var newEntity1 = new Entity { Name = "New" };
        context.Set<Entity>().Add(newEntity1);
        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("New", context.Set<Entity>().Single(b => b.Id == newEntity1.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Insert_twice_with_output_parameter(bool async);

    protected async Task Insert_twice_with_output_parameter(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .InsertUsingStoredProcedure(
                    nameof(Entity) + "_Insert",
                    spb => spb
                        .HasParameter(w => w.Name)
                        .HasParameter(w => w.Id, pb => pb.IsOutput())),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var (newEntity1, newEntity2) = (new Entity { Name = "New1" }, new Entity { Name = "New2" });

        context.Set<Entity>().AddRange(newEntity1, newEntity2);
        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("New1", context.Set<Entity>().Single(b => b.Id == newEntity1.Id).Name);
            Assert.Equal("New2", context.Set<Entity>().Single(b => b.Id == newEntity2.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Insert_with_result_column(bool async);

    protected async Task Insert_with_result_column(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>().InsertUsingStoredProcedure(
                nameof(Entity) + "_Insert", spb => spb
                    .HasParameter(w => w.Name)
                    .HasResultColumn(w => w.Id)),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity { Name = "Foo" };
        context.Set<Entity>().Add(entity);
        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Foo", context.Set<Entity>().Single(b => b.Id == entity.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Insert_with_two_result_columns(bool async);

    protected async Task Insert_with_two_result_columns(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<EntityWithAdditionalProperty>(
                b =>
                {
                    b.Property(w => w.AdditionalProperty).HasComputedColumnSql("8");

                    b.InsertUsingStoredProcedure(
                        nameof(EntityWithAdditionalProperty) + "_Insert", spb => spb
                            .HasParameter(w => w.Name)
                            .HasResultColumn(w => w.AdditionalProperty)
                            .HasResultColumn(w => w.Id));
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Foo" };
        context.Set<EntityWithAdditionalProperty>().Add(entity);
        await SaveChanges(context, async);

        Assert.Equal(1, entity.Id);
        Assert.Equal(8, entity.AdditionalProperty);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Foo", context.Set<EntityWithAdditionalProperty>().Single(b => b.Id == entity.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Insert_with_output_parameter_and_result_column(bool async);

    protected async Task Insert_with_output_parameter_and_result_column(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<EntityWithAdditionalProperty>(
                b =>
                {
                    b.Property(w => w.AdditionalProperty).HasComputedColumnSql("8");

                    b.InsertUsingStoredProcedure(
                        nameof(EntityWithAdditionalProperty) + "_Insert", spb => spb
                            .HasParameter(w => w.Id, pb => pb.IsOutput())
                            .HasParameter(w => w.Name)
                            .HasResultColumn(w => w.AdditionalProperty));
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Foo" };
        context.Set<EntityWithAdditionalProperty>().Add(entity);
        await SaveChanges(context, async);

        Assert.Equal(8, entity.AdditionalProperty);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Foo", context.Set<EntityWithAdditionalProperty>().Single(b => b.Id == entity.Id).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Update(bool async);

    protected async Task Update(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>().UpdateUsingStoredProcedure(
                nameof(Entity) + "_Update",
                spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name)),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.Set<Entity>().Add(entity);
        await SaveChanges(context, async);

        ClearLog();

        entity.Name = "Updated";
        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Updated", (await context.Set<Entity>().SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Update_partial(bool async);

    protected async Task Update_partial(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<EntityWithAdditionalProperty>().UpdateUsingStoredProcedure(
                nameof(EntityWithAdditionalProperty) + "_Update", spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name)
                    .HasParameter(w => w.AdditionalProperty)),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Foo", AdditionalProperty = 8 };
        context.Set<EntityWithAdditionalProperty>().Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "Updated";

        ClearLog();

        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var actual = await context.Set<EntityWithAdditionalProperty>().SingleAsync(w => w.Id == entity.Id);

            Assert.Equal("Updated", actual.Name);
            Assert.Equal(8, actual.AdditionalProperty);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Update_with_output_parameter_and_rows_affected_result_column(bool async);

    protected async Task Update_with_output_parameter_and_rows_affected_result_column(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<EntityWithAdditionalProperty>(
                b =>
                {
                    b.Property(w => w.AdditionalProperty).HasComputedColumnSql("8");

                    b.UpdateUsingStoredProcedure(
                        nameof(EntityWithAdditionalProperty) + "_Update",
                        spb => spb
                            .HasOriginalValueParameter(w => w.Id)
                            .HasParameter(w => w.Name)
                            .HasParameter(w => w.AdditionalProperty, pb => pb.IsOutput())
                            .HasRowsAffectedResultColumn());
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new EntityWithAdditionalProperty { Name = "Foo" };
        context.Set<EntityWithAdditionalProperty>().Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "Updated";

        ClearLog();

        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var actual = await context.Set<EntityWithAdditionalProperty>().SingleAsync(w => w.Id == entity.Id);

            Assert.Equal("Updated", actual.Name);
            Assert.Equal(8, actual.AdditionalProperty);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(bool async);

    protected async Task Update_with_output_parameter_and_rows_affected_result_column_concurrency_failure(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<EntityWithAdditionalProperty>(
                b =>
                {
                    b.Property(w => w.AdditionalProperty).HasComputedColumnSql("8");

                    b.UpdateUsingStoredProcedure(
                        nameof(EntityWithAdditionalProperty) + "_Update",
                        spb => spb
                            .HasOriginalValueParameter(w => w.Id)
                            .HasParameter(w => w.Name)
                            .HasParameter(w => w.AdditionalProperty, pb => pb.IsOutput())
                            .HasRowsAffectedResultColumn());
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context1 = contextFactory.CreateContext();

        var entity1 = new EntityWithAdditionalProperty { Name = "Initial" };
        context1.Set<EntityWithAdditionalProperty>().Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = contextFactory.CreateContext())
        {
            var entity2 = await context2.Set<EntityWithAdditionalProperty>().SingleAsync(w => w.Name == "Initial");
            context2.Set<EntityWithAdditionalProperty>().Remove(entity2);
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
    public abstract Task Delete(bool async);

    protected async Task Delete(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .DeleteUsingStoredProcedure(
                    nameof(Entity) + "_Delete",
                    spb => spb.HasOriginalValueParameter(w => w.Id)),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.Set<Entity>().Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        context.Set<Entity>().Remove(entity);
        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(0, await context.Set<Entity>().CountAsync(b => b.Name == "Initial"));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Delete_and_insert(bool async);

    protected async Task Delete_and_insert(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .InsertUsingStoredProcedure(
                    nameof(Entity) + "_Insert",
                    spb => spb
                        .HasParameter(w => w.Name)
                        .HasParameter(w => w.Id, pb => pb.IsOutput()))
                .DeleteUsingStoredProcedure(
                    nameof(Entity) + "_Delete",
                    spb => spb.HasOriginalValueParameter(w => w.Id)),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity1 = new Entity { Name = "Entity1" };
        context.Set<Entity>().Add(entity1);
        await context.SaveChangesAsync();

        ClearLog();

        context.Set<Entity>().Remove(entity1);
        context.Set<Entity>().Add(new Entity { Name = "Entity2" });
        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(0, await context.Set<Entity>().CountAsync(b => b.Name == "Entity1"));
            Assert.Equal(1, await context.Set<Entity>().CountAsync(b => b.Name == "Entity2"));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Rows_affected_parameter(bool async);

    protected async Task Rows_affected_parameter(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .UpdateUsingStoredProcedure(
                    nameof(Entity) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasParameter(w => w.Name)
                        .HasRowsAffectedParameter()),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.Set<Entity>().Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        entity.Name = "Updated";

        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Updated", (await context.Set<Entity>().SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Rows_affected_parameter_and_concurrency_failure(bool async);

    protected async Task Rows_affected_parameter_and_concurrency_failure(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .UpdateUsingStoredProcedure(
                    nameof(Entity) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasParameter(w => w.Name)
                        .HasRowsAffectedParameter()),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context1 = contextFactory.CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.Set<Entity>().Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = contextFactory.CreateContext())
        {
            var entity2 = await context2.Set<Entity>().SingleAsync(w => w.Name == "Initial");
            context2.Set<Entity>().Remove(entity2);
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
    public abstract Task Rows_affected_result_column(bool async);

    protected async Task Rows_affected_result_column(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .UpdateUsingStoredProcedure(
                    nameof(Entity) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasParameter(w => w.Name)
                        .HasRowsAffectedResultColumn()),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.Set<Entity>().Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        entity.Name = "Updated";

        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Updated", (await context.Set<Entity>().SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Rows_affected_result_column_and_concurrency_failure(bool async);

    protected async Task Rows_affected_result_column_and_concurrency_failure(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .UpdateUsingStoredProcedure(
                    nameof(Entity) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasParameter(w => w.Name)
                        .HasRowsAffectedResultColumn()),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context1 = contextFactory.CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.Set<Entity>().Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = contextFactory.CreateContext())
        {
            var entity2 = await context2.Set<Entity>().SingleAsync(w => w.Name == "Initial");
            context2.Set<Entity>().Remove(entity2);
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
    public abstract Task Rows_affected_return_value(bool async);

    protected async Task Rows_affected_return_value(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .UpdateUsingStoredProcedure(
                    nameof(Entity) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasParameter(w => w.Name)
                        .HasRowsAffectedReturnValue()),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.Set<Entity>().Add(entity);
        await context.SaveChangesAsync();

        ClearLog();

        entity.Name = "Updated";

        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Updated", (await context.Set<Entity>().SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Rows_affected_return_value_and_concurrency_failure(bool async);

    protected async Task Rows_affected_return_value_and_concurrency_failure(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .UpdateUsingStoredProcedure(
                    nameof(Entity) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasParameter(w => w.Name)
                        .HasRowsAffectedReturnValue()),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context1 = contextFactory.CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.Set<Entity>().Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = contextFactory.CreateContext())
        {
            var entity2 = await context2.Set<Entity>().SingleAsync(w => w.Name == "Initial");
            context2.Set<Entity>().Remove(entity2);
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
    public abstract Task Store_generated_concurrency_token_as_in_out_parameter(bool async);

    protected async Task Store_generated_concurrency_token_as_in_out_parameter(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>(
                b =>
                {
                    ConfigureStoreGeneratedConcurrencyToken(b, "ConcurrencyToken");

                    b.UpdateUsingStoredProcedure(
                        nameof(Entity) + "_Update",
                        spb => spb
                            .HasOriginalValueParameter(w => w.Id)
                            .HasOriginalValueParameter("ConcurrencyToken", pb => pb.IsInputOutput())
                            .HasParameter(w => w.Name)
                            .HasRowsAffectedParameter());
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context1 = contextFactory.CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.Set<Entity>().Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = contextFactory.CreateContext())
        {
            var entity2 = await context2.Set<Entity>().SingleAsync(w => w.Name == "Initial");
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
    public abstract Task Store_generated_concurrency_token_as_two_parameters(bool async);

    protected async Task Store_generated_concurrency_token_as_two_parameters(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>(
                b =>
                {
                    ConfigureStoreGeneratedConcurrencyToken(b, "ConcurrencyToken");

                    b.UpdateUsingStoredProcedure(
                        nameof(Entity) + "_Update",
                        spb => spb
                            .HasOriginalValueParameter(w => w.Id)
                            .HasOriginalValueParameter("ConcurrencyToken", pb => pb.HasName("ConcurrencyTokenIn"))
                            .HasParameter(w => w.Name)
                            .HasParameter(
                                "ConcurrencyToken", pb => pb
                                    .HasName("ConcurrencyTokenOut")
                                    .IsOutput())
                            .HasRowsAffectedParameter());
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context1 = contextFactory.CreateContext();

        var entity1 = new Entity { Name = "Initial" };
        context1.Set<Entity>().Add(entity1);
        await context1.SaveChangesAsync();

        await using (var context2 = contextFactory.CreateContext())
        {
            var entity2 = await context2.Set<Entity>().SingleAsync(w => w.Name == "Initial");
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
    public abstract Task User_managed_concurrency_token(bool async);

    protected async Task User_managed_concurrency_token(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<EntityWithAdditionalProperty>(
                b =>
                {
                    b.Property(e => e.AdditionalProperty).IsConcurrencyToken();

                    b.UpdateUsingStoredProcedure(
                        nameof(EntityWithAdditionalProperty) + "_Update",
                        spb => spb
                            .HasOriginalValueParameter(w => w.Id)
                            .HasOriginalValueParameter(w => w.AdditionalProperty, pb => pb.HasName("ConcurrencyTokenOriginal"))
                            .HasParameter(w => w.Name)
                            .HasParameter(w => w.AdditionalProperty, pb => pb.HasName("ConcurrencyTokenCurrent"))
                            .HasRowsAffectedParameter());
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context1 = contextFactory.CreateContext();

        var entity1 = new EntityWithAdditionalProperty
        {
            Name = "Initial", AdditionalProperty = 8 // The concurrency token
        };

        context1.Set<EntityWithAdditionalProperty>().Add(entity1);
        await context1.SaveChangesAsync();

        entity1.Name = "Updated";
        entity1.AdditionalProperty = 9;

        await using (var context2 = contextFactory.CreateContext())
        {
            var entity2 = await context2.Set<EntityWithAdditionalProperty>().SingleAsync(w => w.Name == "Initial");
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
    public abstract Task Original_and_current_value_on_non_concurrency_token(bool async);

    protected async Task Original_and_current_value_on_non_concurrency_token(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>()
                .UpdateUsingStoredProcedure(
                    nameof(Entity) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasParameter(w => w.Name, pb => pb.HasName("NameCurrent"))
                        .HasOriginalValueParameter(w => w.Name, pb => pb.HasName("NameOriginal"))),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity { Name = "Initial" };

        context.Set<Entity>().Add(entity);
        await context.SaveChangesAsync();

        entity.Name = "Updated";

        ClearLog();

        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(
                "Updated",
                (await context.Set<Entity>().SingleAsync(w => w.Id == entity.Id)).Name);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Input_or_output_parameter_with_input(bool async);

    protected async Task Input_or_output_parameter_with_input(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>(
                b =>
                {
                    b.Property(w => w.Name).IsRequired().ValueGeneratedOnAdd();

                    b.InsertUsingStoredProcedure(
                        nameof(Entity) + "_Insert",
                        spb => spb
                            .HasParameter(w => w.Id, pb => pb.IsOutput())
                            .HasParameter(w => w.Name, pb => pb.IsInputOutput()));
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity { Name = "Initial" };
        context.Set<Entity>().Add(entity);
        await SaveChanges(context, async);

        Assert.Equal("Initial", entity.Name);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Same(
                entity, await context.Set<Entity>().SingleAsync(w => w.Id == entity.Id && w.Name == "Initial"));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Input_or_output_parameter_with_output(bool async);

    protected async Task Input_or_output_parameter_with_output(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<Entity>(
                b =>
                {
                    b.Property(w => w.Name).IsRequired().ValueGeneratedOnAdd();

                    b.InsertUsingStoredProcedure(
                        nameof(Entity) + "_Insert",
                        spb => spb
                            .HasParameter(w => w.Id, pb => pb.IsOutput())
                            .HasParameter(w => w.Name, pb => pb.IsInputOutput()));
                }),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity = new Entity();
        context.Set<Entity>().Add(entity);
        await SaveChanges(context, async);

        Assert.Equal("Some default value", entity.Name);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Same(
                entity, await context.Set<Entity>().SingleAsync(w => w.Id == entity.Id && w.Name == "Some default value"));
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Tph(bool async);

    protected async Task Tph(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder =>
            {
                modelBuilder.Entity<Child1>();

                modelBuilder.Entity<Child2>(
                    b =>
                    {
                        b.Property(w => w.Child2OutputParameterProperty).HasDefaultValue(8);
                        b.Property(w => w.Child2ResultColumnProperty).HasDefaultValue(9);
                    });

                modelBuilder.Entity<Parent>(
                    b =>
                    {
                        b.ToTable("Tph");

                        b.InsertUsingStoredProcedure(
                            "Tph_Insert",
                            spb => spb
                                .HasParameter(w => w.Id, pb => pb.IsOutput())
                                .HasParameter("Discriminator")
                                .HasParameter(w => w.Name)
                                .HasParameter(nameof(Child2.Child2InputProperty))
                                .HasParameter(nameof(Child2.Child2OutputParameterProperty), o => o.IsOutput())
                                .HasParameter(nameof(Child1.Child1Property))
                                .HasResultColumn(nameof(Child2.Child2ResultColumnProperty)));
                    });
            },
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity1 = new Child1 { Name = "Child", Child1Property = 8 };
        context.Set<Child1>().Add(entity1);
        await SaveChanges(context, async);

        context.ChangeTracker.Clear();

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var entity2 = context.Set<Child1>().Single(b => b.Id == entity1.Id);

            Assert.Equal("Child", entity2.Name);
            Assert.Equal(8, entity2.Child1Property);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Tpt(bool async);

    protected async Task Tpt(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder =>
            {
                modelBuilder.Entity<Parent>(
                    b =>
                    {
                        b.UseTptMappingStrategy();

                        b.InsertUsingStoredProcedure(
                            "Parent_Insert",
                            spb => spb
                                .HasParameter(w => w.Id, pb => pb.IsOutput())
                                .HasParameter(w => w.Name));
                    });

                modelBuilder.Entity<Child1>()
                    .InsertUsingStoredProcedure(
                        nameof(Child1) + "_Insert",
                        spb => spb
                            .HasParameter(w => w.Id)
                            .HasParameter(w => w.Child1Property));
            },
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity1 = new Child1 { Name = "Child", Child1Property = 8 };
        context.Set<Child1>().Add(entity1);
        await SaveChanges(context, async);

        context.ChangeTracker.Clear();

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var entity2 = context.Set<Child1>().Single(b => b.Id == entity1.Id);

            Assert.Equal("Child", entity2.Name);
            Assert.Equal(8, entity2.Child1Property);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Tpt_mixed_sproc_and_non_sproc(bool async);

    protected async Task Tpt_mixed_sproc_and_non_sproc(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder =>
            {
                modelBuilder.Entity<Parent>(
                    b =>
                    {
                        b.UseTptMappingStrategy();

                        b.InsertUsingStoredProcedure(
                            nameof(Parent) + "_Insert",
                            spb => spb
                                .HasParameter(w => w.Id, pb => pb.IsOutput())
                                .HasParameter(w => w.Name));
                    });

                // No sproc mapping for Child1, use regular SQL
                modelBuilder.Entity<Child1>();
            },
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity1 = new Child1 { Name = "Child", Child1Property = 8 };
        context.Set<Child1>().Add(entity1);
        await SaveChanges(context, async);

        context.ChangeTracker.Clear();

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var entity2 = context.Set<Child1>().Single(b => b.Id == entity1.Id);

            Assert.Equal("Child", entity2.Name);
            Assert.Equal(8, entity2.Child1Property);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Tpc(bool async);

    protected async Task Tpc(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder =>
            {
                modelBuilder.Entity<Parent>().UseTpcMappingStrategy();

                modelBuilder.Entity<Child1>()
                    .UseTpcMappingStrategy()
                    .InsertUsingStoredProcedure(
                        nameof(Child1) + "_Insert",
                        spb => spb
                            .HasParameter(w => w.Id, pb => pb.IsOutput())
                            .HasParameter(w => w.Name)
                            .HasParameter(w => w.Child1Property));
            },
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        var entity1 = new Child1 { Name = "Child", Child1Property = 8 };
        context.Set<Child1>().Add(entity1);
        await SaveChanges(context, async);

        context.ChangeTracker.Clear();

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            var entity2 = context.Set<Child1>().Single(b => b.Id == entity1.Id);

            Assert.Equal("Child", entity2.Name);
            Assert.Equal(8, entity2.Child1Property);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public abstract Task Non_sproc_followed_by_sproc_commands_in_the_same_batch(bool async);

    protected async Task Non_sproc_followed_by_sproc_commands_in_the_same_batch(bool async, string createSprocSql)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            modelBuilder => modelBuilder.Entity<EntityWithAdditionalProperty>()
                .InsertUsingStoredProcedure(
                    nameof(EntityWithAdditionalProperty) + "_Insert",
                    spb => spb
                        .HasParameter(w => w.Name)
                        .HasParameter(w => w.Id, pb => pb.IsOutput())
                        .HasParameter(w => w.AdditionalProperty))
                .Property(e => e.AdditionalProperty).IsConcurrencyToken(),
            seed: ctx => CreateStoredProcedures(ctx, createSprocSql));

        await using var context = contextFactory.CreateContext();

        // Prepare by adding an entity
        var entity1 = new EntityWithAdditionalProperty { Name = "Entity1", AdditionalProperty = 1 };
        context.Set<EntityWithAdditionalProperty>().Add(entity1);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            await SaveChanges(context, async);
        }

        // Now add a second entity and update the first one. The update gets ordered first, and doesn't use a sproc, and then the insertion
        // does.
        var entity2 = new EntityWithAdditionalProperty { Name = "Entity2" };
        context.Set<EntityWithAdditionalProperty>().Add(entity2);
        entity1.Name = "Entity1_Modified";
        entity1.AdditionalProperty = 2;
        await SaveChanges(context, async);

        using (TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal("Entity2", context.Set<EntityWithAdditionalProperty>().Single(b => b.Id == entity2.Id).Name);
        }
    }

    /// <summary>
    ///     A method to be implement by the provider, to set up a store-generated concurrency token shadow property with the given name.
    /// </summary>
    protected abstract void ConfigureStoreGeneratedConcurrencyToken(EntityTypeBuilder entityTypeBuilder, string propertyName);

    protected class Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    protected class EntityWithAdditionalProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AdditionalProperty { get; set; }
    }

    protected class Child1 : Parent
    {
        public int Child1Property { get; set; }
    }

    protected class Child2 : Parent
    {
        public int Child2InputProperty { get; set; }
        public int Child2OutputParameterProperty { get; set; }
        public int Child2ResultColumnProperty { get; set; }
    }

    protected class Parent
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private async Task SaveChanges(DbContext context, bool async)
    {
        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            // ReSharper disable once MethodHasAsyncOverload
            context.SaveChanges();
        }
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected virtual void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected virtual async Task CreateStoredProcedures(DbContext context, string createSprocSql)
    {
        foreach (var batch in
                 new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromMilliseconds(1000.0))
                     .Split(createSprocSql).Where(b => !string.IsNullOrEmpty(b)))
        {
            await context.Database.ExecuteSqlRawAsync(batch);
        }
    }
}
