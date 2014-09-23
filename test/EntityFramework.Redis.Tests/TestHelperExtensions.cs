// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.DependencyInjection;
using Microsoft.Data.Entity.Redis.Extensions;

namespace Microsoft.Data.Entity.Tests
{
    public static class TestHelperExtensions
    {
        public static EntityServicesBuilder AddProviderServices(this EntityServicesBuilder entityServicesBuilder)
        {
            return entityServicesBuilder.AddRedis();
        }

        public static DbContextOptions UseProviderOptions(this DbContextOptions options)
        {
            return options.UseRedis("127.0.0.1", 6375);
        }
    }
}
