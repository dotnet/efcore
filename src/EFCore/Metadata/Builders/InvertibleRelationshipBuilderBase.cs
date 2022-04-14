// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Base class used for configuring an invertible relationship.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public abstract class InvertibleRelationshipBuilderBase : IInfrastructure<IConventionForeignKeyBuilder>
{
    private readonly IReadOnlyList<Property>? _foreignKeyProperties;
    private readonly IReadOnlyList<Property>? _principalKeyProperties;
    private readonly bool? _required;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected InvertibleRelationshipBuilderBase(
        IMutableEntityType declaringEntityType,
        IMutableEntityType relatedEntityType,
        IMutableForeignKey foreignKey)
    {
        Builder = ((ForeignKey)foreignKey).Builder;

        DeclaringEntityType = declaringEntityType;
        RelatedEntityType = relatedEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected InvertibleRelationshipBuilderBase(
        InternalForeignKeyBuilder builder,
        InvertibleRelationshipBuilderBase oldBuilder,
        bool inverted = false,
        bool foreignKeySet = false,
        bool principalKeySet = false,
        bool requiredSet = false)
    {
        Builder = builder;

        if (inverted)
        {
            if (oldBuilder._foreignKeyProperties != null
                || oldBuilder._principalKeyProperties != null)
            {
                throw new InvalidOperationException(CoreStrings.RelationshipCannotBeInverted);
            }
        }

        DeclaringEntityType = oldBuilder.DeclaringEntityType;
        RelatedEntityType = oldBuilder.RelatedEntityType;

        _foreignKeyProperties = foreignKeySet
            ? builder.Metadata.Properties
            : oldBuilder._foreignKeyProperties;
        _principalKeyProperties = principalKeySet
            ? builder.Metadata.PrincipalKey.Properties
            : oldBuilder._principalKeyProperties;
        _required = requiredSet
            ? builder.Metadata.IsRequired
            : oldBuilder._required;

        var foreignKey = builder.Metadata;
        ForeignKey.AreCompatible(
            foreignKey.PrincipalEntityType,
            foreignKey.DeclaringEntityType,
            foreignKey.DependentToPrincipal?.GetIdentifyingMemberInfo(),
            foreignKey.PrincipalToDependent?.GetIdentifyingMemberInfo(),
            _foreignKeyProperties,
            _principalKeyProperties,
            foreignKey.IsUnique,
            shouldThrow: true);
    }

    /// <summary>
    ///     Gets the first entity type used to configure this relationship.
    /// </summary>
    protected virtual IMutableEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the second entity type used to configure this relationship.
    /// </summary>
    protected virtual IMutableEntityType RelatedEntityType { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalForeignKeyBuilder Builder { get; set; }

    /// <inheritdoc />
    IConventionForeignKeyBuilder IInfrastructure<IConventionForeignKeyBuilder>.Instance
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     The foreign key that represents this relationship.
    /// </summary>
    public virtual IMutableForeignKey Metadata
        => Builder.Metadata;

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
