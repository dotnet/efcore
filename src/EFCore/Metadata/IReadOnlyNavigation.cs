// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a navigation property which can be used to navigate a relationship.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyNavigation : IReadOnlyNavigationBase
{
    /// <summary>
    ///     Gets the entity type that this navigation property belongs to.
    /// </summary>
    new IReadOnlyEntityType DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => IsOnDependent ? ForeignKey.DeclaringEntityType : ForeignKey.PrincipalEntityType;
    }

    /// <summary>
    ///     Gets the entity type that this navigation property will hold an instance(s) of.
    /// </summary>
    new IReadOnlyEntityType TargetEntityType
    {
        [DebuggerStepThrough]
        get => IsOnDependent ? ForeignKey.PrincipalEntityType : ForeignKey.DeclaringEntityType;
    }

    /// <summary>
    ///     Gets the inverse navigation.
    /// </summary>
    new IReadOnlyNavigation? Inverse
    {
        [DebuggerStepThrough]
        get => IsOnDependent ? ForeignKey.PrincipalToDependent : ForeignKey.DependentToPrincipal;
    }

    /// <summary>
    ///     Gets the foreign key that defines the relationship this navigation property will navigate.
    /// </summary>
    IReadOnlyForeignKey ForeignKey { get; }

    /// <summary>
    ///     Gets a value indicating whether the navigation property is defined on the dependent side of the underlying foreign key.
    /// </summary>
    bool IsOnDependent
    {
        [DebuggerStepThrough]
        get => ForeignKey.DependentToPrincipal == this;
    }

    /// <summary>
    ///     Gets the entity type that this navigation property belongs to.
    /// </summary>
    IReadOnlyEntityType IReadOnlyNavigationBase.DeclaringEntityType
    {
        [DebuggerStepThrough]
        get => DeclaringEntityType;
    }

    /// <summary>
    ///     Gets the entity type that this navigation property will hold an instance(s) of.
    /// </summary>
    IReadOnlyEntityType IReadOnlyNavigationBase.TargetEntityType
    {
        [DebuggerStepThrough]
        get => TargetEntityType;
    }

    /// <summary>
    ///     Gets the inverse navigation.
    /// </summary>
    IReadOnlyNavigationBase? IReadOnlyNavigationBase.Inverse
    {
        [DebuggerStepThrough]
        get => Inverse;
    }

    /// <summary>
    ///     Gets a value indicating whether the navigation property is a collection property.
    /// </summary>
    bool IReadOnlyNavigationBase.IsCollection
    {
        [DebuggerStepThrough]
        get => !IsOnDependent && !ForeignKey.IsUnique;
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
                builder.Append($"Navigation: {DeclaringEntityType.DisplayName()}.");
            }

            builder.Append(Name);

            var field = GetFieldName();
            if (field == null)
            {
                builder.Append(" (no field, ");
            }
            else if (!field.EndsWith(">k__BackingField", StringComparison.Ordinal))
            {
                builder.Append($" ({field}, ");
            }
            else
            {
                builder.Append(" (");
            }

            builder.Append(ClrType.ShortDisplayName()).Append(')');

            if (IsCollection)
            {
                builder.Append(" Collection");
            }

            builder.Append(IsOnDependent ? " ToPrincipal " : " ToDependent ");

            builder.Append(TargetEntityType.DisplayName());

            if (Inverse != null)
            {
                builder.Append(" Inverse: ").Append(Inverse.Name);
            }

            if (GetPropertyAccessMode() != PropertyAccessMode.PreferField)
            {
                builder.Append(" PropertyAccessMode.").Append(GetPropertyAccessMode());
            }

            if ((options & MetadataDebugStringOptions.IncludePropertyIndexes) != 0
                && ((AnnotatableBase)this).IsReadOnly)
            {
                var indexes = ((INavigation)this).GetPropertyIndexes();
                builder.Append(' ').Append(indexes.Index);
                builder.Append(' ').Append(indexes.OriginalValueIndex);
                builder.Append(' ').Append(indexes.RelationshipIndex);
                builder.Append(' ').Append(indexes.ShadowIndex);
                builder.Append(' ').Append(indexes.StoreGenerationIndex);
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
