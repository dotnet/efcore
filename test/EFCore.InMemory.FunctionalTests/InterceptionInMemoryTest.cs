// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class SaveChangesInterceptionInMemoryTestBase : SaveChangesInterceptionTestBase
    {
        protected SaveChangesInterceptionInMemoryTestBase(InterceptionInMemoryFixtureBase fixture)
            : base(fixture)
        {
        }

        protected override bool SupportsOptimisticConcurrency
            => false;

        public abstract class InterceptionInMemoryFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName
                => "SaveChangesInterception";

            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkInMemoryDatabase(), injectedInterceptors);

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(c => c.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        }

        public class SaveChangesInterceptionInMemoryTest
            : SaveChangesInterceptionInMemoryTestBase, IClassFixture<SaveChangesInterceptionInMemoryTest.InterceptionInMemoryFixture>
        {
            public SaveChangesInterceptionInMemoryTest(InterceptionInMemoryFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionInMemoryFixture : InterceptionInMemoryFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener
                    => false;
            }
        }

        public class SaveChangesInterceptionWithDiagnosticsInMemoryTest
            : SaveChangesInterceptionInMemoryTestBase,
                IClassFixture<SaveChangesInterceptionWithDiagnosticsInMemoryTest.InterceptionInMemoryFixture>
        {
            public SaveChangesInterceptionWithDiagnosticsInMemoryTest(InterceptionInMemoryFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionInMemoryFixture : InterceptionInMemoryFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener
                    => true;
            }
        }
    }
}
