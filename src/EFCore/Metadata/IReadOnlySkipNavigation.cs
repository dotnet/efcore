// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property that is part of a relationship
    ///     that is forwarded through a third entity type.
    /// </summary>
    public interface IReadOnlySkipNavigation : IReadOnlyNavigationBase
    {
        /// <summary>
        ///     Gets the join type used by the foreign key.
        /// </summary>
        IReadOnlyEntityType? JoinEntityType
            => IsOnDependent ? ForeignKey?.PrincipalEntityType : ForeignKey?.DeclaringEntityType;

        /// <summary>
        ///     Gets the inverse skip navigation.
        /// </summary>
        new IReadOnlySkipNavigation Inverse { get; }

        /// <summary>
        ///     Gets the inverse navigation.
        /// </summary>
        IReadOnlyNavigationBase IReadOnlyNavigationBase.Inverse
        {
            [DebuggerStepThrough]
            get => Inverse;
        }

        /// <summary>
        ///     Gets the foreign key to the join type.
        /// </summary>
        IReadOnlyForeignKey? ForeignKey { get; }

        /// <summary>
        ///     Gets a value indicating whether the navigation property is defined on the dependent side of the underlying foreign key.
        /// </summary>
        bool IsOnDependent { get; }

        /// <summary>
        ///     <para>
        ///         Creates a human-readable representation of the given metadata.
        ///     </para>
        ///     <para>
        ///         Warning: Do not rely on the format of the returned string.
        ///         It is designed for debugging only and may change arbitrarily between releases.
        ///     </para>
        /// </summary>
        /// <param name="options"> Options for generating the string. </param>
        /// <param name="indent"> The number of indent spaces to use before each new line. </param>
        /// <returns> A human-readable representation. </returns>
        string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
        {
            var builder = new StringBuilder();
            var indentString = new string(' ', indent);

            builder.Append(indentString);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append($"SkipNavigation: {DeclaringEntityType.DisplayName()}.");
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

            builder.Append(ClrType?.ShortDisplayName()).Append(")");

            if (IsCollection)
            {
                builder.Append(" Collection");
            }

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
                var indexes = ((ISkipNavigation)this).GetPropertyIndexes();
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

            return builder.ToString();
        }
    }
}
