// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class InMemoryTestHelpers : TestHelpers
{
    protected InMemoryTestHelpers()
    {
    }

    public static InMemoryTestHelpers Instance { get; } = new();

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkInMemoryDatabase();

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseInMemoryDatabase(nameof(InMemoryTestHelpers));

    public override LoggingDefinitions LoggingDefinitions { get; } = new InMemoryLoggingDefinitions();
}
