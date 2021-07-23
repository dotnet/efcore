// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class LoggingInMemoryTest : LoggingTestBase
    {
        protected override DbContextOptionsBuilder CreateOptionsBuilder(IServiceCollection services)
            => new DbContextOptionsBuilder()
                .UseInMemoryDatabase("LoggingInMemoryTest")
                .UseInternalServiceProvider(services.AddEntityFrameworkInMemoryDatabase().BuildServiceProvider());

        protected override string ProviderName
            => "Microsoft.EntityFrameworkCore.InMemory";

        protected override string DefaultOptions
            => "StoreName=LoggingInMemoryTest ";
    }
}
