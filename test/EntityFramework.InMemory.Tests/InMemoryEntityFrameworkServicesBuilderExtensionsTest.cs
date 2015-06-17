// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.InMemory.Query;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Tests;

namespace Microsoft.Data.Entity.InMemory.Tests
{
    public class InMemoryEntityFrameworkServicesBuilderExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // In memory dingletones
            VerifySingleton<ModelBuilderFactory>();
            VerifySingleton<InMemoryValueGeneratorCache>();
            VerifySingleton<IInMemoryDatabase>();
            VerifySingleton<InMemoryModelSource>();

            // In memory scoped
            VerifyScoped<InMemoryValueGeneratorSelector>();
            VerifyScoped<InMemoryQueryContextFactory>();
            VerifyScoped<InMemoryDataStoreServices>();
            VerifyScoped<InMemoryDatabaseFactory>();
            VerifyScoped<IInMemoryDataStore>();
            VerifyScoped<InMemoryConnection>();
            VerifyScoped<InMemoryDataStoreCreator>();
        }

        protected override string AddProviderMethodName => "AddInMemoryStore";

        public InMemoryEntityFrameworkServicesBuilderExtensionsTest()
            : base(InMemoryTestHelpers.Instance)
        {
        }
    }
}
