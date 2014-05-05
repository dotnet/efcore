// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
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

            using (var testDatabase = await TestDatabase.Default())
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
