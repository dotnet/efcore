// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Indicates whether checking of a database constraint can be deferred until the end of the transaction,
///     as defined by the SQL standard (<c>[NOT] DEFERRABLE INITIALLY {IMMEDIATE | DEFERRED}</c>).
/// </summary>
/// <remarks>
///     <para>
///         Not all databases support deferrable constraints; for example, PostgreSQL, Oracle and SQLite (foreign keys only)
///         support them, while SQL Server and MySQL do not.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
///         examples.
///     </para>
/// </remarks>
public enum ConstraintDeferrability
{
    /// <summary>
    ///     The constraint is checked immediately after every statement, and checking cannot be deferred (<c>NOT DEFERRABLE</c>).
    ///     This is the default.
    /// </summary>
    NotDeferrable = 0,

    /// <summary>
    ///     The constraint is checked immediately after every statement by default, but checking can be deferred to the
    ///     end of the transaction (e.g. with <c>SET CONSTRAINTS ... DEFERRED</c>) (<c>DEFERRABLE INITIALLY IMMEDIATE</c>).
    /// </summary>
    DeferrableInitiallyImmediate = 1,

    /// <summary>
    ///     The constraint is checked at the end of the transaction (<c>DEFERRABLE INITIALLY DEFERRED</c>).
    /// </summary>
    DeferrableInitiallyDeferred = 2
}
