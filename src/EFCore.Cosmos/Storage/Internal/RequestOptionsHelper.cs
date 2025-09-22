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
    private RequestOptionsHelper(string? ifMatchEtag, bool enableContentResponseOnWrite)
    {
        IfMatchEtag = ifMatchEtag;
        EnableContentResponseOnWrite = enableContentResponseOnWrite;
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
    public virtual bool EnableContentResponseOnWrite { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static RequestOptionsHelper? Create(IUpdateEntry entry, bool? enableContentResponseOnWrite)
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

        bool enabledContentResponse;
        if (enableContentResponseOnWrite.HasValue)
        {
            enabledContentResponse = enableContentResponseOnWrite.Value;
        }
        else
        {
            switch (entry.EntityState)
            {
                case EntityState.Modified:
                {
                    var jObjectProperty = entry.EntityType.FindProperty(CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName);
                    enabledContentResponse = (jObjectProperty?.ValueGenerated & ValueGenerated.OnUpdate) == ValueGenerated.OnUpdate;
                    break;
                }
                case EntityState.Added:
                {
                    var jObjectProperty = entry.EntityType.FindProperty(CosmosPartitionKeyInPrimaryKeyConvention.JObjectPropertyName);
                    enabledContentResponse = (jObjectProperty?.ValueGenerated & ValueGenerated.OnAdd) == ValueGenerated.OnAdd;
                    break;
                }
                default:
                    enabledContentResponse = false;
                    break;
            }
        }

        return new RequestOptionsHelper((string?)etag, enabledContentResponse);
    }
}
