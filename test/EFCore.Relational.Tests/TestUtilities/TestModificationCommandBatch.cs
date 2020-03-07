// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

        protected override bool CanAddCommand(ModificationCommand modificationCommand)
        {
            return ModificationCommands.Count < _maxBatchSize;
        }
    }
}
