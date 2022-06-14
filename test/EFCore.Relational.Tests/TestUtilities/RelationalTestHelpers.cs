// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class RelationalTestHelpers : TestHelpers
{
    protected RelationalTestHelpers()
    {
    }

    public static RelationalTestHelpers Instance { get; } = new();

    protected override EntityFrameworkDesignServicesBuilder CreateEntityFrameworkDesignServicesBuilder(
        IServiceCollection services)
        => new EntityFrameworkRelationalDesignServicesBuilder(services);

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => FakeRelationalOptionsExtension.AddEntityFrameworkRelationalDatabase(services);

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseFakeRelational();

    public override LoggingDefinitions LoggingDefinitions { get; } = new TestRelationalLoggingDefinitions();
}
