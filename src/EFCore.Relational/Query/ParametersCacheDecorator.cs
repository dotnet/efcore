// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A decorator over <see cref="QueryContext.Parameters" /> which provides a cache-safe way to access parameters after the SQL cache.
/// </summary>
/// <remarks>
///     The SQL cache only includes then nullability of parameters in its cache key. Accordingly, this type exposes an API for checking
///     the nullability of a parameter. It also allows retrieving the full parameter dictionary for arbitrary checks, but when this
///     API is called, the decorator records this fact, and the resulting SQL will not get cached.
/// </remarks>
public sealed class ParametersCacheDecorator(Dictionary<string, object?> parameters)
{
    /// <summary>
    ///     Returns whether the parameter with the given name is <see langword="null" />.
    /// </summary>
    /// <remarks>
    ///     The method assumes that the parameter with the given name exists in the dictionary,
    ///     and otherwise throws <see cref="UnreachableException" />.
    /// </remarks>
    public bool IsNull(string parameterName)
        => parameters.TryGetValue(parameterName, out var value)
            ? value is null
            : throw new UnreachableException($"Parameter with name '{parameterName}' does not exist.");

    /// <summary>
    ///     Returns the full dictionary of parameters, and disables caching for the generated SQL.
    /// </summary>
    public Dictionary<string, object?> GetAndDisableCaching()
    {
        CanCache = false;

        return parameters;
    }

    /// <summary>
    ///     Whether the SQL generated using this decorator can be cached, i.e. whether the full dictionary of parameters
    ///     has been accessed.
    /// </summary>
    /// <remarks>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </remarks>
    [EntityFrameworkInternal]
    public bool CanCache { get; private set; } = true;
}
