// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class AzureSynapseTestStoreFactory : RelationalTestStoreFactory
{
    public static AzureSynapseTestStoreFactory Instance { get; } = new();

    protected AzureSynapseTestStoreFactory()
    {
    }

    public override TestStore Create(string storeName)
        => AzureSynapseTestStore.Create(storeName);

    public override TestStore GetOrCreate(string storeName)
        => AzureSynapseTestStore.GetOrCreate(storeName);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection
            .AddEntityFrameworkAzureSynapse();
}
