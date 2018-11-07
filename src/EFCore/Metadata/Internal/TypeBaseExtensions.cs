// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class TypeBaseExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public static string DisplayName([NotNull] this ITypeBase type)
            => type.ClrType != null
                ? type.ClrType.ShortDisplayName()
                : type.Name;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public static bool HasClrType([NotNull] this ITypeBase type)
            => type.ClrType != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public static bool IsAbstract([NotNull] this ITypeBase type)
            => type.ClrType?.GetTypeInfo().IsAbstract ?? false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyInfo EFIndexerProperty([NotNull] this ITypeBase type)
        {
            var runtimeProperties = type is TypeBase typeBase
                ? typeBase.GetRuntimeProperties().Values // better perf if we've already computed them once
                : type.ClrType.GetRuntimeProperties();

            // find the indexer with single argument of type string which returns an object
            return runtimeProperties.FirstOrDefault(p => p.IsEFIndexerProperty());
        }
    }
}
