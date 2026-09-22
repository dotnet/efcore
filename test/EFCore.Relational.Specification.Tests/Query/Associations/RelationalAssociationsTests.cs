// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public class RelationalAssociationsTests
{
    internal static async Task FromSql_on_root<TFixture>(QueryTestBase<TFixture> queryTestBase, TFixture fixture)
        where TFixture : SharedStoreFixtureBase<PoolableDbContext>, IQueryFixtureBase, new()
    {
        using var context = fixture.CreateContext();
        var tableName = context.Model.FindEntityType(typeof(RootEntity))?.GetTableName()
            ?? throw new UnreachableException("Couldn't find relational table name for RootEntity");
        var sqlGenerationHelper = context.GetService<ISqlGenerationHelper>();

        await queryTestBase.AssertQuery(
            ss => ((DbSet<RootEntity>)ss.Set<RootEntity>()).FromSqlRaw($"SELECT * FROM {sqlGenerationHelper.DelimitIdentifier(tableName)}"),
            ss => ss.Set<RootEntity>());
    }
}
