// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore;

public class LoggingInMemoryTest : LoggingTestBase
{
    protected override DbContextOptionsBuilder CreateOptionsBuilder(IServiceCollection services)
        => new DbContextOptionsBuilder()
            .UseInMemoryDatabase("LoggingInMemoryTest")
            .UseInternalServiceProvider(services.AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true));

    protected override TestLogger CreateTestLogger()
        => new TestLogger<InMemoryLoggingDefinitions>();

    protected override string ProviderName
        => "Microsoft.EntityFrameworkCore.InMemory";

    protected override string ProviderVersion
        => typeof(InMemoryOptionsExtension).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    protected override string DefaultOptions
        => "StoreName=LoggingInMemoryTest ";
}
