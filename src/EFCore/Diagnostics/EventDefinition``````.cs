// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Defines metadata for an event with three parameters and a cached delegate to log the
///     event with reduced allocations.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class EventDefinition<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> : EventDefinitionBase
{
    private readonly Action<ILogger, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Exception?> _logAction;

    /// <summary>
    ///     Creates an event definition instance.
    /// </summary>
    /// <param name="loggingOptions">Logging options.</param>
    /// <param name="eventId">The <see cref="EventId" />.</param>
    /// <param name="level">The <see cref="LogLevel" /> at which the event will be logged.</param>
    /// <param name="eventIdCode">
    ///     A string representing the code that should be passed to <see cref="DbContextOptionsBuilder.ConfigureWarnings" />.
    /// </param>
    /// <param name="logActionFunc">Function to create a cached delegate for logging the event.</param>
    public EventDefinition(
        ILoggingOptions loggingOptions,
        EventId eventId,
        LogLevel level,
        string eventIdCode,
        Func<LogLevel, Action<ILogger, TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, Exception?>> logActionFunc)
        : base(loggingOptions, eventId, level, eventIdCode)
    {
        _logAction = logActionFunc(Level);
    }

    /// <summary>
    ///     Generates the message that would be logged without logging it.
    ///     Typically used for throwing an exception in warning-as-error cases.
    /// </summary>
    /// <param name="arg1">The first message argument.</param>
    /// <param name="arg2">The second message argument.</param>
    /// <param name="arg3">The third message argument.</param>
    /// <param name="arg4">The fourth message argument.</param>
    /// <param name="arg5">The fifth message argument.</param>
    /// <param name="arg6">The sixth message argument.</param>
    /// <returns>The message string.</returns>
    public virtual string GenerateMessage(
        TParam1 arg1,
        TParam2 arg2,
        TParam3 arg3,
        TParam4 arg4,
        TParam5 arg5,
        TParam6 arg6)
    {
        var extractor = new MessageExtractingLogger();
        _logAction(extractor, arg1, arg2, arg3, arg4, arg5, arg6, null);
        return extractor.Message;
    }

    /// <summary>
    ///     Logs the event, or throws if the event has been configured to be treated as an error.
    /// </summary>
    /// <typeparam name="TLoggerCategory">The <see cref="DbLoggerCategory" />.</typeparam>
    /// <param name="logger">The logger to which the event should be logged.</param>
    /// <param name="arg1">The first message argument.</param>
    /// <param name="arg2">The second message argument.</param>
    /// <param name="arg3">The third message argument.</param>
    /// <param name="arg4">The fourth message argument.</param>
    /// <param name="arg5">The fifth message argument.</param>
    /// <param name="arg6">The sixth message argument.</param>
    public virtual void Log<TLoggerCategory>(
        IDiagnosticsLogger<TLoggerCategory> logger,
        TParam1 arg1,
        TParam2 arg2,
        TParam3 arg3,
        TParam4 arg4,
        TParam5 arg5,
        TParam6 arg6)
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        switch (WarningBehavior)
        {
            case WarningBehavior.Log:
                _logAction(logger.Logger, arg1, arg2, arg3, arg4, arg5, arg6, null);
                break;
            case WarningBehavior.Throw:
                throw WarningAsError(GenerateMessage(arg1, arg2, arg3, arg4, arg5, arg6));
        }
    }
}
