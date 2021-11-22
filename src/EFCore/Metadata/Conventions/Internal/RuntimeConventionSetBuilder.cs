// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RuntimeConventionSetBuilder : IConventionSetBuilder
{
    private readonly IProviderConventionSetBuilder _conventionSetBuilder;
    private readonly IList<IConventionSetPlugin> _plugins;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RuntimeConventionSetBuilder(
        IProviderConventionSetBuilder providerConventionSetBuilder,
        IEnumerable<IConventionSetPlugin> plugins)
    {
        _conventionSetBuilder = providerConventionSetBuilder;
        _plugins = plugins.ToList();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ConventionSet CreateConventionSet()
    {
        var conventionSet = _conventionSetBuilder.CreateConventionSet();

        foreach (var plugin in _plugins)
        {
            conventionSet = plugin.ModifyConventions(conventionSet);
        }

        return conventionSet;
    }
}
