// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Data.Sqlite;

/// <summary>
///     Specifies the synchronous mode used by SQLite to control how aggressively
///     database changes are synchronized to persistent storage.
/// </summary>
public enum SqliteSynchronousMode
{
    /// <summary>
    ///     Disables synchronous disk synchronization.
    /// </summary>
    Off = 0,

    /// <summary>
    ///     Uses normal synchronization, providing a balance between performance
    ///     and durability.
    /// </summary>
    Normal = 1,

    /// <summary>
    ///     Uses full synchronization to provide stronger durability guarantees.
    /// </summary>
    Full = 2,

    /// <summary>
    ///     Uses extra synchronization for the strongest durability guarantees
    ///     supported by SQLite.
    /// </summary>
    Extra = 3
}
