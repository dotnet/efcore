// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.FunctionalTests;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Tests
{
    public class InMemoryServiceCollectionExtensionsTest : EntityFrameworkServiceCollectionExtensionsTest
    {
        public override void Services_wire_up_correctly()
        {
            base.Services_wire_up_correctly();

            // In memory dingletones
            VerifySingleton<InMemoryValueGeneratorCache>();
            VerifySingleton<IInMemoryStore>();
            VerifySingleton<InMemoryModelSource>();

            // In memory scoped
            VerifyScoped<InMemoryValueGeneratorSelector>();
            VerifyScoped<InMemoryDatabaseProviderServices>();
            VerifyScoped<IInMemoryDatabase>();
            VerifyScoped<InMemoryDatabaseCreator>();
            VerifyScoped<InMemoryQueryContextFactory>();

        }

        public InMemoryServiceCollectionExtensionsTest()
            : base(InMemoryTestHelpers.Instance)
        {
        }
    }
}
