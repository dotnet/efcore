// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class NavigationExpansionExtensibilityHelper : INavigationExpansionExtensibilityHelper
{
    /// <summary>
    ///     Creates a new instance of the <see cref="NavigationExpansionExtensibilityHelper" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    public NavigationExpansionExtensibilityHelper(NavigationExpansionExtensibilityHelperDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual NavigationExpansionExtensibilityHelperDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual QueryRootExpression CreateQueryRoot(IEntityType entityType, QueryRootExpression? source)
        => source?.QueryProvider != null
            ? new QueryRootExpression(source.QueryProvider, entityType)
            : new QueryRootExpression(entityType);

    /// <inheritdoc />
    public virtual void ValidateQueryRootCreation(IEntityType entityType, QueryRootExpression? source)
    {
    }

    /// <inheritdoc />
    public virtual bool AreQueryRootsCompatible(QueryRootExpression? first, QueryRootExpression? second)
    {
        if (first is null && second is null)
        {
            return true;
        }

        if (first is not null && second is not null)
        {
            return first.EntityType.GetRootType() == second.EntityType.GetRootType();
        }

        return false;
    }
}
