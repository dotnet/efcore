// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class GearsOfWarFromSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : GearsOfWarQueryRelationalFixture, new()
{
    protected GearsOfWarFromSqlQueryTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    [ConditionalFact]
    public virtual void From_sql_queryable_simple_columns_out_of_order()
    {
        using var context = CreateContext();
        var actual = context.Set<Weapon>().FromSqlRaw(
                NormalizeDelimitersInRawString(
                    "SELECT [Id], [Name], [IsAutomatic], [AmmunitionType], [OwnerFullName], [SynergyWithId] FROM [Weapons] ORDER BY [Name]"))
            .ToArray();

        Assert.Equal(10, actual.Length);

        var first = actual.First();

        Assert.Equal(AmmunitionType.Shell, first.AmmunitionType);
        Assert.Equal("Baird's Gnasher", first.Name);
    }

    private string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    private FormattableString NormalizeDelimitersInInterpolatedString(FormattableString sql)
        => Fixture.TestStore.NormalizeDelimitersInInterpolatedString(sql);

    protected GearsOfWarContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }
}
