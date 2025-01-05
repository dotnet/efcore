// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPCInheritanceQueryTestBase<TFixture> : InheritanceQueryTestBase<TFixture>
    where TFixture : TPCInheritanceQueryFixture, new()
{
    protected TPCInheritanceQueryTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Keyless entities does not have TPC
    public override Task Can_query_all_animal_views(bool async)
        => Task.CompletedTask;

    // TPC does not have discriminator
    public override Task Discriminator_used_when_projection_over_derived_type(bool async)
        => Task.CompletedTask;

    // TPC does not have discriminator
    public override Task Discriminator_used_when_projection_over_derived_type2(bool async)
        => Task.CompletedTask;

    // TPC does not have discriminator
    public override Task Discriminator_used_when_projection_over_of_type(bool async)
        => Task.CompletedTask;

    // TPC does not have discriminator
    public override Task Discriminator_with_cast_in_shadow_property(bool async)
        => Task.CompletedTask;

    [ConditionalFact]
    public virtual void Using_from_sql_throws()
    {
        using var context = CreateContext();

        var message = Assert.Throws<InvalidOperationException>(() => context.Set<Bird>().FromSqlRaw("Select * from Birds")).Message;

        Assert.Equal(RelationalStrings.MethodOnNonTphRootNotSupported("FromSqlRaw", typeof(Bird).Name), message);

        message = Assert.Throws<InvalidOperationException>(() => context.Set<Bird>().FromSqlInterpolated($"Select * from Birds"))
            .Message;

        Assert.Equal(RelationalStrings.MethodOnNonTphRootNotSupported("FromSqlInterpolated", typeof(Bird).Name), message);

        message = Assert.Throws<InvalidOperationException>(() => context.Set<Bird>().FromSql($"Select * from Birds"))
            .Message;

        Assert.Equal(RelationalStrings.MethodOnNonTphRootNotSupported("FromSql", typeof(Bird).Name), message);
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
