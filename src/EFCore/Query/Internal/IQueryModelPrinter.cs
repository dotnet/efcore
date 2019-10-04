// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Remotion.Linq;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IQueryModelPrinter
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        string Print(
            [NotNull] QueryModel queryModel,
            bool removeFormatting = false,
            int? characterLimit = null,
            bool generateUniqueQsreIds = false);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        string PrintDebug(
            [NotNull] QueryModel queryModel,
            bool removeFormatting = false,
            int? characterLimit = null,
            bool generateUniqueQsreIds = true);
    }
}
