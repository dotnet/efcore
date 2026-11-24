// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;
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
    private sealed record CachedInfo(string DefaultContainerName, HashSet<string> ContainerNames, SessionTokenManagementMode Mode);

    private readonly ConcurrentDictionary<Guid, CachedInfo> _cache = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ISessionTokenStorage Create(DbContext dbContext)
    {
#pragma warning disable EF1001 // Internal EF Core API usage.
        var modelId = dbContext.Model.ModelId;
#pragma warning restore EF1001 // Internal EF Core API usage.

        var info = _cache.GetOrAdd(
            modelId,
            _ =>
            {
                var defaultContainerName = (string)dbContext.Model.GetAnnotation(CosmosAnnotationNames.ContainerName).Value!;
                var containerNames = new HashSet<string>([.. GetContainerNames(dbContext.Model)]);
                var mode = dbContext.GetService<IDbContextOptions>().FindExtension<CosmosOptionsExtension>()!.SessionTokenManagementMode;
                return new CachedInfo(defaultContainerName, containerNames, mode);
            });

        return new SessionTokenStorage(
            info.DefaultContainerName,
            info.ContainerNames,
            info.Mode);
    }

    private static IEnumerable<string> GetContainerNames(IModel model)
        => model.GetEntityTypes()
            .Where(et => et.FindPrimaryKey() != null)
            .Select(et => et.GetContainer())
            .Where(container => container != null)!;
}
