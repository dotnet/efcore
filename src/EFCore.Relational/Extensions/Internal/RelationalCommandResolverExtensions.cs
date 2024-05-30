// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class RelationalCommandResolverExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IRelationalCommand RentAndPopulateRelationalCommand(
        this RelationalCommandResolver relationalCommandResolver,
        RelationalQueryContext queryContext)
    {
        var relationalCommandTemplate = relationalCommandResolver(queryContext.ParameterValues);
        var relationalCommand = queryContext.Connection.RentCommand();
        relationalCommand.PopulateFrom(relationalCommandTemplate);
        return relationalCommand;
    }
}
