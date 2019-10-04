// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class NavigationExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IClrCollectionAccessor GetCollectionAccessor([NotNull] this INavigation navigation)
            => navigation.AsNavigation().CollectionAccessor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string ToDebugString([NotNull] this INavigation navigation, bool singleLine = true, [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

            if (singleLine)
            {
                builder.Append("Navigation: ").Append(navigation.DeclaringEntityType.DisplayName()).Append(".");
            }

            builder.Append(navigation.Name);

            if (navigation.GetFieldName() == null)
            {
                builder.Append(" (no field, ");
            }
            else
            {
                builder.Append(" (").Append(navigation.GetFieldName()).Append(", ");
            }

            builder.Append(navigation.ClrType?.ShortDisplayName()).Append(")");

            if (navigation.IsCollection())
            {
                builder.Append(" Collection");
            }

            builder.Append(navigation.IsDependentToPrincipal() ? " ToPrincipal " : " ToDependent ");

            builder.Append(navigation.GetTargetType().DisplayName());

            if (navigation.FindInverse() != null)
            {
                builder.Append(" Inverse: ").Append(navigation.FindInverse().Name);
            }

            if (navigation.GetPropertyAccessMode() != null)
            {
                builder.Append(" PropertyAccessMode.").Append(navigation.GetPropertyAccessMode());
            }

            var indexes = navigation.GetPropertyIndexes();
            builder.Append(" ").Append(indexes.Index);
            builder.Append(" ").Append(indexes.OriginalValueIndex);
            builder.Append(" ").Append(indexes.RelationshipIndex);
            builder.Append(" ").Append(indexes.ShadowIndex);
            builder.Append(" ").Append(indexes.StoreGenerationIndex);

            if (!singleLine)
            {
                builder.Append(navigation.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Navigation AsNavigation([NotNull] this INavigation navigation, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<INavigation, Navigation>(navigation, methodName);
    }
}
