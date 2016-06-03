// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class QuerySourceExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool HasGeneratedItemName([NotNull] this IQuerySource querySource)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotEmpty(querySource.ItemName, nameof(querySource.ItemName));

            return querySource.ItemName.StartsWith("<generated>_", StringComparison.Ordinal);
        }
    }
}
