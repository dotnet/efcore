// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Text;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerSqlGeneratorTest
    {
        [Fact]
        public void AppendBatchHeader_should_append_SET_NOCOUNT_OFF()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator().AppendBatchHeader(sb);

            Assert.Equal("SET NOCOUNT OFF", sb.ToString());
        }
    }
}
