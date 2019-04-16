// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class MutableEntityTypeExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IMutableEntityType> GetDerivedTypesInclusive([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetDerivedTypesInclusive().Cast<IMutableEntityType>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IMutableForeignKey> GetDeclaredForeignKeys([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetDeclaredForeignKeys().Cast<IMutableForeignKey>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static IEnumerable<IMutableProperty> GetDeclaredProperties([NotNull] this IMutableEntityType entityType)
            => ((IEntityType)entityType).GetDeclaredProperties().Cast<IMutableProperty>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void AddData([NotNull] this IMutableEntityType entityType, [NotNull] params object[] data)
            => entityType.AsEntityType().AddData(data);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void AddData([NotNull] this IMutableEntityType entityType, [NotNull] IEnumerable<object> data)
            => entityType.AsEntityType().AddData(data);
    }
}
