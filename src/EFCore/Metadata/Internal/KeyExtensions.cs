// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class KeyExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Func<bool, IIdentityMap> GetIdentityMapFactory([NotNull] this IKey key)
            => key.AsKey().IdentityMapFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Func<IWeakReferenceIdentityMap> GetWeakReferenceIdentityMapFactory([NotNull] this IKey key)
            => key.AsKey().WeakReferenceIdentityMapFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IPrincipalKeyValueFactory<TKey> GetPrincipalKeyValueFactory<TKey>([NotNull] this IKey key)
            => key.AsKey().GetPrincipalKeyValueFactory<TKey>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsPrimaryKey([NotNull] this IKey key)
            => key == key.DeclaringEntityType.FindPrimaryKey();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static int IndexOf([NotNull] this IKey key, [NotNull] IProperty property)
        {
            var index = 0;
            for (; index < key.Properties.Count && key.Properties[index] != property; index++)
            {
            }

            return index == key.Properties.Count ? -1 : index;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static string ToDebugString([NotNull] this IKey key, bool singleLine = true, [NotNull] string indent = "")
        {
            var builder = new StringBuilder();

            builder.Append(indent);

            if (singleLine)
            {
                builder.Append("Key: ");
            }

            builder.Append(
                string.Join(
                    ", ", key.Properties.Select(
                        p => singleLine
                            ? p.DeclaringEntityType.DisplayName() + "." + p.Name
                            : p.Name)));

            if (key.IsPrimaryKey())
            {
                builder.Append(" PK");
            }

            if (!singleLine)
            {
                builder.Append(key.AnnotationsToDebugString(indent + "  "));
            }

            return builder.ToString();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static Key AsKey([NotNull] this IKey key, [NotNull] [CallerMemberName] string methodName = "")
            => MetadataExtensions.AsConcreteMetadataType<IKey, Key>(key, methodName);
    }
}
