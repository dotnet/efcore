// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <inheritdoc/>
    public class SqlServerSharedTypeEntityExpansionHelper : RelationalSharedTypeEntityExpansionHelper
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerSharedTypeEntityExpansionHelper(RelationalSharedTypeEntityExpansionHelperDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc/>
        public override SelectExpression CreateInnerSelectExpression(
            TableExpressionBase sourceTable,
            IEntityType targetEntityType)
        {
            if (sourceTable is TemporalAsOfTableExpression temporalAsOf)
            {
                var table = targetEntityType.GetTableMappings().Single().Table;
                var temporalTableExpression = new TemporalAsOfTableExpression(table, temporalAsOf.PointInTime);

                return Dependencies.SqlExpressionFactory.Select(targetEntityType, temporalTableExpression);
            }

            if (sourceTable is TemporalTableExpression)
            {
                throw new InvalidOperationException(
                    SqlServerStrings.TemporalOwnedTypeMappedToDifferentTableOnlySupportedForAsOf("AsOf"));
            }

            return base.CreateInnerSelectExpression(sourceTable, targetEntityType);
        }

        /// <inheritdoc/>
        public override bool TableMatchesMetadata(TableExpressionBase tableExpression, ITableBase tableMetadata)
            => base.TableMatchesMetadata(tableExpression, tableMetadata)
                || (tableExpression is TemporalTableExpression table
                    && table.Name == tableMetadata.Name
                    && table.Schema == tableMetadata.Schema);
    }
}
