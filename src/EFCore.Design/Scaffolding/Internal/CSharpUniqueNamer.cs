// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CSharpUniqueNamer<T> : CSharpNamer<T>
    where T : notnull
{
    private readonly HashSet<string> _usedNames;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CSharpUniqueNamer(
        Func<T, string> nameGetter,
        ICSharpUtilities cSharpUtilities,
        Func<string, string>? singularizePluralizer,
        bool caseSensitive)
        : this(nameGetter, null, cSharpUtilities, singularizePluralizer, caseSensitive)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CSharpUniqueNamer(
        Func<T, string> nameGetter,
        IEnumerable<string>? usedNames,
        ICSharpUtilities cSharpUtilities,
        Func<string, string>? singularizePluralizer,
        bool caseSensitive)
        : base(nameGetter, cSharpUtilities, singularizePluralizer)
    {
        _usedNames = new(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
        if (usedNames != null)
        {
            foreach (var name in usedNames)
            {
                _usedNames.Add(name);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetName(T item)
    {
        if (NameCache.ContainsKey(item))
        {
            return base.GetName(item);
        }

        var input = base.GetName(item);
        var name = input;
        var suffix = 1;

        while (_usedNames.Contains(name))
        {
            name = input + suffix++;
        }

        _usedNames.Add(name);
        NameCache[item] = name;

        return name;
    }
}
