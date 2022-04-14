// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Event IDs for in-memory events that correspond to messages logged to an <see cref="ILogger" />
///     and events sent to a <see cref="DiagnosticSource" />.
/// </summary>
/// <remarks>
///     <para>
///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
///         behavior of warnings.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see>, and
///         <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
///     </para>
/// </remarks>
public static class InMemoryEventId
{
    // Warning: These values must not change between releases.
    // Only add new values to the end of sections, never in the middle.
    // Try to use <Noun><Verb> naming and be consistent with existing names.
    private enum Id
    {
        // Transaction events
        TransactionIgnoredWarning = CoreEventId.ProviderBaseId,

        // Update events
        ChangesSaved = CoreEventId.ProviderBaseId + 100
    }

    private static readonly string TransactionPrefix = DbLoggerCategory.Database.Transaction.Name + ".";

    private static EventId MakeTransactionId(Id id)
        => new((int)id, TransactionPrefix + id);

    /// <summary>
    ///     A transaction operation was requested, but ignored because in-memory does not support transactions.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Database.Transaction" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="EventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId TransactionIgnoredWarning = MakeTransactionId(Id.TransactionIgnoredWarning);

    private static readonly string UpdatePrefix = DbLoggerCategory.Update.Name + ".";

    private static EventId MakeUpdateId(Id id)
        => new((int)id, UpdatePrefix + id);

    /// <summary>
    ///     Changes were saved to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="SaveChangesEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ChangesSaved = MakeUpdateId(Id.ChangesSaved);
}
