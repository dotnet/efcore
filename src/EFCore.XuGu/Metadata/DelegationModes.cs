// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Provides precice control over recursive character set or collation delegation/inheritance aspects.
    /// </summary>
    [Flags]
    public enum DelegationModes
    {
        /// <summary>
        /// The current default is <see cref="ApplyToAll"/>.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Applys the character set or collation to databases.
        /// </summary>
        ApplyToDatabases = 1 << 0,

        /// <summary>
        /// Applys the character set or collation to tables.
        /// </summary>
        ApplyToTables = 1 << 1,

        /// <summary>
        /// Applys the character set or collation to columns.
        /// </summary>
        ApplyToColumns = 1 << 2,

        /// <summary>
        /// Applys the character set or collation to all objects.
        /// </summary>
        ApplyToAll = ApplyToDatabases | ApplyToTables | ApplyToColumns
    }
}
