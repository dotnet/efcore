// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestModificationCommandBatchFactory(
    ModificationCommandBatchFactoryDependencies dependencies,
    IDbContextOptions options) : IModificationCommandBatchFactory
{
    private readonly ModificationCommandBatchFactoryDependencies _dependencies = dependencies;
    private readonly IDbContextOptions _options = options;

    public int CreateCount { get; private set; }

    public virtual ModificationCommandBatch Create()
    {
        CreateCount++;

        var optionsExtension = _options.Extensions.OfType<FakeRelationalOptionsExtension>().FirstOrDefault();

        return new TestModificationCommandBatch(_dependencies, optionsExtension?.MaxBatchSize);
    }
}
