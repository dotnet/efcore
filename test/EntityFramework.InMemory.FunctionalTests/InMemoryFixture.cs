// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemoryFixture
    {
        public readonly IServiceProvider ServiceProvider;

        public InMemoryFixture()
        {
            ServiceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddInMemoryStore()
                    .ServiceCollection()
                    .BuildServiceProvider();
        }
    }
}
