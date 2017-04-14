// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Remotion.Linq;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class CoreLoggerExtensions
    {
        private const int QueryModelStringLengthLimit = 100;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SaveChangesFailed(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Update> diagnostics,
            [NotNull] Type contextType,
            [NotNull] Exception exception)
        {
            var eventId = CoreEventId.SaveChangesFailed;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Error))
            {
                diagnostics.Logger.Log(
                    LogLevel.Error,
                    eventId,
                    new DatabaseErrorLogState(contextType),
                    exception,
                    (_, e) => CoreStrings.LogExceptionDuringSaveChanges(Environment.NewLine, e));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        ContextType = contextType,
                        Exception = exception
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryIterationFailed(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] Type contextType,
            [NotNull] Exception exception)
        {
            var eventId = CoreEventId.QueryIterationFailed;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Error))
            {
                diagnostics.Logger.Log(
                    LogLevel.Error,
                    eventId,
                    new DatabaseErrorLogState(contextType),
                    exception,
                    (_, e) => CoreStrings.LogExceptionDuringQueryIteration(Environment.NewLine, e));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        ContextType = contextType,
                        Exception = exception
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryModelCompiling(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var eventId = CoreEventId.QueryModelCompiling;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    CoreStrings.LogCompilingQueryModel(Environment.NewLine, queryModel.Print()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        QueryModel = queryModel
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RowLimitingOperationWithoutOrderByWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var eventId = CoreEventId.RowLimitingOperationWithoutOrderByWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    CoreStrings.RowLimitingOperationWithoutOrderBy(
                        queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        QueryModel = queryModel
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void FirstWithoutOrderByAndFilterWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var eventId = CoreEventId.FirstWithoutOrderByAndFilterWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    CoreStrings.FirstWithoutOrderByAndFilter(
                        queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        QueryModel = queryModel
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryModelOptimized(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var eventId = CoreEventId.QueryModelOptimized;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    CoreStrings.LogOptimizedQueryModel(Environment.NewLine, queryModel.Print()));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        QueryModel = queryModel
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NavigationIncluded(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] string includeSpecification)
        {
            var eventId = CoreEventId.NavigationIncluded;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    CoreStrings.LogIncludingNavigation(includeSpecification));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        IncludeSpecification = includeSpecification
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryExecutionPlanned(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] IExpressionPrinter expressionPrinter,
            [NotNull] Expression queryExecutorExpression)
        {
            var eventId = CoreEventId.QueryExecutionPlanned;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    expressionPrinter.Print(queryExecutorExpression));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        QueryExecutorExpression = queryExecutorExpression
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SensitiveDataLoggingEnabledWarning<TLoggerCategory>(
            [NotNull] this IDiagnosticsLogger<TLoggerCategory> diagnostics)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
        {
            var eventId = CoreEventId.SensitiveDataLoggingEnabledWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    CoreStrings.SensitiveDataLoggingEnabled);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    null);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IncludeIgnoredWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Query> diagnostics,
            [NotNull] string includeSpecification)
        {
            var eventId = CoreEventId.IncludeIgnoredWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    CoreStrings.LogIgnoredInclude(includeSpecification));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        IncludeSpecification = includeSpecification
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ModelValidationShadowKeyWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Model.Validation> diagnostics,
            [NotNull] IEntityType entityType,
            [NotNull] IKey key)
        {
            var eventId = CoreEventId.ModelValidationShadowKeyWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    CoreStrings.ShadowKey(
                        Property.Format(key.Properties),
                        entityType.DisplayName(),
                        Property.Format(key.Properties)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        EntityType = entityType,
                        Key = key
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ServiceProviderCreated(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Infrastructure> diagnostics,
            [NotNull] IServiceProvider serviceProvider)
        {
            var eventId = CoreEventId.ServiceProviderCreated;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Debug))
            {
                diagnostics.Logger.LogDebug(
                    eventId,
                    CoreStrings.ServiceProviderCreated);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        IServiceProvider = serviceProvider
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ManyServiceProvidersCreatedWarning(
            [NotNull] this IDiagnosticsLogger<LoggerCategory.Infrastructure> diagnostics,
            [NotNull] ICollection<IServiceProvider> serviceProviders)
        {
            var eventId = CoreEventId.ManyServiceProvidersCreatedWarning;

            if (diagnostics.Logger.IsEnabled(eventId, LogLevel.Warning))
            {
                diagnostics.Logger.LogWarning(
                    eventId,
                    CoreStrings.ManyServiceProvidersCreated);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(eventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    eventId.Name,
                    new
                    {
                        IServiceProviders = serviceProviders
                    });
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogDebug<TLoggerCategory>(
            [NotNull] this IInterceptingLogger<TLoggerCategory> logger, EventId eventId, [NotNull] string message)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
            => logger.Log<object>(LogLevel.Debug, eventId, null, null, (_, __) => message);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogWarning<TLoggerCategory>(
            [NotNull] this IInterceptingLogger<TLoggerCategory> logger, EventId eventId, [NotNull] string message)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
            => logger.Log<object>(LogLevel.Warning, eventId, null, null, (_, __) => message);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogError<TLoggerCategory>(
            [NotNull] this IInterceptingLogger<TLoggerCategory> logger, EventId eventId, [NotNull] Exception exception, [NotNull] string message)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
            => logger.Log<object>(LogLevel.Error, eventId, null, exception, (_, __) => message);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LogInformation<TLoggerCategory>(
            [NotNull] this IInterceptingLogger<TLoggerCategory> logger, EventId eventId, [NotNull] string message)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
            => logger.Log<object>(LogLevel.Information, eventId, null, null, (_, __) => message);
    }
}
