// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    public class SequenceIdentityGeneratorTest
    {
        [Fact]
        public async Task Can_get_next_values()
        {
            using (var testDatabase = await TestDatabase.Create())
            {
                var sequenceIdentityGenerator
                    = new SequenceIdentityGenerator(testDatabase);

                await testDatabase.ExecuteNonQueryAsync(
                    SqlServerMigrationOperationSqlGenerator.Generate(sequenceIdentityGenerator.CreateMigrationOperation()));

                var next = sequenceIdentityGenerator.NextAsync(CancellationToken.None).Result;

                for (var i = 1; i <= 100; i++)
                {
                    Assert.Equal(next + i, await sequenceIdentityGenerator.NextAsync(CancellationToken.None));
                }
            }
        }
    }
}
