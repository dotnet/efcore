// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Update
{
    public class SqlServerModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_returns_false_when_max_batch_size_is_reached()
        {
            var batch = new SqlServerModificationCommandBatch(
                new SqlServerUpdateSqlGenerator(),
                new RelationalCommandBuilderFactory(
                    new LoggerFactory(),
                    new SqlServerTypeMapper()),
                1);

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new UntypedRelationalValueBufferFactoryFactory())));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new UntypedRelationalValueBufferFactoryFactory())));
        }
    }
}
