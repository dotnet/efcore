// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class OperationLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IOperationReporter _reporter;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public OperationLogger(string categoryName, IOperationReporter reporter)
    {
        _categoryName = categoryName;
        _reporter = reporter;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsEnabled(LogLevel logLevel)
        => true;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IDisposable BeginScope<TState>(TState state)
        where TState : notnull
        => NullScope.Instance;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        // Only show SQL when verbose
        if (_categoryName == DbLoggerCategory.Database.Command.Name
            && eventId.Id == RelationalEventId.CommandExecuted.Id)
        {
            logLevel = LogLevel.Debug;
        }

        var message = GetMessage(state, exception, formatter);
        switch (logLevel)
        {
            case LogLevel.Critical:
            case LogLevel.Error:
                _reporter.WriteError(message);
                break;

            case LogLevel.Warning:
                _reporter.WriteWarning(message);
                break;

            case LogLevel.Information:
                _reporter.WriteInformation(message);
                break;

            default:
                _reporter.WriteVerbose(message);
                break;
        }
    }

    private static string GetMessage<TState>(TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
    {
        var builder = new StringBuilder();
        if (formatter != null)
        {
            builder.Append(formatter(state, exception));
        }
        else if (state != null)
        {
            builder.Append(state);

            if (exception != null)
            {
                builder
                    .AppendLine()
                    .Append(exception);
            }
        }

        return builder.ToString();
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        private NullScope()
        {
        }

        public void Dispose()
        {
        }
    }
}
