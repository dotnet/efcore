// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a relationship where a foreign key composed of properties on the dependent entity type
///     references a corresponding primary or alternate key on the principal entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyForeignKey : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the dependent entity type. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the relationship is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    IReadOnlyEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the foreign key properties in the dependent entity.
    /// </summary>
    IReadOnlyList<IReadOnlyProperty> Properties { get; }

    /// <summary>
    ///     Gets the principal entity type that this relationship targets. This may be different from the type that
    ///     <see cref="PrincipalKey" /> is defined on when the relationship targets a derived type in an inheritance
    ///     hierarchy (since the key is defined on the base type of the hierarchy).
    /// </summary>
    IReadOnlyEntityType PrincipalEntityType { get; }

    /// <summary>
    ///     Gets the primary or alternate key that the relationship targets.
    /// </summary>
    IReadOnlyKey PrincipalKey { get; }

    /// <summary>
    ///     Gets the navigation property on the dependent entity type that points to the principal entity.
    /// </summary>
    IReadOnlyNavigation? DependentToPrincipal { get; }

    /// <summary>
    ///     Gets the navigation property on the principal entity type that points to the dependent entity.
    /// </summary>
    IReadOnlyNavigation? PrincipalToDependent { get; }

    /// <summary>
    ///     Gets a value indicating whether the values assigned to the foreign key properties are unique.
    /// </summary>
    bool IsUnique { get; }

    /// <summary>
    ///     Gets a value indicating whether the principal entity is required.
    ///     If <see langword="true" />, the dependent entity must always be assigned to a valid principal entity.
    /// </summary>
    bool IsRequired { get; }

    /// <summary>
    ///     Gets a value indicating whether the dependent entity is required.
    ///     If <see langword="true" />, the principal entity must always have a valid dependent entity assigned.
    /// </summary>
    bool IsRequiredDependent { get; }

    /// <summary>
    ///     Gets a value indicating whether this relationship defines an ownership.
    ///     If <see langword="true" />, the dependent entity must always be accessed via the navigation from the principal entity.
    /// </summary>
    bool IsOwnership { get; }

    /// <summary>
    ///     Gets a value indicating how a delete operation is applied to dependent entities in the relationship when the
    ///     principal is deleted or the relationship is severed.
    /// </summary>
    DeleteBehavior DeleteBehavior { get; }

    /// <summary>
    ///     Gets the skip navigations using this foreign key.
    /// </summary>
    /// <returns>The skip navigations using this foreign key.</returns>
    IEnumerable<IReadOnlySkipNavigation> GetReferencingSkipNavigations()
        => PrincipalEntityType.GetSkipNavigations().Where(n => !n.IsOnDependent && n.ForeignKey == this)
            .Concat(DeclaringEntityType.GetSkipNavigations().Where(n => n.IsOnDependent && n.ForeignKey == this));

    /// <summary>
    ///     Gets the entity type related to the given one.
    /// </summary>
    /// <param name="entityType">One of the entity types related by the foreign key.</param>
    /// <returns>The entity type related to the given one.</returns>
    IReadOnlyEntityType GetRelatedEntityType(IReadOnlyEntityType entityType)
    {
        if (DeclaringEntityType != entityType
            && PrincipalEntityType != entityType)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityTypeNotInRelationshipStrict(
                    entityType.DisplayName(),
                    DeclaringEntityType.DisplayName(),
                    PrincipalEntityType.DisplayName()));
        }

        return DeclaringEntityType == entityType
            ? PrincipalEntityType
            : DeclaringEntityType;
    }

    /// <summary>
    ///     Returns a navigation associated with this foreign key.
    /// </summary>
    /// <param name="pointsToPrincipal">
    ///     A value indicating whether the navigation is on the dependent type pointing to the principal type.
    /// </param>
    /// <returns>
    ///     A navigation associated with this foreign key or <see langword="null" />.
    /// </returns>
    IReadOnlyNavigation? GetNavigation(bool pointsToPrincipal)
        => pointsToPrincipal ? DependentToPrincipal : PrincipalToDependent;

    /// <summary>
    ///     Returns a value indicating whether the foreign key is defined on the primary key and pointing to the same primary key.
    /// </summary>
    /// <returns>A value indicating whether the foreign key is defined on the primary key and pointing to the same primary key.</returns>
    bool IsBaseLinking()
    {
        var primaryKey = DeclaringEntityType.FindPrimaryKey();
        return primaryKey == PrincipalKey
            && Properties.SequenceEqual(primaryKey.Properties);
    }

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        try
        {
            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append("ForeignKey: ");
            }

            builder
                .Append(DeclaringEntityType.DisplayName())
                .Append(' ')
                .Append(Properties.Format())
                .Append(" -> ")
                .Append(PrincipalEntityType.DisplayName())
                .Append(' ')
                .Append(PrincipalKey.Properties.Format());

            if (IsUnique)
            {
                builder.Append(" Unique");
            }

            if (IsRequired)
            {
                builder.Append(" Required");
            }

            if (IsRequiredDependent)
            {
                builder.Append(" RequiredDependent");
            }

            if (IsOwnership)
            {
                builder.Append(" Ownership");
            }

            if (DeleteBehavior != DeleteBehavior.NoAction)
            {
                builder
                    .Append(' ')
                    .Append(DeleteBehavior);
            }

            if (PrincipalToDependent != null)
            {
                builder.Append(" ToDependent: ").Append(PrincipalToDependent.Name);
            }

            if (DependentToPrincipal != null)
            {
                builder.Append(" ToPrincipal: ").Append(DependentToPrincipal.Name);
            }

            if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent + 2));
            }
        }
        catch (Exception exception)
        {
            builder.AppendLine().AppendLine(CoreStrings.DebugViewError(exception.Message));
        }

        return builder.ToString();
    }
}
