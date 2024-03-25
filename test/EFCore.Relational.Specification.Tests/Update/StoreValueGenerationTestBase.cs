// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public abstract class StoreValueGenerationTestBase<TFixture> : IClassFixture<TFixture>, IAsyncLifetime
    where TFixture : StoreValueGenerationFixtureBase
{
    protected StoreValueGenerationTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    #region Single operation

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_with_generated_values(bool async)
        => Test(EntityState.Added, secondOperationType: null, GeneratedValues.Some, async);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_with_no_generated_values(bool async)
        => Test(EntityState.Added, secondOperationType: null, GeneratedValues.None, async);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_with_all_generated_values(bool async)
        => Test(EntityState.Added, secondOperationType: null, GeneratedValues.All, async);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Modify_with_generated_values(bool async)
        => Test(EntityState.Modified, secondOperationType: null, GeneratedValues.Some, async);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Modify_with_no_generated_values(bool async)
        => Test(EntityState.Modified, secondOperationType: null, GeneratedValues.None, async);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete(bool async)
        => Test(EntityState.Deleted, secondOperationType: null, GeneratedValues.Some, async);

    #endregion Single operation

    #region Same two operations with same entity type

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_Add_with_same_entity_type_and_generated_values(bool async)
        => Test(EntityState.Added, EntityState.Added, GeneratedValues.Some, async, withSameEntityType: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_Add_with_same_entity_type_and_no_generated_values(bool async)
        => Test(EntityState.Added, EntityState.Added, GeneratedValues.None, async, withSameEntityType: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_Add_with_same_entity_type_and_all_generated_values(bool async)
        => Test(EntityState.Added, EntityState.Added, GeneratedValues.All, async, withSameEntityType: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Modify_Modify_with_same_entity_type_and_generated_values(bool async)
        => Test(EntityState.Modified, EntityState.Modified, GeneratedValues.Some, async, withSameEntityType: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Modify_Modify_with_same_entity_type_and_no_generated_values(bool async)
        => Test(EntityState.Modified, EntityState.Modified, GeneratedValues.None, async, withSameEntityType: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Delete_with_same_entity_type(bool async)
        => Test(EntityState.Deleted, EntityState.Deleted, GeneratedValues.Some, async, withSameEntityType: true);

    #endregion Same two operations with same entity type

    #region Same two operations with different entity types

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_Add_with_different_entity_types_and_generated_values(bool async)
        => Test(EntityState.Added, EntityState.Added, GeneratedValues.Some, async, withSameEntityType: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_Add_with_different_entity_types_and_no_generated_values(bool async)
        => Test(EntityState.Added, EntityState.Added, GeneratedValues.None, async, withSameEntityType: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Add_Add_with_different_entity_types_and_all_generated_values(bool async)
        => Test(EntityState.Added, EntityState.Added, GeneratedValues.All, async, withSameEntityType: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Modify_Modify_with_different_entity_types_and_generated_values(bool async)
        => Test(EntityState.Modified, EntityState.Modified, GeneratedValues.Some, async, withSameEntityType: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Modify_Modify_with_different_entity_types_and_no_generated_values(bool async)
        => Test(EntityState.Modified, EntityState.Modified, GeneratedValues.None, async, withSameEntityType: false);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Delete_with_different_entity_types(bool async)
        => Test(EntityState.Deleted, EntityState.Deleted, GeneratedValues.Some, async, withSameEntityType: false);

    #endregion Same two operations with different entity types

    #region Different two operations

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Add_with_same_entity_types(bool async)
        => Test(EntityState.Deleted, EntityState.Added, GeneratedValues.Some, async, withSameEntityType: true);

    #endregion Different two operations

    protected virtual async Task Test(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool async,
        bool withSameEntityType = true)
    {
        await using var context = CreateContext();

        var firstDbSet = generatedValues switch
        {
            GeneratedValues.Some => context.WithSomeDatabaseGenerated,
            GeneratedValues.None => context.WithNoDatabaseGenerated,
            GeneratedValues.All => context.WithAllDatabaseGenerated,
            _ => throw new ArgumentOutOfRangeException(nameof(generatedValues))
        };

        var secondDbSet = secondOperationType is null
            ? null
            : (generatedValues, withSameEntityType) switch
            {
                (GeneratedValues.Some, true) => context.WithSomeDatabaseGenerated,
                (GeneratedValues.Some, false) => context.WithSomeDatabaseGenerated2,
                (GeneratedValues.None, true) => context.WithNoDatabaseGenerated,
                (GeneratedValues.None, false) => context.WithNoDatabaseGenerated2,
                (GeneratedValues.All, true) => context.WithAllDatabaseGenerated,
                (GeneratedValues.All, false) => context.WithAllDatabaseGenerated2,
                _ => throw new ArgumentOutOfRangeException(nameof(generatedValues))
            };

        StoreValueGenerationData first;
        StoreValueGenerationData second;

        switch (firstOperationType)
        {
            case EntityState.Added:
                switch (generatedValues)
                {
                    case GeneratedValues.Some:
                        first = new StoreValueGenerationData { Data2 = 1000 };
                        firstDbSet.Add(first);
                        break;
                    case GeneratedValues.None:
                        first = new StoreValueGenerationData
                        {
                            Id = 100,
                            Data1 = 1000,
                            Data2 = 1000
                        };
                        firstDbSet.Add(first);
                        break;
                    case GeneratedValues.All:
                        first = new StoreValueGenerationData();
                        firstDbSet.Add(first);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(generatedValues));
                }

                break;

            case EntityState.Modified:
                switch (generatedValues)
                {
                    case GeneratedValues.Some:
                        first = firstDbSet.OrderBy(w => w.Id).First();
                        first.Data2 = 1000;
                        break;
                    case GeneratedValues.None:
                        first = firstDbSet.OrderBy(w => w.Id).First();
                        (first.Data1, first.Data2) = (1000, 1000);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(generatedValues));
                }

                break;

            case EntityState.Deleted:
                switch (generatedValues)
                {
                    case GeneratedValues.Some:
                        first = firstDbSet.OrderBy(w => w.Id).First();
                        context.Remove(first);
                        break;
                    case GeneratedValues.None:
                        first = firstDbSet.OrderBy(w => w.Id).First();
                        context.Remove(first);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(generatedValues));
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(firstOperationType));
        }

        switch (secondOperationType)
        {
            case EntityState.Added:
                switch (generatedValues)
                {
                    case GeneratedValues.Some:
                        second = new StoreValueGenerationData { Data2 = 1001 };
                        secondDbSet!.Add(second);
                        break;
                    case GeneratedValues.None:
                        second = new StoreValueGenerationData
                        {
                            Id = 101,
                            Data1 = 1001,
                            Data2 = 1001
                        };
                        secondDbSet!.Add(second);
                        break;
                    case GeneratedValues.All:
                        second = new StoreValueGenerationData();
                        secondDbSet!.Add(second);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(generatedValues));
                }

                break;

            case EntityState.Modified:
                switch (generatedValues)
                {
                    case GeneratedValues.Some:
                        second = secondDbSet!.OrderBy(w => w.Id).Skip(1).First();
                        second.Data2 = 1001;
                        break;
                    case GeneratedValues.None:
                        second = secondDbSet!.OrderBy(w => w.Id).Skip(1).First();
                        (second.Data1, second.Data2) = (1001, 1001);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(generatedValues));
                }

                break;

            case EntityState.Deleted:
                switch (generatedValues)
                {
                    case GeneratedValues.Some:
                        second = secondDbSet!.OrderBy(w => w.Id).Skip(1).First();
                        context.Remove(second);
                        break;
                    case GeneratedValues.None:
                        second = secondDbSet!.OrderBy(w => w.Id).Skip(1).First();
                        context.Remove(second);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(generatedValues));
                }

                break;

            case null:
                second = null;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(firstOperationType));
        }

        // Execute
        Fixture.ListLoggerFactory.Clear();

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        // Make sure a transaction was created (or not)
        if (ShouldCreateImplicitTransaction(firstOperationType, secondOperationType, generatedValues, withSameEntityType))
        {
            Assert.Contains(Fixture.ListLoggerFactory.Log, l => l.Id == RelationalEventId.TransactionStarted);
            Assert.Contains(Fixture.ListLoggerFactory.Log, l => l.Id == RelationalEventId.TransactionCommitted);
        }
        else
        {
            Assert.DoesNotContain(Fixture.ListLoggerFactory.Log, l => l.Id == RelationalEventId.TransactionStarted);
            Assert.DoesNotContain(Fixture.ListLoggerFactory.Log, l => l.Id == RelationalEventId.TransactionCommitted);
        }

        // Make sure the updates executed in the expected number of commands
        Assert.Equal(
            ShouldExecuteInNumberOfCommands(firstOperationType, secondOperationType, generatedValues, withSameEntityType),
            Fixture.ListLoggerFactory.Log.Count(l => l.Id == RelationalEventId.CommandExecuted));

        // To make sure generated values have been propagated, re-load the rows from the database and compare
        context.ChangeTracker.Clear();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            if (firstOperationType != EntityState.Deleted)
            {
                Assert.Equal(await firstDbSet.FindAsync(first.Id), first);
            }

            if (second is not null && secondOperationType != EntityState.Deleted)
            {
                Assert.Equal(await secondDbSet!.FindAsync(second.Id), second);
            }
        }
    }

    /// <summary>
    ///     Providers can override this to specify when <see cref="DbContext.SaveChanges()" /> should create a transaction, and when not.
    ///     By default, it's assumed that multiple updates always require a transaction, whereas a single update never does.
    /// </summary>
    protected virtual bool ShouldCreateImplicitTransaction(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool withSameEntityType)
        => secondOperationType is not null;

    /// <summary>
    ///     Providers can override this to specify how many commands (batches) are used to execute the update.
    ///     By default, it's assumed all operations are batched in one command.
    /// </summary>
    protected virtual int ShouldExecuteInNumberOfCommands(
        EntityState firstOperationType,
        EntityState? secondOperationType,
        GeneratedValues generatedValues,
        bool withSameEntityType)
        => 1;

    protected TFixture Fixture { get; }

    protected StoreValueGenerationContext CreateContext()
        => Fixture.CreateContext();

    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    protected virtual void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected virtual void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected enum GeneratedValues
    {
        Some,
        None,
        All
    }

    public async Task InitializeAsync()
    {
        Fixture.CleanData();
        await Fixture.SeedAsync();

        ClearLog();
    }

    public Task DisposeAsync()
        => Task.CompletedTask;
}
