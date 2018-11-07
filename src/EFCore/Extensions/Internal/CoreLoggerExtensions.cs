// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
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
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context,
            [NotNull] Exception exception)
        {
            var definition = CoreStrings.LogExceptionDuringSaveChanges;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    context.GetType(), Environment.NewLine, exception,
                    exception);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextErrorEventData(
                        definition,
                        SaveChangesFailed,
                        context,
                        exception));
            }
        }

        private static string SaveChangesFailed(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<Type, string, Exception>)definition;
            var p = (DbContextErrorEventData)payload;
            return d.GenerateMessage(p.Context.GetType(), Environment.NewLine, p.Exception);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void OptimisticConcurrencyException(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context,
            [NotNull] Exception exception)
        {
            var definition = CoreStrings.LogOptimisticConcurrencyException;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    exception);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextErrorEventData(
                        definition,
                        OptimisticConcurrencyException,
                        context,
                        exception));
            }
        }

        private static string OptimisticConcurrencyException(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<Exception>)definition;
            var p = (DbContextErrorEventData)payload;
            return d.GenerateMessage(p.Exception);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DuplicateDependentEntityTypeInstanceWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] IEntityType dependent1,
            [NotNull] IEntityType dependent2)
        {
            var definition = CoreStrings.LogDuplicateDependentEntityTypeInstance;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    dependent1.DisplayName(), dependent2.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new SharedDependentEntityEventData(
                        definition,
                        DuplicateDependentEntityTypeInstanceWarning,
                        dependent1,
                        dependent2));
            }
        }

        private static string DuplicateDependentEntityTypeInstanceWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (SharedDependentEntityEventData)payload;
            return d.GenerateMessage(p.FirstEntityType.DisplayName(), p.SecondEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryIterationFailed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] Type contextType,
            [NotNull] Exception exception)
        {
            var definition = CoreStrings.LogExceptionDuringQueryIteration;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    contextType, Environment.NewLine, exception,
                    exception);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextTypeErrorEventData(
                        definition,
                        QueryIterationFailed,
                        contextType,
                        exception));
            }
        }

        private static string QueryIterationFailed(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<Type, string, Exception>)definition;
            var p = (DbContextTypeErrorEventData)payload;
            return d.GenerateMessage(p.ContextType, Environment.NewLine, p.Exception);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryModelCompiling(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var definition = CoreStrings.LogCompilingQueryModel;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Environment.NewLine, queryModel.Print());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelEventData(
                        definition,
                        QueryModelCompiling,
                        queryModel));
            }
        }

        private static string QueryModelCompiling(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (QueryModelEventData)payload;
            return d.GenerateMessage(Environment.NewLine, p.QueryModel.Print());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RowLimitingOperationWithoutOrderByWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var definition = CoreStrings.LogRowLimitingOperationWithoutOrderBy;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelEventData(
                        definition,
                        RowLimitingOperationWithoutOrderByWarning,
                        queryModel));
            }
        }

        private static string RowLimitingOperationWithoutOrderByWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (QueryModelEventData)payload;
            return d.GenerateMessage(p.QueryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void FirstWithoutOrderByAndFilterWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var definition = CoreStrings.LogFirstWithoutOrderByAndFilter;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelEventData(
                        definition,
                        FirstWithoutOrderByAndFilterWarning,
                        queryModel));
            }
        }

        private static string FirstWithoutOrderByAndFilterWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (QueryModelEventData)payload;
            return d.GenerateMessage(p.QueryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryModelOptimized(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] QueryModel queryModel)
        {
            var definition = CoreStrings.LogOptimizedQueryModel;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Environment.NewLine, queryModel.Print());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryModelEventData(
                        definition,
                        QueryModelOptimized,
                        queryModel));
            }
        }

        private static string QueryModelOptimized(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (QueryModelEventData)payload;
            return d.GenerateMessage(Environment.NewLine, p.QueryModel.Print());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NavigationIncluded(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] IncludeResultOperator includeResultOperator)
        {
            var definition = CoreStrings.LogIncludingNavigation;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    includeResultOperator.DisplayString());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new IncludeEventData(
                        definition,
                        NavigationIncluded,
                        includeResultOperator));
            }
        }

        private static string NavigationIncluded(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (IncludeEventData)payload;
            return d.GenerateMessage(p.IncludeResultOperator.DisplayString());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void QueryExecutionPlanned(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] IExpressionPrinter expressionPrinter,
            [NotNull] Expression queryExecutorExpression)
        {
            var definition = CoreStrings.LogQueryExecutionPlanned;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    expressionPrinter.Print(queryExecutorExpression));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new QueryExpressionEventData(
                        definition,
                        QueryExecutionPlanned,
                        queryExecutorExpression,
                        expressionPrinter));
            }
        }

        private static string QueryExecutionPlanned(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (QueryExpressionEventData)payload;
            return d.GenerateMessage(p.ExpressionPrinter.Print(p.Expression));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SensitiveDataLoggingEnabledWarning<TLoggerCategory>(
            [NotNull] this IDiagnosticsLogger<TLoggerCategory> diagnostics)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
        {
            var definition = CoreStrings.LogSensitiveDataLoggingEnabled;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, warningBehavior);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new EventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage()));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IncludeIgnoredWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] IncludeResultOperator includeResultOperator)
        {
            var definition = CoreStrings.LogIgnoredInclude;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    includeResultOperator.DisplayString());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new IncludeEventData(
                        definition,
                        IncludeIgnoredWarning,
                        includeResultOperator));
            }
        }

        private static string IncludeIgnoredWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (IncludeEventData)payload;
            return d.GenerateMessage(p.IncludeResultOperator.DisplayString());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PossibleUnintendedCollectionNavigationNullComparisonWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] IReadOnlyList<IPropertyBase> navigationPath)
        {
            var definition = CoreStrings.LogPossibleUnintendedCollectionNavigationNullComparison;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    string.Join(".", navigationPath.Select(p => p.Name)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new NavigationPathEventData(
                        definition,
                        PossibleUnintendedCollectionNavigationNullComparisonWarning,
                        navigationPath));
            }
        }

        private static string PossibleUnintendedCollectionNavigationNullComparisonWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (NavigationPathEventData)payload;
            return d.GenerateMessage(string.Join(".", p.NavigationPath.Select(pb => pb.Name)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PossibleUnintendedReferenceComparisonWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] Expression left,
            [NotNull] Expression right)
        {
            var definition = CoreStrings.LogPossibleUnintendedReferenceComparison;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    left, right);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new BinaryExpressionEventData(
                        definition,
                        PossibleUnintendedReferenceComparisonWarning,
                        left,
                        right));
            }
        }

        private static string PossibleUnintendedReferenceComparisonWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<object, object>)definition;
            var p = (BinaryExpressionEventData)payload;
            return d.GenerateMessage(p.Left, p.Right);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ServiceProviderCreated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] IServiceProvider serviceProvider)
        {
            var definition = CoreStrings.LogServiceProviderCreated;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, warningBehavior);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ServiceProviderEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        serviceProvider));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ManyServiceProvidersCreatedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] ICollection<IServiceProvider> serviceProviders)
        {
            var definition = CoreStrings.LogManyServiceProvidersCreated;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(diagnostics, warningBehavior);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ServiceProvidersEventData(
                        definition,
                        (d, p) => ((EventDefinition)d).GenerateMessage(),
                        serviceProviders));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ServiceProviderDebugInfo(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] IDictionary<string, string> newDebugInfo,
            [NotNull] IList<IDictionary<string, string>> cachedDebugInfos)
        {
            var definition = CoreStrings.LogServiceProviderDebugInfo;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    GenerateDebugInfoString(newDebugInfo, cachedDebugInfos));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ServiceProviderDebugInfoEventData(
                        definition,
                        (d, p) => ServiceProviderDebugInfo(d, p),
                        newDebugInfo,
                        cachedDebugInfos));
            }
        }

        private static string ServiceProviderDebugInfo(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (ServiceProviderDebugInfoEventData)payload;
            return d.GenerateMessage(
                GenerateDebugInfoString(p.NewDebugInfo, p.CachedDebugInfos));
        }

        private static string GenerateDebugInfoString(
            IDictionary<string, string> newDebugInfo,
            IList<IDictionary<string, string>> cachedDebugInfos)
        {
            List<string> leastConflicts = null;

            foreach (var cachedDebugInfo in cachedDebugInfos)
            {
                var newKeys = new HashSet<string>(newDebugInfo.Keys);

                var conflicts = new List<string>();
                foreach (var key in cachedDebugInfo.Keys)
                {
                    if (newDebugInfo.TryGetValue(key, out var value))
                    {
                        if (!value.Equals(cachedDebugInfo[key]))
                        {
                            conflicts.Add(CoreStrings.ServiceProviderConfigChanged(key));
                        }
                    }
                    else
                    {
                        conflicts.Add(CoreStrings.ServiceProviderConfigRemoved(key));
                    }

                    newKeys.Remove(key);
                }

                foreach (var addedKey in newKeys)
                {
                    conflicts.Add(CoreStrings.ServiceProviderConfigAdded(addedKey));
                }

                if (leastConflicts == null
                    || leastConflicts.Count > conflicts.Count)
                {
                    leastConflicts = conflicts;
                }
            }

            return string.Join(", ", leastConflicts);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ContextInitialized(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] DbContextOptions contextOptions)
        {
            var definition = CoreStrings.LogContextInitialized;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    ProductInfo.GetVersion(),
                    context.GetType().ShortDisplayName(),
                    context.Database.ProviderName,
                    contextOptions.BuildOptionsFragment());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ContextInitializedEventData(
                        definition,
                        ContextInitialized,
                        context,
                        contextOptions));
            }
        }

        private static string ContextInitialized(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (ContextInitializedEventData)payload;
            return d.GenerateMessage(
                ProductInfo.GetVersion(),
                p.Context.GetType().ShortDisplayName(),
                p.Context.Database.ProviderName,
                p.ContextOptions.BuildOptionsFragment());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ExecutionStrategyRetrying(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] IReadOnlyList<Exception> exceptionsEncountered,
            TimeSpan delay,
            bool async)
        {
            var definition = CoreStrings.LogExecutionStrategyRetrying;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                var lastException = exceptionsEncountered[exceptionsEncountered.Count - 1];
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    (int)delay.TotalMilliseconds, Environment.NewLine, lastException,
                    lastException);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ExecutionStrategyEventData(
                        definition,
                        ExecutionStrategyRetrying,
                        exceptionsEncountered,
                        delay,
                        async));
            }
        }

        private static string ExecutionStrategyRetrying(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<int, string, Exception>)definition;
            var p = (ExecutionStrategyEventData)payload;
            return d.GenerateMessage(
                (int)p.Delay.TotalMilliseconds, Environment.NewLine, p.ExceptionsEncountered[p.ExceptionsEncountered.Count - 1]);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void LazyLoadOnDisposedContextWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] object entityType,
            [NotNull] string navigationName)
        {
            var definition = CoreStrings.LogLazyLoadOnDisposedContext;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigationName, entityType.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new LazyLoadingEventData(
                        definition,
                        LazyLoadOnDisposedContextWarning,
                        context,
                        entityType,
                        navigationName));
            }
        }

        private static string LazyLoadOnDisposedContextWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (LazyLoadingEventData)payload;
            return d.GenerateMessage(p.NavigationPropertyName, p.Entity.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NavigationLazyLoading(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] object entityType,
            [NotNull] string navigationName)
        {
            var definition = CoreStrings.LogNavigationLazyLoading;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigationName, entityType.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new LazyLoadingEventData(
                        definition,
                        NavigationLazyLoading,
                        context,
                        entityType,
                        navigationName));
            }
        }

        private static string NavigationLazyLoading(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (LazyLoadingEventData)payload;
            return d.GenerateMessage(p.NavigationPropertyName, p.Entity.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DetachedLazyLoadingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] object entityType,
            [NotNull] string navigationName)
        {
            var definition = CoreStrings.LogDetachedLazyLoading;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigationName, entityType.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new LazyLoadingEventData(
                        definition,
                        DetachedLazyLoadingWarning,
                        context,
                        entityType,
                        navigationName));
            }
        }

        private static string DetachedLazyLoadingWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (LazyLoadingEventData)payload;
            return d.GenerateMessage(p.NavigationPropertyName, p.Entity.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ShadowPropertyCreated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = CoreStrings.LogShadowPropertyCreated;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    property.Name, property.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyEventData(
                        definition,
                        ShadowPropertyCreated,
                        property));
            }
        }

        private static string ShadowPropertyCreated(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(p.Property.Name, p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RedundantIndexRemoved(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IReadOnlyList<IPropertyBase> redundantIndex,
            [NotNull] IReadOnlyList<IPropertyBase> otherIndex)
        {
            var definition = CoreStrings.LogRedundantIndexRemoved;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Property.Format(redundantIndex), redundantIndex.First().DeclaringType.DisplayName(), Property.Format(otherIndex));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        RedundantIndexRemoved,
                        redundantIndex,
                        otherIndex));
            }
        }

        private static string RedundantIndexRemoved(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            return d.GenerateMessage(
                Property.Format(p.FirstPropertyCollection),
                p.FirstPropertyCollection.First().DeclaringType.DisplayName(),
                Property.Format(p.SecondPropertyCollection));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RedundantForeignKeyWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IForeignKey redundantForeignKey)
        {
            var definition = CoreStrings.LogRedundantForeignKey;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Property.Format(redundantForeignKey.Properties), redundantForeignKey.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ForeignKeyEventData(
                        definition,
                        RedundantForeignKeyWarning,
                        redundantForeignKey));
            }
        }

        private static string RedundantForeignKeyWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (ForeignKeyEventData)payload;
            return d.GenerateMessage(
                Property.Format(p.ForeignKey.Properties),
                p.ForeignKey.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void IncompatibleMatchingForeignKeyProperties(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IReadOnlyList<IPropertyBase> foreignKeyProperties,
            [NotNull] IReadOnlyList<IPropertyBase> principalKeyProperties)
        {
            var definition = CoreStrings.LogIncompatibleMatchingForeignKeyProperties;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    Property.Format(foreignKeyProperties, includeTypes: true),
                    Property.Format(principalKeyProperties, includeTypes: true));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        IncompatibleMatchingForeignKeyProperties,
                        foreignKeyProperties,
                        principalKeyProperties));
            }
        }

        private static string IncompatibleMatchingForeignKeyProperties(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            return d.GenerateMessage(
                Property.Format(p.FirstPropertyCollection, includeTypes: true),
                Property.Format(p.SecondPropertyCollection, includeTypes: true));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RequiredAttributeOnDependent(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation)
        {
            var definition = CoreStrings.LogRequiredAttributeOnDependent;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigation.Name, navigation.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new NavigationEventData(
                        definition,
                        RequiredAttributeOnDependent,
                        navigation));
            }
        }

        private static string RequiredAttributeOnDependent(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (NavigationEventData)payload;
            return d.GenerateMessage(p.Navigation.Name, p.Navigation.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void RequiredAttributeOnBothNavigations(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation)
        {
            var definition = CoreStrings.LogRequiredAttributeOnBothNavigations;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstNavigation.DeclaringEntityType.DisplayName(),
                    firstNavigation.Name,
                    secondNavigation.DeclaringEntityType.DisplayName(),
                    secondNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        RequiredAttributeOnBothNavigations,
                        new[] { firstNavigation },
                        new[] { secondNavigation }));
            }
        }

        private static string RequiredAttributeOnBothNavigations(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            var firstNavigation = p.FirstPropertyCollection[0];
            var secondNavigation = p.SecondPropertyCollection[0];
            return d.GenerateMessage(
                firstNavigation.DeclaringType.DisplayName(),
                firstNavigation.Name,
                secondNavigation.DeclaringType.DisplayName(),
                secondNavigation.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConflictingShadowForeignKeysWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IForeignKey foreignKey)
        {
            var definition = CoreStrings.LogConflictingShadowForeignKeys;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                var declaringTypeName = foreignKey.DeclaringEntityType.DisplayName();
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    declaringTypeName,
                    foreignKey.PrincipalEntityType.DisplayName(),
                    declaringTypeName);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ForeignKeyEventData(
                        definition,
                        ConflictingShadowForeignKeysWarning,
                        foreignKey));
            }
        }

        private static string ConflictingShadowForeignKeysWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string>)definition;
            var p = (ForeignKeyEventData)payload;
            return d.GenerateMessage(
                p.ForeignKey.DeclaringEntityType.DisplayName(),
                p.ForeignKey.PrincipalEntityType.DisplayName(),
                p.ForeignKey.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MultiplePrimaryKeyCandidates(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IProperty firstProperty,
            [NotNull] IProperty secondProperty)
        {
            var definition = CoreStrings.LogMultiplePrimaryKeyCandidates;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstProperty.Name,
                    secondProperty.Name,
                    firstProperty.DeclaringEntityType.DisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        MultiplePrimaryKeyCandidates,
                        new[] { firstProperty },
                        new[] { secondProperty }));
            }
        }

        private static string MultiplePrimaryKeyCandidates(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            var firstProperty = p.FirstPropertyCollection[0];
            var secondProperty = p.SecondPropertyCollection[0];
            return d.GenerateMessage(
                firstProperty.Name,
                secondProperty.Name,
                firstProperty.DeclaringType.DisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MultipleNavigationProperties(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> firstPropertyCollection,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> secondPropertyCollection)
        {
            var definition = CoreStrings.LogMultipleNavigationProperties;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstPropertyCollection.First().Item2.ShortDisplayName(),
                    secondPropertyCollection.First().Item2.ShortDisplayName(),
                    Property.Format(firstPropertyCollection.Select(p => p.Item1?.Name)),
                    Property.Format(secondPropertyCollection.Select(p => p.Item1?.Name)));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        MultipleNavigationProperties,
                        firstPropertyCollection,
                        secondPropertyCollection));
            }
        }

        private static string MultipleNavigationProperties(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            return d.GenerateMessage(
                p.FirstPropertyCollection.First().Item2.ShortDisplayName(),
                p.SecondPropertyCollection.First().Item2.ShortDisplayName(),
                Property.Format(p.FirstPropertyCollection.Select(i => i.Item1?.Name)),
                Property.Format(p.SecondPropertyCollection.Select(i => i.Item1?.Name)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void MultipleInversePropertiesSameTargetWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> conflictingNavigations,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] Type targetType)
        {
            var definition = CoreStrings.LogMultipleInversePropertiesSameTarget;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    string.Join(", ", conflictingNavigations.Select(n => n.Item2.ShortDisplayName() + "." + n.Item1.Name)),
                    inverseNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        MultipleInversePropertiesSameTargetWarning,
                        conflictingNavigations,
                        new[] { new Tuple<MemberInfo, Type>(inverseNavigation, targetType) }));
            }
        }

        private static string MultipleInversePropertiesSameTargetWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            return d.GenerateMessage(
                string.Join(
                    ", ", p.FirstPropertyCollection.Select(n => n.Item2.ShortDisplayName() + "." + n.Item1.Name)),
                p.SecondPropertyCollection.First().Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NonDefiningInverseNavigationWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEntityType declaringType,
            [NotNull] MemberInfo navigation,
            [NotNull] IEntityType targetType,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] MemberInfo definingNavigation)
        {
            var definition = CoreStrings.LogNonDefiningInverseNavigation;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    targetType.DisplayName(),
                    inverseNavigation.Name,
                    declaringType.DisplayName(),
                    navigation.Name,
                    definingNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        NonDefiningInverseNavigationWarning,
                        new[] { new Tuple<MemberInfo, Type>(navigation, declaringType.ClrType) },
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(inverseNavigation, targetType.ClrType),
                            new Tuple<MemberInfo, Type>(definingNavigation, targetType.ClrType)
                        }));
            }
        }

        private static string NonDefiningInverseNavigationWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            var navigation = p.FirstPropertyCollection.First();
            var inverseNavigation = p.SecondPropertyCollection.First();
            var definingNavigation = p.SecondPropertyCollection.Last();
            return d.GenerateMessage(
                inverseNavigation.Item2.ShortDisplayName(),
                inverseNavigation.Item1.Name,
                navigation.Item2.ShortDisplayName(),
                navigation.Item1.Name,
                definingNavigation.Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void NonOwnershipInverseNavigationWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEntityType declaringType,
            [NotNull] MemberInfo navigation,
            [NotNull] IEntityType targetType,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] MemberInfo ownershipNavigation)
        {
            var definition = CoreStrings.LogNonOwnershipInverseNavigation;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    targetType.DisplayName(),
                    inverseNavigation.Name,
                    declaringType.DisplayName(),
                    navigation.Name,
                    ownershipNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        NonOwnershipInverseNavigationWarning,
                        new[] { new Tuple<MemberInfo, Type>(navigation, declaringType.ClrType) },
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(inverseNavigation, targetType.ClrType),
                            new Tuple<MemberInfo, Type>(ownershipNavigation, targetType.ClrType)
                        }));
            }
        }

        private static string NonOwnershipInverseNavigationWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            var navigation = p.FirstPropertyCollection.First();
            var inverseNavigation = p.SecondPropertyCollection.First();
            var ownershipNavigation = p.SecondPropertyCollection.Last();
            return d.GenerateMessage(
                inverseNavigation.Item2.ShortDisplayName(),
                inverseNavigation.Item1.Name,
                navigation.Item2.ShortDisplayName(),
                navigation.Item1.Name,
                ownershipNavigation.Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyAttributesOnBothPropertiesWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation,
            [NotNull] MemberInfo firstProperty,
            [NotNull] MemberInfo secondProperty)
        {
            var definition = CoreStrings.LogForeignKeyAttributesOnBothProperties;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstNavigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                    firstNavigation.GetIdentifyingMemberInfo().Name,
                    secondNavigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                    secondNavigation.GetIdentifyingMemberInfo().Name,
                    firstProperty.Name,
                    secondProperty.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        ForeignKeyAttributesOnBothPropertiesWarning,
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(firstNavigation.GetIdentifyingMemberInfo(), firstNavigation.DeclaringEntityType.ClrType),
                            new Tuple<MemberInfo, Type>(firstProperty, firstNavigation.DeclaringEntityType.ClrType)
                        },
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(secondNavigation.GetIdentifyingMemberInfo(), secondNavigation.DeclaringEntityType.ClrType),
                            new Tuple<MemberInfo, Type>(secondProperty, secondNavigation.DeclaringEntityType.ClrType)
                        }));
            }
        }

        private static string ForeignKeyAttributesOnBothPropertiesWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            var firstNavigation = p.FirstPropertyCollection.First();
            var firstProperty = p.FirstPropertyCollection.Last();
            var secondNavigation = p.SecondPropertyCollection.First();
            var secondProperty = p.SecondPropertyCollection.Last();
            return d.GenerateMessage(
                firstNavigation.Item2.ShortDisplayName(),
                firstNavigation.Item1.Name,
                secondNavigation.Item2.ShortDisplayName(),
                secondNavigation.Item1.Name,
                firstProperty.Item1.Name,
                secondProperty.Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyAttributesOnBothNavigationsWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation)
        {
            var definition = CoreStrings.LogForeignKeyAttributesOnBothNavigations;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    firstNavigation.DeclaringEntityType.DisplayName(),
                    firstNavigation.Name,
                    secondNavigation.DeclaringEntityType.DisplayName(),
                    secondNavigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoPropertyBaseCollectionsEventData(
                        definition,
                        ForeignKeyAttributesOnBothNavigationsWarning,
                        new[] { firstNavigation },
                        new[] { secondNavigation }));
            }
        }

        private static string ForeignKeyAttributesOnBothNavigationsWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (TwoPropertyBaseCollectionsEventData)payload;
            var firstNavigation = p.FirstPropertyCollection[0];
            var secondNavigation = p.SecondPropertyCollection[0];
            return d.GenerateMessage(
                firstNavigation.DeclaringType.DisplayName(),
                firstNavigation.Name,
                secondNavigation.DeclaringType.DisplayName(),
                secondNavigation.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation,
            [NotNull] MemberInfo property)
        {
            var definition = CoreStrings.LogConflictingForeignKeyAttributesOnNavigationAndProperty;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigation.DeclaringEntityType.ClrType.ShortDisplayName(),
                    navigation.GetIdentifyingMemberInfo()?.Name,
                    property.DeclaringType.ShortDisplayName(),
                    property.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new TwoUnmappedPropertyCollectionsEventData(
                        definition,
                        ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning,
                        new[] { new Tuple<MemberInfo, Type>(navigation.GetIdentifyingMemberInfo(), navigation.DeclaringEntityType.ClrType) },
                        new[] { new Tuple<MemberInfo, Type>(property, property.DeclaringType) }));
            }
        }

        private static string ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, string>)definition;
            var p = (TwoUnmappedPropertyCollectionsEventData)payload;
            var navigation = p.FirstPropertyCollection.First();
            var property = p.SecondPropertyCollection.First();
            return d.GenerateMessage(
                navigation.Item2.ShortDisplayName(),
                navigation.Item1.Name,
                property.Item2.ShortDisplayName(),
                property.Item1.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DetectChangesStarting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] DbContext context)
        {
            var definition = CoreStrings.LogDetectChangesStarting;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    context.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextEventData(
                        definition,
                        DetectChangesStarting,
                        context));
            }
        }

        private static string DetectChangesStarting(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (DbContextEventData)payload;
            return d.GenerateMessage(p.Context.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void DetectChangesCompleted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] DbContext context)
        {
            var definition = CoreStrings.LogDetectChangesCompleted;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    context.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextEventData(
                        definition,
                        DetectChangesCompleted,
                        context));
            }
        }

        private static string DetectChangesCompleted(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (DbContextEventData)payload;
            return d.GenerateMessage(p.Context.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PropertyChangeDetected(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreStrings.LogPropertyChangeDetected;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    property.DeclaringEntityType.ShortName(),
                    property.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyChangedEventData(
                        definition,
                        PropertyChangeDetected,
                        new EntityEntry(internalEntityEntry),
                        property,
                        oldValue,
                        newValue));
            }
        }

        private static string PropertyChangeDetected(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyChangedEventData)payload;
            return d.GenerateMessage(
                p.Property.DeclaringEntityType.ShortName(),
                p.Property.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void PropertyChangeDetectedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreStrings.LogPropertyChangeDetectedSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    property.DeclaringEntityType.ShortName(),
                    property.Name,
                    oldValue,
                    newValue,
                    internalEntityEntry.BuildCurrentValuesString(property.DeclaringEntityType.FindPrimaryKey().Properties));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyChangedEventData(
                        definition,
                        PropertyChangeDetectedSensitive,
                        new EntityEntry(internalEntityEntry),
                        property,
                        oldValue,
                        newValue));
            }
        }

        private static string PropertyChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, object, object, string>)definition;
            var p = (PropertyChangedEventData)payload;
            return d.GenerateMessage(
                p.Property.DeclaringEntityType.ShortName(),
                p.Property.Name,
                p.OldValue,
                p.NewValue,
                p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.Property.DeclaringEntityType.FindPrimaryKey().Properties));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyChangeDetected(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreStrings.LogForeignKeyChangeDetected;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    property.DeclaringEntityType.ShortName(),
                    property.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyChangedEventData(
                        definition,
                        ForeignKeyChangeDetected,
                        new EntityEntry(internalEntityEntry),
                        property,
                        oldValue,
                        newValue));
            }
        }

        private static string ForeignKeyChangeDetected(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyChangedEventData)payload;
            return d.GenerateMessage(
                p.Property.DeclaringEntityType.ShortName(),
                p.Property.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ForeignKeyChangeDetectedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreStrings.LogForeignKeyChangeDetectedSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    property.DeclaringEntityType.ShortName(),
                    property.Name,
                    oldValue,
                    newValue,
                    internalEntityEntry.BuildCurrentValuesString(property.DeclaringEntityType.FindPrimaryKey().Properties));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyChangedEventData(
                        definition,
                        ForeignKeyChangeDetectedSensitive,
                        new EntityEntry(internalEntityEntry),
                        property,
                        oldValue,
                        newValue));
            }
        }

        private static string ForeignKeyChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, object, object, string>)definition;
            var p = (PropertyChangedEventData)payload;
            return d.GenerateMessage(
                p.Property.DeclaringEntityType.ShortName(),
                p.Property.Name,
                p.OldValue,
                p.NewValue,
                p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.Property.DeclaringEntityType.FindPrimaryKey().Properties));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CollectionChangeDetected(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigation navigation,
            [NotNull] ISet<object> added,
            [NotNull] ISet<object> removed)
        {
            var definition = CoreStrings.LogCollectionChangeDetected;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    added.Count,
                    removed.Count,
                    navigation.DeclaringEntityType.ShortName(),
                    navigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CollectionChangedEventData(
                        definition,
                        CollectionChangeDetected,
                        new EntityEntry(internalEntityEntry),
                        navigation,
                        added,
                        removed));
            }
        }

        private static string CollectionChangeDetected(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<int, int, string, string>)definition;
            var p = (CollectionChangedEventData)payload;
            return d.GenerateMessage(
                p.Added.Count(),
                p.Removed.Count(),
                p.Navigation.DeclaringEntityType.ShortName(),
                p.Navigation.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CollectionChangeDetectedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigation navigation,
            [NotNull] ISet<object> added,
            [NotNull] ISet<object> removed)
        {
            var definition = CoreStrings.LogCollectionChangeDetectedSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    added.Count,
                    removed.Count,
                    navigation.DeclaringEntityType.ShortName(),
                    navigation.Name,
                    internalEntityEntry.BuildCurrentValuesString(navigation.DeclaringEntityType.FindPrimaryKey().Properties));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CollectionChangedEventData(
                        definition,
                        CollectionChangeDetectedSensitive,
                        new EntityEntry(internalEntityEntry),
                        navigation,
                        added,
                        removed));
            }
        }

        private static string CollectionChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<int, int, string, string, string>)definition;
            var p = (CollectionChangedEventData)payload;
            return d.GenerateMessage(
                p.Added.Count(),
                p.Removed.Count(),
                p.Navigation.DeclaringEntityType.ShortName(),
                p.Navigation.Name,
                p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.Navigation.DeclaringEntityType.FindPrimaryKey().Properties));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ReferenceChangeDetected(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigation navigation,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreStrings.LogReferenceChangeDetected;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigation.DeclaringEntityType.ShortName(),
                    navigation.Name);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ReferenceChangedEventData(
                        definition,
                        ReferenceChangeDetected,
                        new EntityEntry(internalEntityEntry),
                        navigation,
                        oldValue,
                        newValue));
            }
        }

        private static string ReferenceChangeDetected(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (ReferenceChangedEventData)payload;
            return d.GenerateMessage(
                p.Navigation.DeclaringEntityType.ShortName(),
                p.Navigation.Name);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ReferenceChangeDetectedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigation navigation,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreStrings.LogReferenceChangeDetectedSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    navigation.DeclaringEntityType.ShortName(),
                    navigation.Name,
                    internalEntityEntry.BuildCurrentValuesString(navigation.DeclaringEntityType.FindPrimaryKey().Properties));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new ReferenceChangedEventData(
                        definition,
                        ReferenceChangeDetectedSensitive,
                        new EntityEntry(internalEntityEntry),
                        navigation,
                        oldValue,
                        newValue));
            }
        }

        private static string ReferenceChangeDetectedSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string>)definition;
            var p = (ReferenceChangedEventData)payload;
            return d.GenerateMessage(
                p.Navigation.DeclaringEntityType.ShortName(),
                p.Navigation.Name,
                p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.Navigation.DeclaringEntityType.FindPrimaryKey().Properties));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void StartedTracking(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry entry)
        {
            var definition = CoreStrings.LogStartedTracking;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    entry.StateManager.Context.GetType().ShortDisplayName(),
                    entry.EntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new EntityEntryEventData(
                        definition,
                        StartedTracking,
                        new EntityEntry(entry)));
            }
        }

        private static string StartedTracking(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (EntityEntryEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Context.GetType().ShortDisplayName(),
                p.EntityEntry.Metadata.ShortName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void StartedTrackingSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry entry)
        {
            var definition = CoreStrings.LogStartedTrackingSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    entry.StateManager.Context.GetType().ShortDisplayName(),
                    entry.EntityType.ShortName(),
                    entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new EntityEntryEventData(
                        definition,
                        StartedTrackingSensitive,
                        new EntityEntry(entry)));
            }
        }

        private static string StartedTrackingSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string>)definition;
            var p = (EntityEntryEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Context.GetType().ShortDisplayName(),
                p.EntityEntry.Metadata.ShortName(),
                p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.EntityEntry.Metadata.FindPrimaryKey().Properties));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void StateChanged(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry entry,
            EntityState oldState,
            EntityState newState)
        {
            var definition = CoreStrings.LogStateChanged;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    entry.EntityType.ShortName(),
                    entry.StateManager.Context.GetType().ShortDisplayName(),
                    oldState,
                    newState);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new StateChangedEventData(
                        definition,
                        StateChanged,
                        new EntityEntry(entry),
                        oldState,
                        newState));
            }
        }

        private static string StateChanged(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, EntityState, EntityState>)definition;
            var p = (StateChangedEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Metadata.ShortName(),
                p.EntityEntry.Context.GetType().ShortDisplayName(),
                p.OldState,
                p.NewState);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void StateChangedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry entry,
            EntityState oldState,
            EntityState newState)
        {
            var definition = CoreStrings.LogStateChangedSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    entry.EntityType.ShortName(),
                    entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
                    entry.StateManager.Context.GetType().ShortDisplayName(),
                    oldState,
                    newState);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new StateChangedEventData(
                        definition,
                        StateChangedSensitive,
                        new EntityEntry(entry),
                        oldState,
                        newState));
            }
        }

        private static string StateChangedSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string, EntityState, EntityState>)definition;
            var p = (StateChangedEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Metadata.ShortName(),
                p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.EntityEntry.Metadata.FindPrimaryKey().Properties),
                p.EntityEntry.Context.GetType().ShortDisplayName(),
                p.OldState,
                p.NewState);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ValueGenerated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry entry,
            [NotNull] IProperty property,
            [CanBeNull] object value,
            bool temporary)
        {
            var definition = temporary
                ? CoreStrings.LogTempValueGenerated
                : CoreStrings.LogValueGenerated;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    entry.StateManager.Context.GetType().ShortDisplayName(),
                    property.Name,
                    entry.EntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyValueEventData(
                        definition,
                        ValueGenerated,
                        new EntityEntry(entry),
                        property,
                        value));
            }
        }

        private static string ValueGenerated(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string>)definition;
            var p = (PropertyValueEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Context.GetType().ShortDisplayName(),
                p.Property.Name,
                p.EntityEntry.Metadata.ShortName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ValueGeneratedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry entry,
            [NotNull] IProperty property,
            [CanBeNull] object value,
            bool temporary)
        {
            var definition = temporary
                ? CoreStrings.LogTempValueGeneratedSensitive
                : CoreStrings.LogValueGeneratedSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    entry.StateManager.Context.GetType().ShortDisplayName(),
                    value,
                    property.Name,
                    entry.EntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyValueEventData(
                        definition,
                        ValueGeneratedSensitive,
                        new EntityEntry(entry),
                        property,
                        value));
            }
        }

        private static string ValueGeneratedSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, object, string, string>)definition;
            var p = (PropertyValueEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Context.GetType().ShortDisplayName(),
                p.Value,
                p.Property.Name,
                p.EntityEntry.Metadata.ShortName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CascadeDelete(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] InternalEntityEntry childEntry,
            [NotNull] InternalEntityEntry parentEntry,
            EntityState state)
        {
            var definition = CoreStrings.LogCascadeDelete;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    childEntry.EntityType.ShortName(),
                    state,
                    parentEntry.EntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CascadeDeleteEventData(
                        definition,
                        CascadeDelete,
                        new EntityEntry(childEntry),
                        new EntityEntry(parentEntry),
                        state));
            }
        }

        private static string CascadeDelete(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, EntityState, string>)definition;
            var p = (CascadeDeleteEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Metadata.ShortName(),
                p.State,
                p.ParentEntityEntry.Metadata.ShortName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CascadeDeleteSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] InternalEntityEntry childEntry,
            [NotNull] InternalEntityEntry parentEntry,
            EntityState state)
        {
            var definition = CoreStrings.LogCascadeDeleteSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    childEntry.EntityType.ShortName(),
                    childEntry.BuildCurrentValuesString(childEntry.EntityType.FindPrimaryKey().Properties),
                    state,
                    parentEntry.EntityType.ShortName(),
                    parentEntry.BuildCurrentValuesString(parentEntry.EntityType.FindPrimaryKey().Properties));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CascadeDeleteEventData(
                        definition,
                        CascadeDeleteSensitive,
                        new EntityEntry(childEntry),
                        new EntityEntry(parentEntry),
                        state));
            }
        }

        private static string CascadeDeleteSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, EntityState, string, string>)definition;
            var p = (CascadeDeleteEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Metadata.ShortName(),
                p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.EntityEntry.Metadata.FindPrimaryKey().Properties),
                p.State,
                p.ParentEntityEntry.Metadata.ShortName(),
                p.ParentEntityEntry.GetInfrastructure().BuildCurrentValuesString(p.ParentEntityEntry.Metadata.FindPrimaryKey().Properties));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CascadeDeleteOrphan(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] InternalEntityEntry childEntry,
            [NotNull] IEntityType parentEntityType,
            EntityState state)
        {
            var definition = CoreStrings.LogCascadeDeleteOrphan;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    childEntry.EntityType.ShortName(),
                    state,
                    parentEntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CascadeDeleteOrphanEventData(
                        definition,
                        CascadeDeleteOrphan,
                        new EntityEntry(childEntry),
                        parentEntityType,
                        state));
            }
        }

        private static string CascadeDeleteOrphan(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, EntityState, string>)definition;
            var p = (CascadeDeleteOrphanEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Metadata.ShortName(),
                p.State,
                p.ParentEntityType.ShortName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void CascadeDeleteOrphanSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] InternalEntityEntry childEntry,
            [NotNull] IEntityType parentEntityType,
            EntityState state)
        {
            var definition = CoreStrings.LogCascadeDeleteOrphanSensitive;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    childEntry.EntityType.ShortName(),
                    childEntry.BuildCurrentValuesString(childEntry.EntityType.FindPrimaryKey().Properties),
                    state,
                    parentEntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CascadeDeleteOrphanEventData(
                        definition,
                        CascadeDeleteOrphanSensitive,
                        new EntityEntry(childEntry),
                        parentEntityType,
                        state));
            }
        }

        private static string CascadeDeleteOrphanSensitive(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, EntityState, string>)definition;
            var p = (CascadeDeleteOrphanEventData)payload;
            return d.GenerateMessage(
                p.EntityEntry.Metadata.ShortName(),
                p.EntityEntry.GetInfrastructure().BuildCurrentValuesString(p.EntityEntry.Metadata.FindPrimaryKey().Properties),
                p.State,
                p.ParentEntityType.ShortName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SaveChangesStarting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context)
        {
            var definition = CoreStrings.LogSaveChangesStarting;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    context.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextEventData(
                        definition,
                        SaveChangesStarting,
                        context));
            }
        }

        private static string SaveChangesStarting(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (DbContextEventData)payload;
            return d.GenerateMessage(p.Context.GetType().ShortDisplayName());
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void SaveChangesCompleted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context,
            int entitiesSavedCount)
        {
            var definition = CoreStrings.LogSaveChangesCompleted;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    context.GetType().ShortDisplayName(),
                    entitiesSavedCount);
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new SaveChangesCompletedEventData(
                        definition,
                        SaveChangesCompleted,
                        context,
                        entitiesSavedCount));
            }
        }

        private static string SaveChangesCompleted(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, int>)definition;
            var p = (SaveChangesCompletedEventData)payload;
            return d.GenerateMessage(
                p.Context.GetType().ShortDisplayName(),
                p.EntitiesSavedCount);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static void ContextDisposed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context)
        {
            var definition = CoreStrings.LogContextDisposed;

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    context.GetType().ShortDisplayName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new DbContextEventData(
                        definition,
                        ContextDisposed,
                        context));
            }
        }

        private static string ContextDisposed(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (DbContextEventData)payload;
            return d.GenerateMessage(p.Context.GetType().ShortDisplayName());
        }
    }
}
