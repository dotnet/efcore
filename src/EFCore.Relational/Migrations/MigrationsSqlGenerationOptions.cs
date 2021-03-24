// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     The options to use when generating SQL for migrations.
    /// </summary>
    [Flags]
    public enum MigrationsSqlGenerationOptions
    {
        /// <summary>
        ///     Generate SQL to execute at runtime.
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Generate SQL for a script. Automatically added by <see cref="IMigrator.GenerateScript" />.
        /// </summary>
        Script = 1,

        /// <summary>
        ///     Generate SQL for an idempotent script.
        /// </summary>
        Idempotent = 1 << 1,

        /// <summary>
        ///     Generate SQL for a script without transaction statements.
        /// </summary>
        NoTransactions = 1 << 2
    }
}
