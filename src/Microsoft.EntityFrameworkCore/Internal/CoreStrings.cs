// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public partial class CoreStrings
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete("This overload is obsolete and will be removed in the 1.1.0 release.", error: true)]
        public static string ReferencedShadowKey(
            [CanBeNull] object key,
            [CanBeNull] object referencingEntityTypeWithNavigation,
            [CanBeNull] object referencedEntityTypeWithNaviagation)
            => string.Empty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Obsolete("This method is obsolete and will be removed in the 1.1.0 release.", error: true)]
        public static string ReferencedShadowKeyWithoutNavigations(
            [CanBeNull] object key,
            [CanBeNull] object entityType,
            [CanBeNull] object foreignKey,
            [CanBeNull] object referencingEntityType)
            => string.Empty;
    }
}
