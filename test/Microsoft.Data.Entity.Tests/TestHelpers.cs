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
        public static EntityConfiguration CreateEntityConfiguration(IServiceProvider provider, IModel model)
        {
            return new EntityConfigurationBuilder(provider)
                .UseModel(model)
                .BuildConfiguration();
        }

        public static EntityConfiguration CreateEntityConfiguration(IServiceProvider provider)
        {
            return new EntityConfigurationBuilder(provider)
                .BuildConfiguration();
        }

        public static EntityConfiguration CreateEntityConfiguration(IModel model)
        {
            return CreateEntityConfiguration(
                new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .BuildServiceProvider(),
                model);
        }

        public static EntityConfiguration CreateEntityConfiguration()
        {
            return CreateEntityConfiguration(
                new ServiceCollection()
                    .AddEntityFramework(s => s.AddInMemoryStore())
                    .BuildServiceProvider());
        }

        public static ContextConfiguration CreateContextConfiguration(IServiceProvider provider, IModel model)
        {
            return new EntityContext(CreateEntityConfiguration(provider, model)).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration(IServiceProvider provider)
        {
            return new EntityContext(CreateEntityConfiguration(provider)).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration(IModel model)
        {
            return new EntityContext(CreateEntityConfiguration(model)).Configuration;
        }

        public static ContextConfiguration CreateContextConfiguration()
        {
            return new EntityContext(CreateEntityConfiguration()).Configuration;
        }
    }
}
