// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SequentialGuidIdentityGeneratorTest
    {
        [Fact]
        public async Task Can_get_next_values()
        {
            var sequentialGuidIdentityGenerator = new SequentialGuidIdentityGenerator();
            var values = new List<Guid>();

            for (var _ = 0; _ < 100; _++)
            {
                values.Add(await sequentialGuidIdentityGenerator.NextAsync(CancellationToken.None));
            }

            using (var testDatabase = await TestDatabase.Create())
            {
                await testDatabase.ExecuteNonQueryAsync("CREATE TABLE SequentialGuidTest (value uniqueidentifier)");

                for (var i = values.Count - 1; i >= 0; i--)
                {
                    await testDatabase.ExecuteNonQueryAsync("INSERT SequentialGuidTest VALUES (@p0)", values[i]);
                }

                Assert.Equal(
                    values,
                    await testDatabase.QueryAsync<Guid>("SELECT value FROM SequentialGuidTest ORDER BY value"));
            }
        }
    }
}
