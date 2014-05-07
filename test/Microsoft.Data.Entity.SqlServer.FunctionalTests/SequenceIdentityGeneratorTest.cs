// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SequenceIdentityGeneratorTest
    {
        [Fact]
        public async Task Can_get_next_values()
        {
            using (var testDatabase = await TestDatabase.Default())
            {
                var sequenceIdentityGenerator
                    = new SequenceIdentityGenerator(testDatabase);

                var generator = new SqlServerMigrationOperationSqlGenerator(new SqlServerTypeMapper());

                await testDatabase.ExecuteNonQueryAsync(
                    generator.Generate(new[] { sequenceIdentityGenerator.CreateMigrationOperation() }, generateIdempotentSql: true).Single().Sql);

                var next = sequenceIdentityGenerator.NextAsync(CancellationToken.None).Result;

                for (var i = 1; i <= 100; i++)
                {
                    Assert.Equal(next + i, await sequenceIdentityGenerator.NextAsync(CancellationToken.None));
                }
            }
        }
    }
}
