// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosLoggerExtensions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void SyncNotSupported(
        this IDiagnosticsLogger<DbLoggerCategory.Database> diagnostics)
    {
        var definition = CosmosResources.LogSyncNotSupported(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            definition.Log(diagnostics);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new EventData(
                definition,
                (d, p) => ((EventDefinition)d).GenerateMessage());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ExecutingSqlQuery(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
        string containerId,
        string? partitionKey,
        CosmosSqlQuery cosmosSqlQuery)
    {
        var definition = CosmosResources.LogExecutingSqlQuery(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var logSensitiveData = diagnostics.ShouldLogSensitiveData();

            definition.Log(
                diagnostics,
                containerId,
                logSensitiveData ? partitionKey : "?",
                FormatParameters(cosmosSqlQuery.Parameters, logSensitiveData && cosmosSqlQuery.Parameters.Count > 0),
                Environment.NewLine,
                cosmosSqlQuery.Query);
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CosmosQueryEventData(
                definition,
                ExecutingSqlQuery,
                containerId,
                partitionKey,
                cosmosSqlQuery.Parameters.Select(p => (p.Name, p.Value)).ToList(),
                cosmosSqlQuery.Query,
                diagnostics.ShouldLogSensitiveData());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecutingSqlQuery(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string?, string, string, string>)definition;
        var p = (CosmosQueryEventData)payload;
        return d.GenerateMessage(
            p.ContainerId,
            p.LogSensitiveData ? p.PartitionKey : "?",
            FormatParameters(p.Parameters, p is { LogSensitiveData: true, Parameters.Count: > 0 }),
            Environment.NewLine,
            p.QuerySql);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ExecutingReadItem(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
        string containerId,
        string? partitionKey,
        string resourceId)
    {
        var definition = CosmosResources.LogExecutingReadItem(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var logSensitiveData = diagnostics.ShouldLogSensitiveData();
            definition.Log(diagnostics, logSensitiveData ? resourceId : "?", containerId, logSensitiveData ? partitionKey : "?");
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CosmosReadItemEventData(
                definition,
                ExecutingReadItem,
                resourceId,
                containerId,
                partitionKey,
                diagnostics.ShouldLogSensitiveData());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecutingReadItem(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string?>)definition;
        var p = (CosmosReadItemEventData)payload;
        return d.GenerateMessage(p.LogSensitiveData ? p.ResourceId : "?", p.ContainerId, p.LogSensitiveData ? p.PartitionKey : "?");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ExecutedReadNext(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
        TimeSpan elapsed,
        double requestCharge,
        string activityId,
        string containerId,
        string? partitionKey,
        CosmosSqlQuery cosmosSqlQuery)
    {
        var definition = CosmosResources.LogExecutedReadNext(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var logSensitiveData = diagnostics.ShouldLogSensitiveData();

            definition.Log(
                diagnostics,
                l => l.Log(
                    definition.Level,
                    definition.EventId,
                    definition.MessageFormat,
                    elapsed.TotalMilliseconds,
                    requestCharge,
                    activityId,
                    containerId,
                    logSensitiveData ? partitionKey : "?",
                    FormatParameters(cosmosSqlQuery.Parameters, logSensitiveData && cosmosSqlQuery.Parameters.Count > 0),
                    Environment.NewLine,
                    cosmosSqlQuery.Query));
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CosmosQueryExecutedEventData(
                definition,
                ExecutedReadNext,
                elapsed,
                requestCharge,
                activityId,
                containerId,
                partitionKey,
                cosmosSqlQuery.Parameters.Select(p => (p.Name, p.Value)).ToList(),
                cosmosSqlQuery.Query,
                diagnostics.ShouldLogSensitiveData());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecutedReadNext(EventDefinitionBase definition, EventData payload)
    {
        var d = (FallbackEventDefinition)definition;
        var p = (CosmosQueryExecutedEventData)payload;
        return d.GenerateMessage(
            l => l.Log(
                d.Level,
                d.EventId,
                d.MessageFormat,
                p.Elapsed.TotalMilliseconds,
                p.RequestCharge,
                p.ActivityId,
                p.ContainerId,
                p.LogSensitiveData ? p.PartitionKey : "?",
                FormatParameters(p.Parameters, p is { LogSensitiveData: true, Parameters.Count: > 0 }),
                Environment.NewLine,
                p.QuerySql));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ExecutedReadItem(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
        TimeSpan elapsed,
        double requestCharge,
        string activityId,
        string resourceId,
        string containerId,
        string? partitionKey)
    {
        var definition = CosmosResources.LogExecutedReadItem(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var logSensitiveData = diagnostics.ShouldLogSensitiveData();
            definition.Log(
                diagnostics,
                elapsed.TotalMilliseconds.ToString(),
                requestCharge.ToString(),
                activityId,
                containerId,
                logSensitiveData ? resourceId : "?",
                logSensitiveData ? partitionKey : "?");
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CosmosItemCommandExecutedEventData(
                definition,
                ExecutedReadItem,
                elapsed,
                requestCharge,
                activityId,
                containerId,
                resourceId,
                partitionKey,
                diagnostics.ShouldLogSensitiveData());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecutedReadItem(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string, string, string?>)definition;
        var p = (CosmosItemCommandExecutedEventData)payload;
        return d.GenerateMessage(
            p.Elapsed.Milliseconds.ToString(),
            p.RequestCharge.ToString(),
            p.ActivityId,
            p.ContainerId,
            p.LogSensitiveData ? p.ResourceId : "?",
            p.LogSensitiveData ? p.PartitionKey : "?");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ExecutedCreateItem(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
        TimeSpan elapsed,
        double requestCharge,
        string activityId,
        string resourceId,
        string containerId,
        string? partitionKey)
    {
        var definition = CosmosResources.LogExecutedCreateItem(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var logSensitiveData = diagnostics.ShouldLogSensitiveData();
            definition.Log(
                diagnostics,
                elapsed.TotalMilliseconds.ToString(),
                requestCharge.ToString(),
                activityId,
                containerId,
                logSensitiveData ? resourceId : "?",
                logSensitiveData ? partitionKey : "?");
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CosmosItemCommandExecutedEventData(
                definition,
                ExecutedCreateItem,
                elapsed,
                requestCharge,
                activityId,
                containerId,
                resourceId,
                partitionKey,
                diagnostics.ShouldLogSensitiveData());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecutedCreateItem(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string, string, string?>)definition;
        var p = (CosmosItemCommandExecutedEventData)payload;
        return d.GenerateMessage(
            p.Elapsed.Milliseconds.ToString(),
            p.RequestCharge.ToString(),
            p.ActivityId,
            p.ContainerId,
            p.LogSensitiveData ? p.ResourceId : "?",
            p.LogSensitiveData ? p.PartitionKey : "?");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ExecutedDeleteItem(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
        TimeSpan elapsed,
        double requestCharge,
        string activityId,
        string resourceId,
        string containerId,
        string? partitionKey)
    {
        var definition = CosmosResources.LogExecutedDeleteItem(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var logSensitiveData = diagnostics.ShouldLogSensitiveData();
            definition.Log(
                diagnostics,
                elapsed.TotalMilliseconds.ToString(),
                requestCharge.ToString(),
                activityId,
                containerId,
                logSensitiveData ? resourceId : "?",
                logSensitiveData ? partitionKey : "?");
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CosmosItemCommandExecutedEventData(
                definition,
                ExecutedDeleteItem,
                elapsed,
                requestCharge,
                activityId,
                containerId,
                resourceId,
                partitionKey,
                diagnostics.ShouldLogSensitiveData());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecutedDeleteItem(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string, string, string?>)definition;
        var p = (CosmosItemCommandExecutedEventData)payload;
        return d.GenerateMessage(
            p.Elapsed.Milliseconds.ToString(),
            p.RequestCharge.ToString(),
            p.ActivityId,
            p.ContainerId,
            p.LogSensitiveData ? p.ResourceId : "?",
            p.LogSensitiveData ? p.PartitionKey : "?");
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void ExecutedReplaceItem(
        this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
        TimeSpan elapsed,
        double requestCharge,
        string activityId,
        string resourceId,
        string containerId,
        string? partitionKey)
    {
        var definition = CosmosResources.LogExecutedReplaceItem(diagnostics);

        if (diagnostics.ShouldLog(definition))
        {
            var logSensitiveData = diagnostics.ShouldLogSensitiveData();
            definition.Log(
                diagnostics,
                elapsed.TotalMilliseconds.ToString(),
                requestCharge.ToString(),
                activityId,
                containerId,
                logSensitiveData ? resourceId : "?",
                logSensitiveData ? partitionKey : "?");
        }

        if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
        {
            var eventData = new CosmosItemCommandExecutedEventData(
                definition,
                ExecutedReplaceItem,
                elapsed,
                requestCharge,
                activityId,
                containerId,
                resourceId,
                partitionKey,
                diagnostics.ShouldLogSensitiveData());

            diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
        }
    }

    private static string ExecutedReplaceItem(EventDefinitionBase definition, EventData payload)
    {
        var d = (EventDefinition<string, string, string, string, string, string?>)definition;
        var p = (CosmosItemCommandExecutedEventData)payload;
        return d.GenerateMessage(
            p.Elapsed.Milliseconds.ToString(),
            p.RequestCharge.ToString(),
            p.ActivityId,
            p.ContainerId,
            p.LogSensitiveData ? p.ResourceId : "?",
            p.LogSensitiveData ? p.PartitionKey : "?");
    }

    private static string FormatParameters(IReadOnlyList<(string Name, object? Value)> parameters, bool shouldLogParameterValues)
        => FormatParameters(parameters.Select(p => new SqlParameter(p.Name, p.Value)).ToList(), shouldLogParameterValues);

    private static string FormatParameters(IReadOnlyList<SqlParameter> parameters, bool shouldLogParameterValues)
        => parameters.Count == 0
            ? ""
            : string.Join(", ", parameters.Select(e => FormatParameter(e, shouldLogParameterValues)));

    private static string FormatParameter(SqlParameter parameter, bool shouldLogParameterValue)
    {
        var builder = new StringBuilder();
        builder
            .Append(parameter.Name)
            .Append('=');

        if (shouldLogParameterValue)
        {
            FormatParameterValue(builder, parameter.Value);
        }
        else
        {
            builder.Append('?');
        }

        return builder.ToString();
    }

    private static void FormatParameterValue(StringBuilder builder, object? parameterValue)
    {
        if (parameterValue == null)
        {
            builder.Append("null");
            return;
        }

        builder.Append('\'');

        switch (parameterValue)
        {
            case JToken jTokenValue:
                builder.Append(jTokenValue.ToString(Formatting.None).Trim('"'));
                break;
            case DateTime dateTimeValue:
                builder.Append(dateTimeValue.ToString("s"));
                break;
            case DateTimeOffset dateTimeOffsetValue:
                builder.Append(dateTimeOffsetValue.ToString("o"));
                break;
            case byte[] binaryValue:
                builder.AppendBytes(binaryValue);
                break;
            default:
                builder.Append(Convert.ToString(parameterValue, CultureInfo.InvariantCulture));
                break;
        }

        builder.Append('\'');
    }
}
