// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
                values.Add(await sequentialGuidIdentityGenerator.NextAsync());
            }

            using (var testDatabase = await new TestDatabase().Create())
            {
                await testDatabase.Execute("CREATE TABLE SequentialGuidTest (value uniqueidentifier)");

                for (var i = values.Count - 1; i >= 0; i--)
                {
                    await testDatabase.Execute("INSERT SequentialGuidTest VALUES (@p0)", values[i]);
                }

                Assert.Equal(
                    values,
                    await testDatabase.Query<Guid>("SELECT value FROM SequentialGuidTest ORDER BY value"));
            }
        }
    }
}
