// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Text;
using Microsoft.Data.Relational;

namespace Microsoft.Data.SqlServer
{
    internal class SqlServerSqlGenerator : SqlGenerator
    {
        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            commandStringBuilder.Append("SET NOCOUNT OFF");
        }
    }
}
