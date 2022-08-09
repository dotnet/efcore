// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class DbSetInitializer : IDbSetInitializer
{
    private readonly IDbSetFinder _setFinder;
    private readonly IDbSetSource _setSource;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public DbSetInitializer(
        IDbSetFinder setFinder,
        IDbSetSource setSource)
    {
        _setFinder = setFinder;
        _setSource = setSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void InitializeSets(DbContext context)
    {
        foreach (var setInfo in _setFinder.FindSets(context.GetType()).Where(p => p.Setter != null))
        {
            setInfo.Setter!.SetClrValue(
                context,
                ((IDbSetCache)context).GetOrAddSet(_setSource, setInfo.Type));
        }
    }
}
