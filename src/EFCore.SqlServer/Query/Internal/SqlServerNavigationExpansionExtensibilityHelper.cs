// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerNavigationExpansionExtensibilityHelper : NavigationExpansionExtensibilityHelper
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerNavigationExpansionExtensibilityHelper(NavigationExpansionExtensibilityHelperDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override EntityQueryRootExpression CreateQueryRoot(IEntityType entityType, EntityQueryRootExpression? source)
    {
        if (source is TemporalAsOfQueryRootExpression asOf)
        {
            // AsOf is the only temporal operation that can pass the validation
            return source.QueryProvider != null
                ? new TemporalAsOfQueryRootExpression(source.QueryProvider, entityType, asOf.PointInTime)
                : new TemporalAsOfQueryRootExpression(entityType, asOf.PointInTime);
        }

        return base.CreateQueryRoot(entityType, source);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void ValidateQueryRootCreation(IEntityType entityType, EntityQueryRootExpression? source)
    {
        if (source is TemporalQueryRootExpression
            && !entityType.IsMappedToJson()
            && !OwnedEntityMappedToSameTableAsOwner(entityType))
        {
            if (!entityType.GetRootType().IsTemporal())
            {
                throw new InvalidOperationException(
                    SqlServerStrings.TemporalNavigationExpansionBetweenTemporalAndNonTemporal(entityType.DisplayName()));
            }

            if (source is not TemporalAsOfQueryRootExpression)
            {
                throw new InvalidOperationException(
                    SqlServerStrings.TemporalNavigationExpansionOnlySupportedForAsOf("AsOf"));
            }
        }

        base.ValidateQueryRootCreation(entityType, source);
    }

    private bool OwnedEntityMappedToSameTableAsOwner(IEntityType entityType)
        => entityType.IsOwned()
            && entityType.FindOwnership()!.PrincipalEntityType.GetTableMappings().FirstOrDefault()?.Table is ITable ownerTable
                && entityType.GetTableMappings().FirstOrDefault()?.Table is ITable entityTable
                && ownerTable == entityTable;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool AreQueryRootsCompatible(EntityQueryRootExpression? first, EntityQueryRootExpression? second)
    {
        if (!base.AreQueryRootsCompatible(first, second))
        {
            return false;
        }

        var firstTemporal = first is TemporalQueryRootExpression;
        var secondTemporal = second is TemporalQueryRootExpression;

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
