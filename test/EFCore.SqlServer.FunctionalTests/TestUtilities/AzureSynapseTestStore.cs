// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable IDE0022 // Use block body for methods
// ReSharper disable SuggestBaseTypeForParameter
namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class AzureSynapseTestStore : SqlServerTestStore
{
    public new static async Task<AzureSynapseTestStore> GetNorthwindStoreAsync()
        => (AzureSynapseTestStore)await SqlServerNorthwindTestStoreFactory.Instance
            .GetOrCreate(SqlServerNorthwindTestStoreFactory.Name).InitializeAsync(null, (Func<DbContext>?)null);

    public new static AzureSynapseTestStore GetOrCreate(string name)
        => new(name);

    public new static async Task<AzureSynapseTestStore> GetOrCreateInitializedAsync(string name)
        => (AzureSynapseTestStore)await new AzureSynapseTestStore(name).InitializeSqlServerAsync(null, (Func<DbContext>?)null, null);

    public new static AzureSynapseTestStore GetOrCreateWithInitScript(string name, string initScript)
        => new(name, initScript: initScript);

    public new static AzureSynapseTestStore GetOrCreateWithScriptPath(
        string name,
        string scriptPath,
        bool? multipleActiveResultSets = null,
        bool shared = true)
        => new(name, scriptPath: scriptPath, multipleActiveResultSets: multipleActiveResultSets, shared: shared);

    public new static AzureSynapseTestStore Create(string name, bool useFileName = false, bool? multipleActiveResultSets = null)
        => new(name, useFileName, shared: false, multipleActiveResultSets: multipleActiveResultSets);

    public new static async Task<AzureSynapseTestStore> CreateInitializedAsync(
        string name,
        bool useFileName = false,
        bool? multipleActiveResultSets = null)
        => (AzureSynapseTestStore)await new AzureSynapseTestStore(name, useFileName, shared: false, multipleActiveResultSets: multipleActiveResultSets)
            .InitializeSqlServerAsync(null, (Func<DbContext>?)null, null);

    protected AzureSynapseTestStore(string name, bool useFileName = false, bool? multipleActiveResultSets = null, string? initScript = null, string? scriptPath = null, bool shared = true)
        : base(name, useFileName, multipleActiveResultSets, initScript, scriptPath, shared)
    {
    }

    public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
        => (UseConnectionString
            ? builder.UseAzureSynapse(ConnectionString, b => b.ApplyConfiguration())
            : builder.UseAzureSynapse(Connection, b => b.ApplyConfiguration()))
            .ConfigureWarnings(b => b.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
}
