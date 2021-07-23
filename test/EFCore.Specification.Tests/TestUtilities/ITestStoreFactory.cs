// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public interface ITestStoreFactory
    {
        TestStore Create(string storeName);
        TestStore GetOrCreate(string storeName);
        IServiceCollection AddProviderServices(IServiceCollection serviceCollection);
        ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory);
    }
}
