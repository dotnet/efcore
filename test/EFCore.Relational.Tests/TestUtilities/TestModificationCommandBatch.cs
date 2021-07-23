// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestModificationCommandBatch : SingularModificationCommandBatch
    {
        private readonly int _maxBatchSize;

        public TestModificationCommandBatch(
            ModificationCommandBatchFactoryDependencies dependencies,
            int? maxBatchSize)
            : base(dependencies)
        {
            _maxBatchSize = maxBatchSize ?? 1;
        }

        protected override bool CanAddCommand(IModificationCommand modificationCommand)
        {
            return ModificationCommands.Count < _maxBatchSize;
        }
    }
}
