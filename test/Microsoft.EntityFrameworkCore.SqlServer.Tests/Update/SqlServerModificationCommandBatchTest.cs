// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Update
{
    public class SqlServerModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_returns_false_when_max_batch_size_is_reached()
        {
            var batch = new SqlServerModificationCommandBatch(
                new RelationalCommandBuilderFactory(
                    new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                    new DiagnosticListener("Fake"),
                    new SqlServerTypeMapper()),
                new SqlServerSqlGenerationHelper(),
                new SqlServerUpdateSqlGenerator(new SqlServerSqlGenerationHelper(), new SqlServerTypeMapper()),
                new UntypedRelationalValueBufferFactoryFactory(),
                1);

            Assert.True(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, p => p.SqlServer())));
            Assert.False(batch.AddCommand(new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, p => p.SqlServer())));
        }
    }
}
