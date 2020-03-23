// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Extension methods for <see cref="Type" /> instances.
    ///     </para>
    ///     <para>
    ///         These extensions are typically used by database providers (and other extensions). They are generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     <para>
        ///         Generates a short, human-readable name of the type such as is suitable for exception messages, etc.
        ///     </para>
        ///     <para>
        ///         Notes that this name should be used for display purposes only. It is not the same string
        ///         as the entity type name in the model.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="type"> The type. </param>
        /// <returns> The human-readable name. </returns>
        public static string ShortDisplayName([NotNull] this Type type)
            => type.DisplayName(fullName: false);

        /// <summary>
        ///     Gets a value indicating whether this type is same as or implements <see cref="IQueryable" />
        /// </summary>
        /// <param name="type"> The type to check. </param>
        /// <returns>
        ///     <c>True</c> if the type is same as or implements <see cref="IQueryable" />, otherwise <c>false</c>.
        /// </returns>
        public static bool IsQueryableType([NotNull] this Type type)
        {
            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                return true;
            }

            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>));
        }
    }
}
