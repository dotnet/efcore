// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SequenceIdentityGeneratorTest
    {
        [Fact]
        public async Task CanGetNextValues()
        {
            using (var testDatabase = await new TestDatabase().Create())
            {
                var sequenceIdentityGenerator
                    = new SequenceIdentityGenerator(testDatabase.Transaction);

                await testDatabase.ExecuteAsync(
                    SqlServerMigrationOperationSqlGenerator.Generate(sequenceIdentityGenerator.CreateMigrationOperation()));

                var next = sequenceIdentityGenerator.NextAsync().Result;

                for (var i = 1; i <= 100; i++)
                {
                    Assert.Equal(next + i, await sequenceIdentityGenerator.NextAsync());
                }
            }
        }
    }
}
