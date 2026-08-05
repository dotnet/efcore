// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPHInheritanceQueryTestBase<TFixture> : InheritanceQueryTestBase<TFixture>
    where TFixture : TPHInheritanceQueryFixture, new()
{
    protected TPHInheritanceQueryTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void FromSql_on_root()
    {
        using var context = CreateContext();
        context.Set<Animal>().FromSqlRaw(NormalizeDelimitersInRawString("select * from [Animals]")).ToList();
    }

    [ConditionalFact]
    public virtual void FromSql_on_derived()
    {
        using var context = CreateContext();
        context.Set<Eagle>().FromSqlRaw(NormalizeDelimitersInRawString("select * from [Animals]")).ToList();
    }

    [ConditionalFact]
    public virtual void Casting_to_base_type_joining_with_query_type_works()
    {
        using var context = CreateContext();
        var query = context.Set<Eagle>();

        GetEntityWithAuditHistoryQuery(context, query);
    }

    private void GetEntityWithAuditHistoryQuery<T>(InheritanceContext context, IQueryable<T> query)
        where T : Animal
    {
        var queryTypeQuery = context.Set<AnimalQuery>().FromSqlRaw(NormalizeDelimitersInRawString("Select * from [Animals]"));

        var animalQuery = query.Cast<Animal>();

        var joinQuery =
            from animal in animalQuery
            join keylessanimal in queryTypeQuery on animal.Name equals keylessanimal.Name
            select new { animal, keylessanimal };

        var result = joinQuery.ToList();

        Assert.Single(result);
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    private string NormalizeDelimitersInRawString(string sql)
        => ((RelationalTestStore)Fixture.TestStore).NormalizeDelimitersInRawString(sql);

    private FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
        => ((RelationalTestStore)Fixture.TestStore).NormalizeDelimitersInInterpolatedString(sql);
}
