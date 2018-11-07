// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public static class ModelExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string GetProductVersion([NotNull] this IModel model)
            => model[CoreAnnotationNames.ProductVersionAnnotation] as string;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SetProductVersion([NotNull] this Model model, [NotNull] string value)
            => model[CoreAnnotationNames.ProductVersionAnnotation] = value;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool ShouldBeOwnedType([NotNull] this IModel model, [NotNull] string value)
            => model[CoreAnnotationNames.OwnedTypesAnnotation] is HashSet<string> ownedTypes && ownedTypes.Contains(value);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool ShouldBeOwnedType([NotNull] this IModel model, [NotNull] Type clrType)
        {
            if (!(model[CoreAnnotationNames.OwnedTypesAnnotation] is HashSet<string> ownedTypes))
            {
                return false;
            }

            while (clrType != null)
            {
                var name = (model as Model)?.GetDisplayName(clrType) ?? clrType.DisplayName();
                if (ownedTypes.Contains(name))
                {
                    return true;
                }

                clrType = clrType.BaseType;
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MarkAsOwnedType([NotNull] this Model model, [NotNull] string value)
        {
            if (!(model[CoreAnnotationNames.OwnedTypesAnnotation] is HashSet<string> ownedTypes))
            {
                ownedTypes = new HashSet<string>(StringComparer.Ordinal);
                model[CoreAnnotationNames.OwnedTypesAnnotation] = ownedTypes;
            }

            ownedTypes.Add(value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MarkAsOwnedType([NotNull] this Model model, [NotNull] Type clrType)
            => model.MarkAsOwnedType(model.GetDisplayName(clrType));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void UnmarkAsOwnedType([NotNull] this Model model, [NotNull] string value)
        {
            var ownedTypes = model[CoreAnnotationNames.OwnedTypesAnnotation] as HashSet<string>;
            ownedTypes?.Remove(value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void UnmarkAsOwnedType([NotNull] this Model model, [NotNull] Type clrType)
        {
            if (!(model[CoreAnnotationNames.OwnedTypesAnnotation] is HashSet<string> ownedTypes))
            {
                return;
            }

            while (clrType != null)
            {
                ownedTypes.Remove(model.GetDisplayName(clrType));

                clrType = clrType.BaseType;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IEntityType> GetRootEntityTypes([NotNull] this IModel model)
            => model.GetEntityTypes().Where(e => e.BaseType == null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string ToDebugString([NotNull] this IModel model, [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent).Append("Model: ");

            if (model.GetPropertyAccessMode() != null)
            {
                builder.Append(" PropertyAccessMode.").Append(model.GetPropertyAccessMode());
            }

            if (model.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
            {
                builder.Append(" ChangeTrackingStrategy.").Append(model.GetChangeTrackingStrategy());
            }

            var entityTypes = model.GetEntityTypes().ToList();
            foreach (var entityType in entityTypes)
            {
                builder.AppendLine().Append(entityType.ToDebugString(false, indent + "  "));
            }

            builder.Append(model.AnnotationsToDebugString(indent));

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Model AsModel([NotNull] this IModel model, [CallerMemberName] [NotNull] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IModel, Model>(model, methodName);
    }
}
