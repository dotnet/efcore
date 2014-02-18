// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerSimpleCommandExecutorTest
    {
        [Fact]
        public async Task CanExecuteScalarCommand()
        {
            using (var testDatabase = await TestDatabase.Create())
            {
                var commandExecutor = new SqlServerSimpleCommandExecutor(testDatabase.Connection.ConnectionString);

                var scalar = await commandExecutor.ExecuteScalarAsync<int>("select 42");

                Assert.Equal(42, scalar);
            }
        }
    }
}
