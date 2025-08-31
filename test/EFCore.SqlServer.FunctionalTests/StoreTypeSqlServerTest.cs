// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.EntityFrameworkCore;

public class StoreTypeSqlServerTest : StoreTypeRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

    [ConditionalFact]
    public override Task Decimal()
        => TestType(
            30.5m,
            30m,
            onConfiguring: b => b.ConfigureWarnings(c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning)));

    protected override async Task TestExecuteUpdateWithinJsonToNonJsonColumn<T>(
        ContextFactory<DbContext> contextFactory,
        T value,
        T otherValue,
        Func<T, T, bool> comparer)
    {
        if (typeof(T) == typeof(string)
            || typeof(T) == typeof(bool)
            || typeof(T).GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>)))
        {
            await base.TestExecuteUpdateWithinJsonToNonJsonColumn(contextFactory, value, otherValue, comparer);
        }
        else
        {
            // See #36688 for supporting this for relational types other than string/numeric/bool
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.TestExecuteUpdateWithinJsonToNonJsonColumn(contextFactory, value, otherValue, comparer));
            Assert.Equal(RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn, exception.Message);
        }
    }

    // When testing against SQL Server 2025 or later, set the compatibility level to 170 to use the json type instead of nvarchar(max).
    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var options = base.AddOptions(builder);
        return TestEnvironment.SqlServerMajorVersion < 17
            ? options
            : options.UseSqlServerCompatibilityLevel(170);
    }
}
