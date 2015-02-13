// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryEntityServicesBuilderExtensionsTest : EntityServiceCollectionExtensionsTest
    {
        [Fact]
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // In memory dingletones
            VerifySingleton<InMemoryModelBuilderFactory>();
            VerifySingleton<InMemoryValueGeneratorCache>();
            VerifySingleton<InMemoryValueGeneratorSelector>();
            VerifySingleton<SimpleValueGeneratorFactory<InMemoryValueGenerator>>();
            VerifySingleton<InMemoryDatabase>();
            VerifySingleton<InMemoryModelSource>();

            // In memory scoped
            VerifyScoped<InMemoryDataStoreServices>();
            VerifyScoped<InMemoryDatabaseFacade>();
            VerifyScoped<InMemoryDataStore>();
            VerifyScoped<InMemoryConnection>();
            VerifyScoped<InMemoryDataStoreCreator>();

            VerifyCommonDataStoreServices();
        }

        protected override IServiceCollection GetServices(IServiceCollection services = null)
        {
            return (services ?? new ServiceCollection())
                .AddEntityFramework()
                .AddInMemoryStore()
                .ServiceCollection();
        }

        protected override DbContext CreateContext(IServiceProvider serviceProvider)
        {
            return InMemoryTestHelpers.Instance.CreateContext(serviceProvider);
        }
    }
}
