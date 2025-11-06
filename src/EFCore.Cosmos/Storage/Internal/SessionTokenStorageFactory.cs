// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    private readonly string _defaultContainerName;
    private readonly HashSet<string> _containerNames;
    private readonly SessionTokenManagementMode _mode;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SessionTokenStorageFactory(ICurrentDbContext currentDbContext, ICosmosSingletonOptions options)
    {
        _defaultContainerName = (string)currentDbContext.Context.Model.GetAnnotation(CosmosAnnotationNames.ContainerName).Value!;
        _containerNames = new HashSet<string>([_defaultContainerName, ..GetContainerNames(currentDbContext.Context.Model)]);
        _mode = options.SessionTokenManagementMode;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ISessionTokenStorage Create()
        => _mode == SessionTokenManagementMode.FullyAutomatic ?
                        new NullSessionTokenStorage() :
                        new SessionTokenStorage(_defaultContainerName, _containerNames, _mode);


    private static IEnumerable<string> GetContainerNames(IModel model)
        => model.GetEntityTypes()
            .Where(et => et.FindPrimaryKey() != null)
            .Select(et => et.GetContainer())
            .Where(container => container != null)!;
}
