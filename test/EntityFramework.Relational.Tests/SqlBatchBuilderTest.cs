// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.Entity
{
    public class SqlBatchBuilderTest
    {
        [Fact]
        public void SqlBatchBuilder_correctly_groups_multiple_statements_into_one_batch()
        {
            var batchBuilder = CreateBuilder();
            batchBuilder.AppendLine("Statement1");
            batchBuilder.AppendLine("Statement2");
            batchBuilder.AppendLine("Statement3");
            batchBuilder.EndCommand();

            Assert.Equal(1, batchBuilder.GetCommands().Count);
            Assert.Equal(
                @"Statement1
Statement2
Statement3
", batchBuilder.GetCommands()[0].CommandText);
        }

        [Fact]
        public void SqlBatchBuilder_correctly_produces_multiple_batches()
        {
            var batchBuilder = CreateBuilder();
            batchBuilder.AppendLine("Statement1");
            batchBuilder.EndCommand();
            batchBuilder.AppendLine("Statement2");
            batchBuilder.AppendLine("Statement3");
            batchBuilder.EndCommand();
            batchBuilder.AppendLine("Statement4");
            batchBuilder.AppendLine("Statement5");
            batchBuilder.AppendLine("Statement6");
            batchBuilder.EndCommand();

            Assert.Equal(3, batchBuilder.GetCommands().Count);

            Assert.Equal(
                @"Statement1
", batchBuilder.GetCommands()[0].CommandText);

            Assert.Equal(
                @"Statement2
Statement3
", batchBuilder.GetCommands()[1].CommandText);

            Assert.Equal(
                @"Statement4
Statement5
Statement6
", batchBuilder.GetCommands()[2].CommandText);
        }

        [Fact]
        public void SqlBatchBuilder_ignores_empty_batches()
        {
            var batchBuilder = CreateBuilder();
            batchBuilder.AppendLine("Statement1");
            batchBuilder.EndCommand();
            batchBuilder.EndCommand();
            batchBuilder.EndCommand();
            batchBuilder.AppendLine("Statement2");
            batchBuilder.AppendLine("Statement3");
            batchBuilder.EndCommand();
            batchBuilder.EndCommand();

            Assert.Equal(2, batchBuilder.GetCommands().Count);

            Assert.Equal(
                @"Statement1
", batchBuilder.GetCommands()[0].CommandText);

            Assert.Equal(
                @"Statement2
Statement3
", batchBuilder.GetCommands()[1].CommandText);
        }

        private RelationalCommandListBuilder CreateBuilder()
            => new RelationalCommandListBuilder(
                new RelationalCommandBuilderFactory(
                    new TestRelationalTypeMapper()));

        private class TestRelationalTypeMapper : RelationalTypeMapper
        {
            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }
                = new Dictionary<Type, RelationalTypeMapping>
                    {
                        { typeof(int), new RelationalTypeMapping("int", DbType.String) }
                    };

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }
                = new Dictionary<string, RelationalTypeMapping>();

            protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;
        }
    }
}