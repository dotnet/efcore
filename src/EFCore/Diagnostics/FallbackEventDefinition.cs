// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Defines metadata for an event with more than six parameters such that it has to have
///     special handling.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class FallbackEventDefinition : EventDefinitionBase
{
    /// <summary>
    ///     Creates an event definition instance.
    /// </summary>
    /// <param name="loggingOptions">Logging options.</param>
    /// <param name="eventId">The <see cref="EventId" />.</param>
    /// <param name="level">The <see cref="LogLevel" /> at which the event will be logged.</param>
    /// <param name="eventIdCode">
    ///     A string representing the code that should be passed to <see cref="DbContextOptionsBuilder.ConfigureWarnings" />.
    /// </param>
    /// <param name="messageFormat">The parameterized message definition.</param>
    public FallbackEventDefinition(
        ILoggingOptions loggingOptions,
        EventId eventId,
        LogLevel level,
        string eventIdCode,
        string messageFormat)
        : base(loggingOptions, eventId, level, eventIdCode)
    {
        MessageFormat = messageFormat;
    }

    /// <summary>
    ///     Generates the message that would be logged without logging it.
    ///     Typically used for throwing an exception in warning-as-error cases.
    /// </summary>
    /// <param name="logAction">A delegate that will log the message to an <see cref="ILogger" />.</param>
    /// <returns>The message string.</returns>
    public virtual string GenerateMessage(Action<ILogger> logAction)
    {
        var extractor = new MessageExtractingLogger();
        logAction(extractor);
        return extractor.Message;
    }

    /// <summary>
    ///     Logs the event, or throws if the event has been configured to be treated as an error.
    /// </summary>
    /// <typeparam name="TLoggerCategory">The <see cref="DbLoggerCategory" />.</typeparam>
    /// <param name="logger">The logger to which the event should be logged.</param>
    /// <param name="logAction">A delegate that will log the message to an <see cref="ILogger" />.</param>
    public virtual void Log<TLoggerCategory>(
        IDiagnosticsLogger<TLoggerCategory> logger,
        Action<ILogger> logAction)
        where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
    {
        switch (WarningBehavior)
        {
            case WarningBehavior.Log:
                logAction(logger.Logger);
                break;
            case WarningBehavior.Throw:
                throw WarningAsError(GenerateMessage(logAction));
        }
    }

    /// <summary>
    ///     The parameterized message definition.
    /// </summary>
    public virtual string MessageFormat { get; }
}
