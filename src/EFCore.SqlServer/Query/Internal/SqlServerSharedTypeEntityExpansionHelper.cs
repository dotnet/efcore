// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public class SqlServerSharedTypeEntityExpansionHelper : RelationalSharedTypeEntityExpansionHelper
#pragma warning restore EF1001 // Internal EF Core API usage.
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public SqlServerSharedTypeEntityExpansionHelper(RelationalSharedTypeEntityExpansionHelperDependencies dependencies)
#pragma warning disable EF1001 // Internal EF Core API usage.
            : base(dependencies)
#pragma warning restore EF1001 // Internal EF Core API usage.
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public override TableExpressionBase CreateRelatedTableExpression(
            TableExpressionBase sourceTable,
            IEntityType targetEntityType)
        {
            if (sourceTable is TemporalAsOfTableExpression temporalAsOf)
            {
                var table = targetEntityType.GetTableMappings().Single().Table;

                return new TemporalAsOfTableExpression(table, temporalAsOf.PointInTime);
            }

            if (sourceTable is TemporalTableExpression)
            {
                throw new InvalidOperationException(
                    SqlServerStrings.TemporalOwnedTypeMappedToDifferentTableOnlySupportedForAsOf("AsOf"));
            }

#pragma warning disable EF1001 // Internal EF Core API usage.
            return base.CreateRelatedTableExpression(sourceTable, targetEntityType);
#pragma warning restore EF1001 // Internal EF Core API usage.
        }
    }
}
