// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.AspNet.DependencyInjection.Fallback;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.InMemory;

namespace Microsoft.Data.Entity.Tests
{
    public static class TestHelpers
    {
        public static EntityConfiguration CreateEntityConfiguration(IModel model)
        {
            return new EntityConfigurationBuilder()
                .UseModel(model)
                .BuildConfiguration();
        }

        public static EntityConfiguration CreateEntityConfiguration()
        {
            return new EntityConfigurationBuilder()
                .BuildConfiguration();
        }

        public static IServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddEntityFramework(s => s.AddInMemoryStore())
                .BuildServiceProvider();
        }

        public static ContextConfiguration CreateContextConfiguration(IServiceProvider serviceProvider, IModel model)
        {
            return new DbContext(serviceProvider, CreateEntityConfiguration(model)).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration(IServiceProvider serviceProvider)
        {
            return new DbContext(serviceProvider, CreateEntityConfiguration()).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration(IModel model)
        {
            return new DbContext(CreateServiceProvider(), CreateEntityConfiguration(model)).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration()
        {
            return new DbContext(CreateServiceProvider(), CreateEntityConfiguration()).Configuration;
        }
    }
}
