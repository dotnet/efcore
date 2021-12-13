// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class NamedConnectionStringResolverBase
{
    private const string DefaultSection = "ConnectionStrings:";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected abstract IServiceProvider? ApplicationServiceProvider { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string ResolveConnectionString(string connectionString)
    {
        var connectionName = TryGetConnectionName(connectionString);

        if (connectionName == null)
        {
            return connectionString;
        }

        var configuration = ApplicationServiceProvider
            ?.GetService<IConfiguration>();

        var resolved = configuration?[connectionName]
            ?? configuration?[DefaultSection + connectionName];

        if (resolved == null)
        {
            throw new InvalidOperationException(
                RelationalStrings.NamedConnectionStringNotFound(connectionName));
        }

        return resolved;
    }

    private static string? TryGetConnectionName(string connectionString)
    {
        var firstEquals = connectionString.IndexOf('=');
        if (firstEquals < 0)
        {
            return null;
        }

        if (connectionString.IndexOf('=', firstEquals + 1) >= 0)
        {
            return null;
        }

        return connectionString[..firstEquals].Trim().Equals(
            "name", StringComparison.OrdinalIgnoreCase)
            ? connectionString[(firstEquals + 1)..].Trim()
            : null;
    }
}
