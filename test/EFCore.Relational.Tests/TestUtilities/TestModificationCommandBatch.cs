// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestModificationCommandBatch : SingularModificationCommandBatch
    {
        private readonly int _maxBatchSize;

        public TestModificationCommandBatch(
            ModificationCommandBatchFactoryDependencies relationalDependencies,
            int? maxBatchSize)
            : base(relationalDependencies)
        {
            _maxBatchSize = maxBatchSize ?? 1;
        }

        protected override bool CanAddCommand(IReadOnlyModificationCommand modificationCommand)
        {
            return ModificationCommands.Count < _maxBatchSize;
        }
    }
}
