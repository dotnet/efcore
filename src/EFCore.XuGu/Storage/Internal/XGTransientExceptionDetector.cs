// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    /// <summary>
    ///     Detects the exceptions caused by MySQL transient failures.
    /// </summary>
    public static class XGTransientExceptionDetector
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool ShouldRetryOn([NotNull] Exception ex)
            => ex is TimeoutException;
    }
}
