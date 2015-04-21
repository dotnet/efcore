// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.SqlServer.Update;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Update
{
    public class SqlServerModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_returns_false_when_max_batch_size_is_reached()
        {
            var batch = new SqlServerModificationCommandBatch(new SqlServerSqlGenerator(), 1);

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new BoxedValueReaderSource(), new SqlServerValueReaderFactoryFactory())));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.SqlServer(), new BoxedValueReaderSource(), new SqlServerValueReaderFactoryFactory())));
        }
    }
}
