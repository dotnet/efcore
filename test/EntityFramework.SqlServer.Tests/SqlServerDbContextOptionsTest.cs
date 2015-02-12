// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerDbContextOptionsTest
    {
        [Fact]
        public void Can_add_extension_with_max_batch_size()
        {
            var options = new DbContextOptions();
            options.UseSqlServer().MaxBatchSize(123);

            var extension = ((IDbContextOptions)options).Extensions.OfType<SqlServerOptionsExtension>().Single();

            Assert.Equal(123, extension.MaxBatchSize);
        }
    }
}
