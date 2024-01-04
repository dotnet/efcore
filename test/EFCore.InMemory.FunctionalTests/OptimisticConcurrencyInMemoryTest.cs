// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class OptimisticConcurrencyULongInMemoryTest(F1ULongInMemoryFixture fixture) : OptimisticConcurrencyInMemoryTestBase<F1ULongInMemoryFixture, ulong>(fixture);

public class OptimisticConcurrencyInMemoryTest(F1InMemoryFixture fixture) : OptimisticConcurrencyInMemoryTestBase<F1InMemoryFixture, byte[]>(fixture);

public abstract class OptimisticConcurrencyInMemoryTestBase<TFixture, TRowVersion>
    : OptimisticConcurrencyTestBase<TFixture, TRowVersion>
    where TFixture : F1FixtureBase<TRowVersion>, new()
{
    protected OptimisticConcurrencyInMemoryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Simple_concurrency_exception_can_be_resolved_with_store_values()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Simple_concurrency_exception_can_be_resolved_with_client_values()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Simple_concurrency_exception_can_be_resolved_with_new_values()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_equivalent_of_accept_changes()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_Reload()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task
        Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task
        Change_in_independent_association_after_change_in_different_concurrency_token_results_in_independent_association_exception()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Change_in_independent_association_results_in_independent_association_exception()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Two_concurrency_issues_in_one_to_many_related_entities_can_be_handled_by_dealing_with_dependent_first()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Two_concurrency_issues_in_one_to_one_related_entities_can_be_handled_by_dealing_with_dependent_first()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Throw DbUpdateException or DbUpdateConcurrencyException for in-memory database errors #23569")]
    public override Task Adding_the_same_entity_twice_results_in_DbUpdateException()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Throw DbUpdateException or DbUpdateConcurrencyException for in-memory database errors #23569")]
    public override Task Deleting_the_same_entity_twice_results_in_DbUpdateConcurrencyException()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Throw DbUpdateException or DbUpdateConcurrencyException for in-memory database errors #23569")]
    public override Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Attempting_to_delete_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        => Task.CompletedTask;

    [ConditionalFact(Skip = "Optimistic Offline Lock #2195")]
    public override Task Attempting_to_add_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        => Task.CompletedTask;
}
