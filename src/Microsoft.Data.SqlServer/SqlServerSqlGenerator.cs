// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.SqlServer
{
    internal class SqlServerSqlGenerator : SqlGenerator
    {
        public override void AppendBatchHeader(StringBuilder commandStringBuilder)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");

            commandStringBuilder.Append("SET NOCOUNT OFF");
        }

        public override void AppendModificationOperationSelectWhereClause(StringBuilder commandStringBuilder,
            IEnumerable<KeyValuePair<string, string>> knownKeyValues,
            IEnumerable<KeyValuePair<string, ValueGenerationStrategy>> generatedKeys)
        {
            Check.NotNull(commandStringBuilder, "commandStringBuilder");
            Check.NotNull(knownKeyValues, "knownKeyValues");
            Check.NotNull(generatedKeys, "generatedKeys");

            AppendWhereClause(
                commandStringBuilder,
                knownKeyValues.Concat(
                    generatedKeys.Where(k => k.Value == ValueGenerationStrategy.StoreIdentity)
                        .Select(k => new KeyValuePair<string, string>(k.Key, "scope_identity()"))));
        }
    }
}
