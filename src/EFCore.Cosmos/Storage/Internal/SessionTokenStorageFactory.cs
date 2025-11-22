// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SessionTokenStorageFactory : ISessionTokenStorageFactory
{
    private HashSet<string>? _containerNames;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ISessionTokenStorage Create(DbContext dbContext)
    {
        var options = dbContext.GetService<IDbContextOptions>().FindExtension<CosmosOptionsExtension>();
        Debug.Assert(options != null, "CosmosOptionsExtension is not found");

        var defaultContainerName = (string)dbContext.Model.GetAnnotation(CosmosAnnotationNames.ContainerName).Value!;
        _containerNames ??= new HashSet<string>(GetContainerNames(dbContext.Model, defaultContainerName));

        return new SessionTokenStorage(
            defaultContainerName,
            _containerNames,
            options.SessionTokenManagementMode);
    }

    private static IEnumerable<string> GetContainerNames(IModel model, string defaultContainerName)
        => new[] { defaultContainerName }.Concat(model.GetEntityTypes()
            .Where(et => et.FindPrimaryKey() != null)
            .Select(et => et.GetContainer())
            .Where(container => container != null))!;
}
