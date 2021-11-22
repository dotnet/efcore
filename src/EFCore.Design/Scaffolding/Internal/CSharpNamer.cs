// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CSharpNamer<T>
    where T : notnull
{
    private readonly Func<T, string> _nameGetter;
    private readonly ICSharpUtilities _cSharpUtilities;
    private readonly Func<string, string>? _singularizePluralizer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected readonly Dictionary<T, string> NameCache = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CSharpNamer(
        Func<T, string> nameGetter,
        ICSharpUtilities cSharpUtilities,
        Func<string, string>? singularizePluralizer)
    {
        _nameGetter = nameGetter;
        _cSharpUtilities = cSharpUtilities;
        _singularizePluralizer = singularizePluralizer;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetName(T item)
    {
        if (NameCache.TryGetValue(item, out var cachedName))
        {
            return cachedName;
        }

        var name = _cSharpUtilities.GenerateCSharpIdentifier(
            _nameGetter(item), existingIdentifiers: null, singularizePluralizer: _singularizePluralizer);
        NameCache.Add(item, name);
        return name;
    }
}
