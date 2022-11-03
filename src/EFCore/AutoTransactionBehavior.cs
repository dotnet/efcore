// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates whether or not a transaction will be created automatically by <see cref="DbContext.SaveChanges()" /> if a user transaction
///     wasn't created via 'BeginTransaction' or provided via 'UseTransaction'.
/// </summary>
public enum AutoTransactionBehavior
{
    /// <summary>
    ///     Transactions are automatically created as needed. For example, most single SQL statements are implicitly executed within a
    ///     transaction, and so do not require an explicit one to be created, reducing database round trips. This is the default setting.
    /// </summary>
    WhenNeeded,

    /// <summary>
    ///     Transactions are always created automatically, as long there's no user transaction. This setting may create transactions even
    ///     when they're not needed, adding additional database round trips which may degrade performance.
    /// </summary>
    Always,

    /// <summary>
    ///     Transactions are never created automatically. Use this options with caution, since the database could be left in an inconsistent
    ///     state if a failure occurs.
    /// </summary>
    Never
}
