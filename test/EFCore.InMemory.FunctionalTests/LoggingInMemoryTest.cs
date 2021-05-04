// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
