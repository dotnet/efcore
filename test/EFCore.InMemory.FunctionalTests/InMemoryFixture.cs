// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryFixture
    {
        public readonly IServiceProvider ServiceProvider;

        public InMemoryFixture()
        {
            ServiceProvider = InMemoryTestStoreFactory.Instance.AddProviderServices(new ServiceCollection())
                .BuildServiceProvider();
        }
    }
}
