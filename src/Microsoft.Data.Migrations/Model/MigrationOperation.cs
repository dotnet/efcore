// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Migrations.Model
{
    public abstract class MigrationOperation
    {
        public virtual bool IsDestructiveChange
        {
            get { return false; }
        }

        public abstract void GenerateSql([NotNull] MigrationOperationSqlGenerator visitor, [NotNull] IndentedStringBuilder stringBuilder, bool generateIdempotentSql);
    }
}
