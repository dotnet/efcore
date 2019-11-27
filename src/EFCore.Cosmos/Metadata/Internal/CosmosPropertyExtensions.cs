// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class CosmosPropertyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static bool IsOrdinalKeyProperty(this IProperty property)
        {
            Debug.Assert(property.DeclaringEntityType.IsOwned());
            Debug.Assert(property.GetPropertyName().Length == 0);

            return property.IsPrimaryKey()
                && !property.IsForeignKey()
                && property.ClrType == typeof(int)
                && property.ValueGenerated == ValueGenerated.OnAdd
                && property.DeclaringEntityType.FindPrimaryKey().Properties.Count > 1;
        }
    }
}
