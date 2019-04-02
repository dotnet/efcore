// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
        public static bool HasClrType([NotNull] this ITypeBase type)
            => type.ClrType != null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyInfo FindIndexerProperty([NotNull] this ITypeBase type)
            => (type is TypeBase typeBase
                ? typeBase.GetRuntimeProperties().Values
                : type.ClrType?.GetRuntimeProperties())
                    ?.FirstOrDefault(p => p.IsEFIndexerProperty());


        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static PropertyInfo GetIndexerProperty([NotNull] this ITypeBase type)
        {
            var indexerProperty = type.FindIndexerProperty();
            if (indexerProperty == null)
            {
                throw new InvalidOperationException(CoreStrings.NoIndexer(type.DisplayName()));
            }

            return indexerProperty;
        }
    }
}
