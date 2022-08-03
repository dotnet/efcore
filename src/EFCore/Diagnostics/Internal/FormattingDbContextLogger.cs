// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class FormattingDbContextLogger : IDbContextLogger
{
    private readonly Action<string> _sink;
    private readonly Func<EventId, LogLevel, bool> _filter;
    private readonly DbContextLoggerOptions _options;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public FormattingDbContextLogger(
        Action<string> sink,
        Func<EventId, LogLevel, bool> filter,
        DbContextLoggerOptions options)
    {
        _sink = sink;
        _filter = filter;
        _options = options;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Log(EventData eventData)
    {
        var message = eventData.ToString();
        var logLevel = eventData.LogLevel;
        var eventId = eventData.EventId;

        if (_options != DbContextLoggerOptions.None)
        {
            var messageBuilder = new StringBuilder();

            if ((_options & DbContextLoggerOptions.Level) != 0)
            {
                messageBuilder.Append(GetLogLevelString(logLevel));
            }

            if ((_options & DbContextLoggerOptions.LocalTime) != 0)
            {
                messageBuilder.Append(DateTime.Now.ToShortDateString()).Append(DateTime.Now.ToString(" HH:mm:ss.fff "));
            }

            if ((_options & DbContextLoggerOptions.UtcTime) != 0)
            {
                messageBuilder.Append(DateTime.UtcNow.ToString("o")).Append(' ');
            }

            if ((_options & DbContextLoggerOptions.Id) != 0)
            {
                messageBuilder.Append(eventData.EventIdCode).Append('[').Append(eventId.Id).Append("] ");
            }

            if ((_options & DbContextLoggerOptions.Category) != 0)
            {
                var lastDot = eventId.Name!.LastIndexOf('.');
                if (lastDot > 0)
                {
                    messageBuilder.Append('(').Append(eventId.Name[..lastDot]).Append(") ");
                }
            }

            const string padding = "      ";
            var preambleLength = messageBuilder.Length;

            if (_options == DbContextLoggerOptions.SingleLine) // Single line ONLY
            {
                message = messageBuilder
                    .Append(message)
                    .Replace(Environment.NewLine, "")
                    .ToString();
            }
            else
            {
                message = (_options & DbContextLoggerOptions.SingleLine) != 0
                    ? messageBuilder
                        .Append("-> ")
                        .Append(message)
                        .Replace(Environment.NewLine, "", preambleLength, messageBuilder.Length - preambleLength)
                        .ToString()
                    : messageBuilder
                        .AppendLine()
                        .Append(message)
                        .Replace(
                            Environment.NewLine, Environment.NewLine + padding, preambleLength, messageBuilder.Length - preambleLength)
                        .ToString();
            }
        }

        _sink(message);
    }

    /// <inheritdoc />
    public virtual bool ShouldLog(EventId eventId, LogLevel logLevel)
        => _filter(eventId, logLevel);

    private static string GetLogLevelString(LogLevel logLevel)
        => logLevel switch
        {
            LogLevel.Trace => "trce: ",
            LogLevel.Debug => "dbug: ",
            LogLevel.Information => "info: ",
            LogLevel.Warning => "warn: ",
            LogLevel.Error => "fail: ",
            LogLevel.Critical => "crit: ",
            _ => "none"
        };
}
