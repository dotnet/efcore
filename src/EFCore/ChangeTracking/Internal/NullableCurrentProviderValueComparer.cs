// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class NullableStructCurrentProviderValueComparer<TModel, TProvider> : IComparer<IUpdateEntry>
    where TModel : struct
{
    private readonly IProperty _property;
    private readonly IComparer<TProvider> _underlyingComparer;
    private readonly Func<TModel, TProvider> _converter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NullableStructCurrentProviderValueComparer(IProperty property)
    {
        _property = property;
        _converter = ((ValueConverter<TModel, TProvider>)property.GetTypeMapping().Converter!).ConvertToProviderTyped;
        _underlyingComparer = Comparer<TProvider>.Default;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int Compare(IUpdateEntry? x, IUpdateEntry? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var xValue = x.GetCurrentValue<TModel?>(_property);
        var yValue = y.GetCurrentValue<TModel?>(_property);

        return !xValue.HasValue
            && !yValue.HasValue
                ? 0
                : !xValue.HasValue
                    ? -1
                    : !yValue.HasValue
                        ? 1
                        : _underlyingComparer.Compare(_converter(xValue.Value), _converter(yValue.Value));
    }
}
