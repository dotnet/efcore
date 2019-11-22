// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Update;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestModificationCommandBatchFactory : IModificationCommandBatchFactory
    {
        private readonly ModificationCommandBatchFactoryDependencies _dependencies;
        private readonly IDbContextOptions _options;

        public TestModificationCommandBatchFactory(
            ModificationCommandBatchFactoryDependencies dependencies,
            IDbContextOptions options)
        {
            _dependencies = dependencies;
            _options = options;
        }

        public int CreateCount { get; private set; }

        public virtual ModificationCommandBatch Create()
        {
            CreateCount++;

            var optionsExtension = _options.Extensions.OfType<FakeRelationalOptionsExtension>().FirstOrDefault();

            return new TestModificationCommandBatch(_dependencies, optionsExtension?.MaxBatchSize);
        }
    }
}
