// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class PropertiesSnapshot
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PropertiesSnapshot(
        List<InternalPropertyBuilder>? properties,
        List<InternalIndexBuilder>? indexes,
        List<(InternalKeyBuilder, ConfigurationSource?)>? keys,
        List<RelationshipSnapshot>? relationships)
    {
        Properties = properties;
        Indexes = indexes;
        Keys = keys;
        Relationships = relationships;
    }

    private List<InternalPropertyBuilder>? Properties { [DebuggerStepThrough] get; }
    private List<RelationshipSnapshot>? Relationships { [DebuggerStepThrough] get; set; }
    private List<InternalIndexBuilder>? Indexes { [DebuggerStepThrough] get; set; }
    private List<(InternalKeyBuilder, ConfigurationSource?)>? Keys { [DebuggerStepThrough] get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Add(List<RelationshipSnapshot> relationships)
    {
        if (Relationships == null)
        {
            Relationships = relationships;
        }
        else
        {
            Relationships.AddRange(relationships);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Add(List<InternalIndexBuilder> indexes)
    {
        if (Indexes == null)
        {
            Indexes = indexes;
        }
        else
        {
            Indexes.AddRange(indexes);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Add(List<(InternalKeyBuilder, ConfigurationSource?)> keys)
    {
        if (Keys == null)
        {
            Keys = keys;
        }
        else
        {
            Keys.AddRange(keys);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Attach(InternalTypeBaseBuilder typeBaseBuilder)
    {
        if (Properties != null)
        {
            foreach (var propertyBuilder in Properties)
            {
                propertyBuilder.Attach(typeBaseBuilder);
            }
        }

        var entityTypeBuilder = typeBaseBuilder as InternalEntityTypeBuilder
            ?? ((InternalComplexTypeBuilder)typeBaseBuilder).Metadata.ContainingEntityType.Builder;

        if (Keys != null)
        {
            foreach (var (internalKeyBuilder, configurationSource) in Keys)
            {
                internalKeyBuilder.Attach(entityTypeBuilder.Metadata.GetRootType().Builder, configurationSource);
            }
        }

        if (Indexes != null)
        {
            foreach (var indexBuilder in Indexes)
            {
                var originalEntityType = indexBuilder.Metadata.DeclaringEntityType;
                var targetEntityTypeBuilder = originalEntityType.Name == entityTypeBuilder.Metadata.Name
                    || (!originalEntityType.IsInModel && originalEntityType.ClrType == entityTypeBuilder.Metadata.ClrType)
                        ? entityTypeBuilder
                        : originalEntityType.Builder;
                indexBuilder.Attach(targetEntityTypeBuilder);
            }
        }

        if (Relationships != null)
        {
            foreach (var detachedRelationshipTuple in Relationships)
            {
                detachedRelationshipTuple.Attach(entityTypeBuilder);
            }
        }
    }
}
