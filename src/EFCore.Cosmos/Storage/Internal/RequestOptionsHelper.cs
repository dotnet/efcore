// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RequestOptionsHelper
{
    private RequestOptionsHelper(string? ifMatchEtag)
    {
        IfMatchEtag = ifMatchEtag;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? IfMatchEtag { get; }


    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RequestOptionsHelper? Create(IUpdateEntry entry)
    {
        var etagProperty = entry.EntityType.GetETagProperty();
        if (etagProperty == null)
        {
            return null;
        }

        var etag = entry.GetOriginalValue(etagProperty);
        var converter = etagProperty.GetTypeMapping().Converter;
        if (converter != null)
        {
            etag = converter.ConvertToProvider(etag);
        }

        return new RequestOptionsHelper((string?)etag);
    }
}
