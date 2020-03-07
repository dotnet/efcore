// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class SkipNavigationExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static MemberIdentity CreateMemberIdentity([CanBeNull] this ISkipNavigation navigation)
            => navigation?.GetIdentifyingMemberInfo() == null
                ? MemberIdentity.Create(navigation?.Name)
                : MemberIdentity.Create(navigation.GetIdentifyingMemberInfo());

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static string ToDebugString(
            [NotNull] this ISkipNavigation navigation,
            MetadataDebugStringOptions options,
            [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

            var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
            if (singleLine)
            {
                builder.Append($"SkipNavigation: {navigation.DeclaringEntityType.DisplayName()}.");
            }

            builder.Append(navigation.Name);

            var field = navigation.GetFieldName();
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

            builder.Append(navigation.ClrType?.ShortDisplayName()).Append(")");

            if (navigation.IsCollection)
            {
                builder.Append(" Collection");
            }

            builder.Append(navigation.TargetEntityType.DisplayName());

            if (navigation.Inverse != null)
            {
                builder.Append(" Inverse: ").Append(navigation.Inverse.Name);
            }

            if (navigation.GetPropertyAccessMode() != PropertyAccessMode.PreferField)
            {
                builder.Append(" PropertyAccessMode.").Append(navigation.GetPropertyAccessMode());
            }

            if ((options & MetadataDebugStringOptions.IncludePropertyIndexes) != 0)
            {
                var indexes = navigation.GetPropertyIndexes();
                builder.Append(" ").Append(indexes.Index);
                builder.Append(" ").Append(indexes.OriginalValueIndex);
                builder.Append(" ").Append(indexes.RelationshipIndex);
                builder.Append(" ").Append(indexes.ShadowIndex);
                builder.Append(" ").Append(indexes.StoreGenerationIndex);
            }

            if (!singleLine &&
                (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(navigation.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }
    }
}
