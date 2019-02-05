// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryFixture
    {
        public static IServiceProvider DefaultServiceProvider { get; }
            = BuildServiceProvider();

        public static IServiceProvider DefaultSensitiveServiceProvider { get; }
            = BuildServiceProvider();

        public readonly IServiceProvider ServiceProvider;

        public InMemoryFixture()
        {
            ServiceProvider = BuildServiceProvider();
        }

        public static ServiceProvider BuildServiceProvider(ILoggerFactory loggerFactory)
            => BuildServiceProvider(new ServiceCollection().AddSingleton(loggerFactory));

        public static ServiceProvider BuildServiceProvider(IServiceCollection providerServices = null)
            => InMemoryTestStoreFactory.Instance.AddProviderServices(
                    providerServices
                    ?? new ServiceCollection())
                .BuildServiceProvider();
    }
}
