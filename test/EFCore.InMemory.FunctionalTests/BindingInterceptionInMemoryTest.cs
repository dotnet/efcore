// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class BindingInterceptionInMemoryTest : BindingInterceptionTestBase,
    IClassFixture<BindingInterceptionInMemoryTest.BindingInterceptionInMemoryFixture>
{
    public BindingInterceptionInMemoryTest(BindingInterceptionInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class BindingInterceptionInMemoryFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "BindingInterception";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkInMemoryDatabase(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
}
