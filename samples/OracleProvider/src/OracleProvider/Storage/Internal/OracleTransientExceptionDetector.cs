// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Oracle.ManagedDataAccess.Client;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     Detects the exceptions caused by SQL Server transient failures.
    /// </summary>
    public class OracleTransientExceptionDetector
    {
        public static bool ShouldRetryOn([NotNull] Exception ex)
        {
            if (ex is OracleException sqlException)
            {
                // TODO: Add Oracle specific error numbers.

                return false;
            }

            if (ex is TimeoutException)
            {
                return true;
            }

            return false;
        }
    }
}
