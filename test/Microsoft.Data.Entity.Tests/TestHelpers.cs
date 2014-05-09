// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Tests
{
    public static class TestHelpers
    {
        public static ImmutableDbContextOptions CreateEntityConfiguration(IModel model)
        {
            return new DbContextOptions()
                .UseModel(model)
                .BuildConfiguration();
        }

        public static ImmutableDbContextOptions CreateEntityConfiguration()
        {
            return new DbContextOptions()
                .BuildConfiguration();
        }

        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddInMemoryStore();
            return services.BuildServiceProvider();
        }

        public static DbContextConfiguration CreateContextConfiguration(IServiceProvider serviceProvider, IModel model)
        {
            return new DbContext(serviceProvider, CreateEntityConfiguration(model)).Configuration;
        }

        public static DbContextConfiguration CreateContextConfiguration(IServiceProvider serviceProvider)
        {
            return new DbContext(serviceProvider, CreateEntityConfiguration()).Configuration;
        }

        public static DbContextConfiguration CreateContextConfiguration(IModel model)
        {
            return new DbContext(CreateServiceProvider(), CreateEntityConfiguration(model)).Configuration;
        }

        public static DbContextConfiguration CreateContextConfiguration()
        {
            return new DbContext(CreateServiceProvider(), CreateEntityConfiguration()).Configuration;
        }
    }
}
