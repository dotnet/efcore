// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisFixture
    {
        public readonly IServiceProvider ServiceProvider;

        public RedisFixture()
        {
            ServiceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddRedis()
                    .ServiceCollection
                    .BuildServiceProvider();
        }
    }
}
