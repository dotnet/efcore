// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public class BindingInterceptionSqlServerTest : BindingInterceptionTestBase,
    IClassFixture<BindingInterceptionSqlServerTest.BindingInterceptionSqlServerFixture>
{
    public BindingInterceptionSqlServerTest(BindingInterceptionSqlServerFixture fixture)
        : base(fixture)
    {
    }

    public class BindingInterceptionSqlServerFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "BindingInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));
            return builder;
        }
    }
}
