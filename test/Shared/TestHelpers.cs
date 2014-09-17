// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Tests
{
    public static class TestHelpers
    {
        public static DbContextOptions CreateOptions(IModel model)
        {
            return new DbContextOptions().UseModel(model).UseProviderOptions();
        }

        public static DbContextOptions CreateOptions()
        {
            return new DbContextOptions();
        }

        public static IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddEntityFramework().AddProviderServices();
            return services.BuildServiceProvider();
        }

        public static DbContextConfiguration CreateContextConfiguration(IServiceProvider serviceProvider, IModel model)
        {
            return new DbContext(serviceProvider, CreateOptions(model)).Configuration;
        }

        public static DbContextConfiguration CreateContextConfiguration(IServiceProvider serviceProvider)
        {
            return new DbContext(serviceProvider, CreateOptions()).Configuration;
        }

        public static DbContextConfiguration CreateContextConfiguration(IModel model)
        {
            return new DbContext(CreateServiceProvider(), CreateOptions(model)).Configuration;
        }

        public static DbContextConfiguration CreateContextConfiguration()
        {
            return new DbContext(CreateServiceProvider(), CreateOptions()).Configuration;
        }

        public static Model BuildModelFor<TEntity>()
        {
            var builder = new ModelBuilder();
            builder.Entity<TEntity>();
            return builder.Model;
        }

        public static StateEntry CreateStateEntry<TEntity>(IModel model, EntityState entityState = EntityState.Unknown)
            where TEntity : new()
        {
            var entry = CreateContextConfiguration(model)
                .Services
                .StateEntryFactory
                .Create(model.GetEntityType(typeof(TEntity)), new TEntity());

            entry.EntityState = entityState;

            return entry;
        }
    }
}
