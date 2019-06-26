// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class TransactionInterceptionSqliteTestBase : TransactionInterceptionTestBase
    {
        protected TransactionInterceptionSqliteTestBase(InterceptionSqliteFixtureBase fixture)
            : base(fixture)
        {
        }

        public abstract class InterceptionSqliteFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName => "TransactionInterception";
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
        }

        public class TransactionInterceptionSqliteTest
            : TransactionInterceptionSqliteTestBase, IClassFixture<TransactionInterceptionSqliteTest.InterceptionSqliteFixture>
        {
            public TransactionInterceptionSqliteTest(InterceptionSqliteFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => false;
            }
        }

        public class TransactionInterceptionWithDiagnosticsSqliteTest
            : TransactionInterceptionSqliteTestBase, IClassFixture<TransactionInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture>
        {
            public TransactionInterceptionWithDiagnosticsSqliteTest(InterceptionSqliteFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => true;
            }
        }
    }
}
