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

using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.SqlServer.FunctionalTests
{
    public class SqlServerSimpleCommandExecutorTest
    {
        [Fact]
        public async Task Can_execute_scalar_command()
        {
            using (var testDatabase = await TestDatabase.Default())
            {
                var commandExecutor = new SqlServerSimpleCommandExecutor(testDatabase.Connection.ConnectionString);

                var scalar = await commandExecutor.ExecuteScalarAsync<int>("select 42", CancellationToken.None);

                Assert.Equal(42, scalar);
            }
        }
    }
}
