// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Represents the conditions under which a lock is released implicitly.
/// </summary>
public enum LockReleaseBehavior
{
    /// <summary>
    ///     The lock is released when the transaction is committed or rolled back.
    /// </summary>
    Transaction,

    /// <summary>
    ///     The lock is released when the connection is closed.
    /// </summary>
    Connection,

    /// <summary>
    ///     The lock can only be released explicitly.
    /// </summary>
    Explicit
}
