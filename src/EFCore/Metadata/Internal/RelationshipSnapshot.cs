// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RelationshipSnapshot
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RelationshipSnapshot(
        InternalForeignKeyBuilder relationship,
        EntityType.Snapshot? ownedEntityTypeSnapshot,
        List<(SkipNavigation, ConfigurationSource)>? referencingSkipNavigations)
    {
        Relationship = relationship;
        OwnedEntityTypeSnapshot = ownedEntityTypeSnapshot;
        ReferencingSkipNavigations = referencingSkipNavigations;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder Relationship { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual EntityType.Snapshot? OwnedEntityTypeSnapshot { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual List<(SkipNavigation SkipNavigation, ConfigurationSource ForeignKeyConfigurationSource)>? ReferencingSkipNavigations
    {
        [DebuggerStepThrough]
        get;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InternalForeignKeyBuilder? Attach(InternalEntityTypeBuilder? entityTypeBuilder = null)
    {
        entityTypeBuilder ??= Relationship.Metadata.DeclaringEntityType.Builder;

        var newRelationship = Relationship.Attach(entityTypeBuilder);
        if (newRelationship != null)
        {
            OwnedEntityTypeSnapshot?.Attach(
                newRelationship.Metadata.ResolveOtherEntityType(entityTypeBuilder.Metadata).Builder);

            if (ReferencingSkipNavigations != null)
            {
                foreach (var referencingNavigationTuple in ReferencingSkipNavigations)
                {
                    var skipNavigation = referencingNavigationTuple.SkipNavigation;
                    if (!skipNavigation.IsInModel)
                    {
                        var navigationEntityType = skipNavigation.DeclaringEntityType;
                        skipNavigation = !navigationEntityType.IsInModel
                            ? null
                            : navigationEntityType.FindSkipNavigation(skipNavigation.Name);
                    }

                    skipNavigation?.Builder.HasForeignKey(
                        newRelationship.Metadata, referencingNavigationTuple.ForeignKeyConfigurationSource);
                }
            }
        }

        return newRelationship;
    }
}
