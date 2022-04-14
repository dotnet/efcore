// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestModificationCommandBatch : SingularModificationCommandBatch
{
    public TestModificationCommandBatch(
        ModificationCommandBatchFactoryDependencies dependencies,
        int? maxBatchSize)
        : base(dependencies)
        => MaxBatchSize = maxBatchSize ?? 1;

    protected override int MaxBatchSize { get; }
}
