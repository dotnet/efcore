// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Frozen;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosStructuralTypeSerializerProvider
{
    private readonly FrozenDictionary<ITypeBase, Lazy<CosmosStructuralTypeSerializer>> _metadataMap;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosStructuralTypeSerializerProvider(IModel model)
    {
        var structuralTypes = new HashSet<ITypeBase>();

        foreach (var entityType in model.GetEntityTypes())
        {
            AddStructuralType(entityType);
        }

        _metadataMap = structuralTypes.ToFrozenDictionary(
            structuralType => structuralType,
            structuralType => new Lazy<CosmosStructuralTypeSerializer>(() => new CosmosStructuralTypeSerializer(this, structuralType)));

        void AddStructuralType(ITypeBase structuralType)
        {
            if (!structuralTypes.Add(structuralType))
            {
                return;
            }

            foreach (var complexProperty in structuralType.GetComplexProperties())
            {
                AddStructuralType(complexProperty.ComplexType);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual CosmosStructuralTypeSerializer Get(ITypeBase type)
        => _metadataMap[type].Value;
}
