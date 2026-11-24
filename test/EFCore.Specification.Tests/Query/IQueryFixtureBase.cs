// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public interface IQueryFixtureBase
{
    string StoreName { get; }

    Func<DbContext> GetContextCreator();
    Func<DbContext, ISetSource> GetSetSourceCreator();
    ISetSource GetExpectedData();
    IReadOnlyDictionary<Type, object> EntitySorters { get; }
    IReadOnlyDictionary<Type, object> EntityAsserters { get; }
    ISetSource? GetFilteredExpectedData(DbContext context);
    Action<DatabaseFacade, IDbContextTransaction> GetUseTransaction();
    TestStore GetOrCreateNonSharedTestStore(Func<TestStore> createTestStore);
    ITestStoreFactory GetTestStoreFactory();
    ListLoggerFactory ListLoggerFactory { get; }
    DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder);
}
