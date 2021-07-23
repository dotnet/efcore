﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerQueryRootCreator : QueryRootCreator
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerQueryRootCreator(QueryRootCreatorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override QueryRootExpression CreateQueryRoot(IEntityType entityType, QueryRootExpression? source)
        {
            if (source is TemporalAsOfQueryRootExpression
                || source is TemporalRangeQueryRootExpression
                || source is TemporalAllQueryRootExpression)
            {
                if (!entityType.GetRootType().IsTemporal())
                {
                    throw new InvalidOperationException(SqlServerStrings.TemporalNavigationExpansionBetweenTemporalAndNonTemporal(entityType.DisplayName()));
                }

                if (source is TemporalAsOfQueryRootExpression asOf)
                {
                    return source.QueryProvider != null
                        ? new TemporalAsOfQueryRootExpression(source.QueryProvider, entityType, asOf.PointInTime)
                        : new TemporalAsOfQueryRootExpression(entityType, asOf.PointInTime);
                }

                throw new InvalidOperationException(SqlServerStrings.TemporalNavigationExpansionOnlySupportedForAsOf(nameof(TemporalOperationType.AsOf)));
            }

            return base.CreateQueryRoot(entityType, source);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool AreCompatible(QueryRootExpression? first, QueryRootExpression? second)
        {
            if (!base.AreCompatible(first, second))
            {
                return false;
            }

            var firstTemporal = first is TemporalAsOfQueryRootExpression
                || first is TemporalRangeQueryRootExpression
                || first is TemporalAllQueryRootExpression;

            var secondTemporal = second is TemporalAsOfQueryRootExpression
                || second is TemporalRangeQueryRootExpression
                || second is TemporalAllQueryRootExpression;

            if (firstTemporal && secondTemporal)
            {
                if (first is TemporalAsOfQueryRootExpression firstAsOf
                    && second is TemporalAsOfQueryRootExpression secondAsOf
                    && firstAsOf.PointInTime == secondAsOf.PointInTime)
                {
                    return true;
                }

                if (first is TemporalAllQueryRootExpression
                    && second is TemporalAllQueryRootExpression)
                {
                    return true;
                }

                if (first is TemporalRangeQueryRootExpression firstRange
                    && second is TemporalRangeQueryRootExpression secondRange
                    && firstRange.From == secondRange.From
                    && firstRange.To == secondRange.To)
                {
                    return true;
                }
            }

            if (firstTemporal || secondTemporal)
            {
                var entityType = first?.EntityType ?? second?.EntityType;

                throw new InvalidOperationException(SqlServerStrings.TemporalSetOperationOnMismatchedSources(entityType!.DisplayName()));
            }
            
            return true;
        }
    }
}
