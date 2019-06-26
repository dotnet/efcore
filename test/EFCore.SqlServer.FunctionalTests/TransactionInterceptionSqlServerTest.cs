// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class TransactionInterceptionSqlServerTestBase : TransactionInterceptionTestBase
    {
        protected TransactionInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
            : base(fixture)
        {
        }

        public abstract class InterceptionSqlServerFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName => "TransactionInterception";
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);
        }

        public class TransactionInterceptionSqlServerTest
            : TransactionInterceptionSqlServerTestBase, IClassFixture<TransactionInterceptionSqlServerTest.InterceptionSqlServerFixture>
        {
            public TransactionInterceptionSqlServerTest(InterceptionSqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => false;
            }
        }

        public class TransactionInterceptionWithDiagnosticsSqlServerTest
            : TransactionInterceptionSqlServerTestBase, IClassFixture<TransactionInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
        {
            public TransactionInterceptionWithDiagnosticsSqlServerTest(InterceptionSqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
            {
                protected override bool ShouldSubscribeToDiagnosticListener => true;
            }
        }
    }
}
