// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class RelationalTestStoreFactory : ITestStoreFactory
{
    public abstract TestStore Create(string storeName);
    public abstract TestStore GetOrCreate(string storeName);
    public abstract IServiceCollection AddProviderServices(IServiceCollection serviceCollection);

    public virtual ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
        => new TestSqlLoggerFactory(shouldLogCategory);
}
