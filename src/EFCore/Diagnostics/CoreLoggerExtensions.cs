// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         This class contains static methods used by EF Core internals and database providers to
    ///         write information to an <see cref="ILogger" /> and a <see cref="DiagnosticListener" /> for
    ///         well-known events.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public static class CoreLoggerExtensions
    {
        /// <summary>
        ///     Logs for the <see cref="CoreEventId.SaveChangesFailed" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context in use. </param>
        /// <param name="exception"> The exception that caused this event. </param>
        public static void SaveChangesFailed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context,
            [NotNull] Exception exception)
        {
            var definition = CoreResources.LogExceptionDuringSaveChanges(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.OptimisticConcurrencyException" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context in use. </param>
        /// <param name="exception"> The exception that caused this event. </param>
        public static void OptimisticConcurrencyException(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context,
            [NotNull] Exception exception)
        {
            var definition = CoreResources.LogOptimisticConcurrencyException(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.DuplicateDependentEntityTypeInstanceWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="dependent1"> The first dependent type. </param>
        /// <param name="dependent2"> The second dependent type. </param>
        public static void DuplicateDependentEntityTypeInstanceWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] IEntityType dependent1,
            [NotNull] IEntityType dependent2)
        {
            var definition = CoreResources.LogDuplicateDependentEntityTypeInstance(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.QueryIterationFailed" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="contextType"> The <see cref="DbContext" /> type being used. </param>
        /// <param name="exception"> The exception that caused this failure. </param>
        public static void QueryIterationFailed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] Type contextType,
            [NotNull] Exception exception)
        {
            var definition = CoreResources.LogExceptionDuringQueryIteration(diagnostics);

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

        // TODO: Commenting this since we need to add similar logging in ExpressionTrees
        ///// <summary>
        /////     Logs for the <see cref="CoreEventId.QueryModelCompiling" /> event.
        ///// </summary>
        ///// <param name="diagnostics"> The diagnostics logger to use. </param>
        ///// <param name="queryModel"> The query model. </param>
        //public static void QueryModelCompiling(
        //    [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        //    [NotNull] QueryModel queryModel)
        //{
        //    var definition = CoreResources.LogCompilingQueryModel(diagnostics);

        //    var warningBehavior = definition.GetLogBehavior(diagnostics);
        //    if (warningBehavior != WarningBehavior.Ignore)
        //    {
        //        definition.Log(
        //            diagnostics,
        //            warningBehavior,
        //            Environment.NewLine, queryModel.Print());
        //    }

        //    if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
        //    {
        //        diagnostics.DiagnosticSource.Write(
        //            definition.EventId.Name,
        //            new QueryModelEventData(
        //                definition,
        //                QueryModelCompiling,
        //                queryModel));
        //    }
        //}

        //private static string QueryModelCompiling(EventDefinitionBase definition, EventData payload)
        //{
        //    var d = (EventDefinition<string, string>)definition;
        //    var p = (QueryModelEventData)payload;
        //    return d.GenerateMessage(Environment.NewLine, p.QueryModel.Print());
        //}

        ///// <summary>
        /////     Logs for the <see cref="CoreEventId.RowLimitingOperationWithoutOrderByWarning" /> event.
        ///// </summary>
        ///// <param name="diagnostics"> The diagnostics logger to use. </param>
        ///// <param name="queryModel"> The query model. </param>
        //public static void RowLimitingOperationWithoutOrderByWarning(
        //    [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        //    [NotNull] QueryModel queryModel)
        //{
        //    var definition = CoreResources.LogRowLimitingOperationWithoutOrderBy(diagnostics);

        //    var warningBehavior = definition.GetLogBehavior(diagnostics);
        //    if (warningBehavior != WarningBehavior.Ignore)
        //    {
        //        definition.Log(
        //            diagnostics,
        //            warningBehavior,
        //            queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
        //    }

        //    if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
        //    {
        //        diagnostics.DiagnosticSource.Write(
        //            definition.EventId.Name,
        //            new QueryModelEventData(
        //                definition,
        //                RowLimitingOperationWithoutOrderByWarning,
        //                queryModel));
        //    }
        //}

        //private static string RowLimitingOperationWithoutOrderByWarning(EventDefinitionBase definition, EventData payload)
        //{
        //    var d = (EventDefinition<string>)definition;
        //    var p = (QueryModelEventData)payload;
        //    return d.GenerateMessage(p.QueryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
        //}

        ///// <summary>
        /////     Logs for the <see cref="CoreEventId.FirstWithoutOrderByAndFilterWarning" /> event.
        ///// </summary>
        ///// <param name="diagnostics"> The diagnostics logger to use. </param>
        ///// <param name="queryModel"> The query model. </param>
        //public static void FirstWithoutOrderByAndFilterWarning(
        //    [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        //    [NotNull] QueryModel queryModel)
        //{
        //    var definition = CoreResources.LogFirstWithoutOrderByAndFilter(diagnostics);

        //    var warningBehavior = definition.GetLogBehavior(diagnostics);
        //    if (warningBehavior != WarningBehavior.Ignore)
        //    {
        //        definition.Log(
        //            diagnostics,
        //            warningBehavior,
        //            queryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
        //    }

        //    if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
        //    {
        //        diagnostics.DiagnosticSource.Write(
        //            definition.EventId.Name,
        //            new QueryModelEventData(
        //                definition,
        //                FirstWithoutOrderByAndFilterWarning,
        //                queryModel));
        //    }
        //}

        //private static string FirstWithoutOrderByAndFilterWarning(EventDefinitionBase definition, EventData payload)
        //{
        //    var d = (EventDefinition<string>)definition;
        //    var p = (QueryModelEventData)payload;
        //    return d.GenerateMessage(p.QueryModel.Print(removeFormatting: true, characterLimit: QueryModelStringLengthLimit));
        //}

        ///// <summary>
        /////     Logs for the <see cref="CoreEventId.QueryModelOptimized" /> event.
        ///// </summary>
        ///// <param name="diagnostics"> The diagnostics logger to use. </param>
        ///// <param name="queryModel"> The query model. </param>
        //public static void QueryModelOptimized(
        //    [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        //    [NotNull] QueryModel queryModel)
        //{
        //    var definition = CoreResources.LogOptimizedQueryModel(diagnostics);

        //    var warningBehavior = definition.GetLogBehavior(diagnostics);
        //    if (warningBehavior != WarningBehavior.Ignore)
        //    {
        //        definition.Log(
        //            diagnostics,
        //            warningBehavior,
        //            Environment.NewLine, queryModel.Print());
        //    }

        //    if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
        //    {
        //        diagnostics.DiagnosticSource.Write(
        //            definition.EventId.Name,
        //            new QueryModelEventData(
        //                definition,
        //                QueryModelOptimized,
        //                queryModel));
        //    }
        //}

        //private static string QueryModelOptimized(EventDefinitionBase definition, EventData payload)
        //{
        //    var d = (EventDefinition<string, string>)definition;
        //    var p = (QueryModelEventData)payload;
        //    return d.GenerateMessage(Environment.NewLine, p.QueryModel.Print());
        //}

        ///// <summary>
        /////     Logs for the <see cref="CoreEventId.NavigationIncluded" /> event.
        ///// </summary>
        ///// <param name="diagnostics"> The diagnostics logger to use. </param>
        ///// <param name="includeResultOperator"> The result operator for the Include. </param>
        //public static void NavigationIncluded(
        //    [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        //    [NotNull] IncludeResultOperator includeResultOperator)
        //{
        //    var definition = CoreResources.LogIncludingNavigation(diagnostics);

        //    var warningBehavior = definition.GetLogBehavior(diagnostics);
        //    if (warningBehavior != WarningBehavior.Ignore)
        //    {
        //        definition.Log(
        //            diagnostics,
        //            warningBehavior,
        //            includeResultOperator.DisplayString());
        //    }

        //    if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
        //    {
        //        diagnostics.DiagnosticSource.Write(
        //            definition.EventId.Name,
        //            new IncludeEventData(
        //                definition,
        //                NavigationIncluded,
        //                includeResultOperator));
        //    }
        //}

        //private static string NavigationIncluded(EventDefinitionBase definition, EventData payload)
        //{
        //    var d = (EventDefinition<string>)definition;
        //    var p = (IncludeEventData)payload;
        //    return d.GenerateMessage(p.IncludeResultOperator.DisplayString());
        //}

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.QueryExecutionPlanned" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="expressionPrinter"> Used to create a human-readable representation of the expression tree. </param>
        /// <param name="queryExecutorExpression"> The query expression tree. </param>
        public static void QueryExecutionPlanned(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] ExpressionPrinter expressionPrinter,
            [NotNull] Expression queryExecutorExpression)
        {
            var definition = CoreResources.LogQueryExecutionPlanned(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.SensitiveDataLoggingEnabledWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <typeparam name="TLoggerCategory"> The logger category for which to log the warning. </typeparam>
        public static void SensitiveDataLoggingEnabledWarning<TLoggerCategory>(
            [NotNull] this IDiagnosticsLogger<TLoggerCategory> diagnostics)
            where TLoggerCategory : LoggerCategory<TLoggerCategory>, new()
        {
            var definition = CoreResources.LogSensitiveDataLoggingEnabled(diagnostics);

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

        ///// <summary>
        /////     Logs for the <see cref="CoreEventId.IncludeIgnoredWarning" /> event.
        ///// </summary>
        ///// <param name="diagnostics"> The diagnostics logger to use. </param>
        ///// <param name="includeResultOperator"> The result operator for the Include. </param>
        //public static void IncludeIgnoredWarning(
        //    [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
        //    [NotNull] IncludeResultOperator includeResultOperator)
        //{
        //    var definition = CoreResources.LogIgnoredInclude(diagnostics);

        //    var warningBehavior = definition.GetLogBehavior(diagnostics);
        //    if (warningBehavior != WarningBehavior.Ignore)
        //    {
        //        definition.Log(
        //            diagnostics,
        //            warningBehavior,
        //            includeResultOperator.DisplayString());
        //    }

        //    if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
        //    {
        //        diagnostics.DiagnosticSource.Write(
        //            definition.EventId.Name,
        //            new IncludeEventData(
        //                definition,
        //                IncludeIgnoredWarning,
        //                includeResultOperator));
        //    }
        //}

        //private static string IncludeIgnoredWarning(EventDefinitionBase definition, EventData payload)
        //{
        //    var d = (EventDefinition<string>)definition;
        //    var p = (IncludeEventData)payload;
        //    return d.GenerateMessage(p.IncludeResultOperator.DisplayString());
        //}

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="navigation"> The navigation being used. </param>
        public static void PossibleUnintendedCollectionNavigationNullComparisonWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] INavigation navigation)
        {
            var definition = CoreResources.LogPossibleUnintendedCollectionNavigationNullComparison(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    $"{navigation.DeclaringEntityType.Name}.{navigation.GetTargetType().Name}");
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new NavigationEventData(
                        definition,
                        PossibleUnintendedCollectionNavigationNullComparisonWarning,
                        navigation));
            }
        }

        private static string PossibleUnintendedCollectionNavigationNullComparisonWarning(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string>)definition;
            var p = (NavigationEventData)payload;
            return d.GenerateMessage($"{p.Navigation.DeclaringEntityType.Name}.{p.Navigation.GetTargetType().Name}");
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.PossibleUnintendedReferenceComparisonWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="left"> The left side expression. </param>
        /// <param name="right"> The right side expression. </param>
        public static void PossibleUnintendedReferenceComparisonWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Query> diagnostics,
            [NotNull] Expression left,
            [NotNull] Expression right)
        {
            var definition = CoreResources.LogPossibleUnintendedReferenceComparison(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ServiceProviderCreated" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="serviceProvider"> The service provider. </param>
        public static void ServiceProviderCreated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] IServiceProvider serviceProvider)
        {
            var definition = CoreResources.LogServiceProviderCreated(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ManyServiceProvidersCreatedWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="serviceProviders"> The service providers that have been created. </param>
        public static void ManyServiceProvidersCreatedWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] ICollection<IServiceProvider> serviceProviders)
        {
            var definition = CoreResources.LogManyServiceProvidersCreated(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ServiceProviderDebugInfo" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="newDebugInfo"> Debug information for the new service providers. </param>
        /// <param name="cachedDebugInfos"> Debug information for existing service providers. </param>
        public static void ServiceProviderDebugInfo(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] IDictionary<string, string> newDebugInfo,
            [NotNull] IList<IDictionary<string, string>> cachedDebugInfos)
        {
            var definition = CoreResources.LogServiceProviderDebugInfo(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ContextInitialized" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        /// <param name="contextOptions"> The context options being used. </param>
        public static void ContextInitialized(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] DbContextOptions contextOptions)
        {
            var definition = CoreResources.LogContextInitialized(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ExecutionStrategyRetrying" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="exceptionsEncountered"> The exceptions(s) that caused the failure. </param>
        /// <param name="delay"> The delay that before the next retry. </param>
        /// <param name="async"> Indicates whether execution is asynchronous or not. </param>
        public static void ExecutionStrategyRetrying(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] IReadOnlyList<Exception> exceptionsEncountered,
            TimeSpan delay,
            bool async)
        {
            var definition = CoreResources.LogExecutionStrategyRetrying(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.LazyLoadOnDisposedContextWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="navigationName"> The name of the navigation property. </param>
        public static void LazyLoadOnDisposedContextWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] object entityType,
            [NotNull] string navigationName)
        {
            var definition = CoreResources.LogLazyLoadOnDisposedContext(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.NavigationLazyLoading" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="navigationName"> The name of the navigation property. </param>
        public static void NavigationLazyLoading(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] object entityType,
            [NotNull] string navigationName)
        {
            var definition = CoreResources.LogNavigationLazyLoading(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.DetachedLazyLoadingWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="navigationName"> The name of the navigation property. </param>
        public static void DetachedLazyLoadingWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context,
            [NotNull] object entityType,
            [NotNull] string navigationName)
        {
            var definition = CoreResources.LogDetachedLazyLoading(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ShadowPropertyCreated" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="property"> The property. </param>
        public static void ShadowPropertyCreated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = CoreResources.LogShadowPropertyCreated(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.CollectionWithoutComparer" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="property"> The property. </param>
        public static void CollectionWithoutComparer(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IProperty property)
        {
            var definition = CoreResources.LogCollectionWithoutComparer(diagnostics);

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
                        CollectionWithoutComparer,
                        property));
            }
        }

        private static string CollectionWithoutComparer(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (PropertyEventData)payload;
            return d.GenerateMessage(p.Property.Name, p.Property.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.RedundantIndexRemoved" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="redundantIndex"> The redundant index. </param>
        /// <param name="otherIndex"> The other index. </param>
        public static void RedundantIndexRemoved(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IReadOnlyList<IPropertyBase> redundantIndex,
            [NotNull] IReadOnlyList<IPropertyBase> otherIndex)
        {
            var definition = CoreResources.LogRedundantIndexRemoved(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    redundantIndex.Format(), redundantIndex.First().DeclaringType.DisplayName(), otherIndex.Format());
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
                p.FirstPropertyCollection.Format(),
                p.FirstPropertyCollection.First().DeclaringType.DisplayName(),
                p.SecondPropertyCollection.Format());
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.RedundantForeignKeyWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="redundantForeignKey"> The redundant foreign key. </param>
        public static void RedundantForeignKeyWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model.Validation> diagnostics,
            [NotNull] IForeignKey redundantForeignKey)
        {
            var definition = CoreResources.LogRedundantForeignKey(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    redundantForeignKey.Properties.Format(), redundantForeignKey.DeclaringEntityType.DisplayName());
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
                p.ForeignKey.Properties.Format(),
                p.ForeignKey.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.IncompatibleMatchingForeignKeyProperties" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="foreignKeyProperties"> The properties that make up the foreign key. </param>
        /// <param name="principalKeyProperties"> The corresponding keys on the principal side. </param>
        public static void IncompatibleMatchingForeignKeyProperties(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IReadOnlyList<IPropertyBase> foreignKeyProperties,
            [NotNull] IReadOnlyList<IPropertyBase> principalKeyProperties)
        {
            var definition = CoreResources.LogIncompatibleMatchingForeignKeyProperties(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    foreignKeyProperties.Format(includeTypes: true),
                    principalKeyProperties.Format(includeTypes: true));
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
                p.FirstPropertyCollection.Format(includeTypes: true),
                p.SecondPropertyCollection.Format(includeTypes: true));
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.RequiredAttributeInverted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="navigation"> The navigation property. </param>
        public static void RequiredAttributeInverted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation)
        {
            var definition = CoreResources.LogRequiredAttributeInverted(diagnostics);

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
                        RequiredAttributeInverted,
                        navigation));
            }
        }

        private static string RequiredAttributeInverted(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (NavigationEventData)payload;
            return d.GenerateMessage(p.Navigation.Name, p.Navigation.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.NonNullableInverted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="navigation"> The navigation property. </param>
        public static void NonNullableInverted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation)
        {
            var definition = CoreResources.LogNonNullableInverted(diagnostics);

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
                        NonNullableInverted,
                        navigation));
            }
        }

        private static string NonNullableInverted(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (NavigationEventData)payload;
            return d.GenerateMessage(p.Navigation.Name, p.Navigation.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.RequiredAttributeOnBothNavigations" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="firstNavigation"> The first navigation property. </param>
        /// <param name="secondNavigation"> The second navigation property. </param>
        public static void RequiredAttributeOnBothNavigations(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation)
        {
            var definition = CoreResources.LogRequiredAttributeOnBothNavigations(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.NonNullableReferenceOnBothNavigations" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="firstNavigation"> The first navigation property. </param>
        /// <param name="secondNavigation"> The second navigation property. </param>
        public static void NonNullableReferenceOnBothNavigations(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation)
        {
            var definition = CoreResources.LogNonNullableReferenceOnBothNavigations(diagnostics);

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
                        NonNullableReferenceOnBothNavigations,
                        new[] { firstNavigation },
                        new[] { secondNavigation }));
            }
        }

        private static string NonNullableReferenceOnBothNavigations(EventDefinitionBase definition, EventData payload)
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
        ///     Logs for the <see cref="CoreEventId.RequiredAttributeOnDependent" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="navigation"> The navigation property. </param>
        public static void RequiredAttributeOnDependent(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation)
        {
            var definition = CoreResources.LogRequiredAttributeOnDependent(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.NonNullableReferenceOnDependent" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="navigation"> The navigation property. </param>
        public static void NonNullableReferenceOnDependent(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation)
        {
            var definition = CoreResources.LogNonNullableReferenceOnDependent(diagnostics);

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
                        NonNullableReferenceOnDependent,
                        navigation));
            }
        }

        private static string NonNullableReferenceOnDependent(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (NavigationEventData)payload;
            return d.GenerateMessage(p.Navigation.Name, p.Navigation.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.RequiredAttributeOnCollection" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="navigation"> The navigation property. </param>
        public static void RequiredAttributeOnCollection(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation)
        {
            var definition = CoreResources.LogRequiredAttributeOnCollection(diagnostics);

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
                        RequiredAttributeOnCollection,
                        navigation));
            }
        }

        private static string RequiredAttributeOnCollection(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string>)definition;
            var p = (NavigationEventData)payload;
            return d.GenerateMessage(p.Navigation.Name, p.Navigation.DeclaringEntityType.DisplayName());
        }

        /// <summary>
        ///     Logs for the <see cref="CoreEventId.ConflictingShadowForeignKeysWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="foreignKey"> The foreign key. </param>
        public static void ConflictingShadowForeignKeysWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IForeignKey foreignKey)
        {
            var definition = CoreResources.LogConflictingShadowForeignKeys(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.MultiplePrimaryKeyCandidates" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="firstProperty"> The first property. </param>
        /// <param name="secondProperty"> The second property. </param>
        public static void MultiplePrimaryKeyCandidates(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IProperty firstProperty,
            [NotNull] IProperty secondProperty)
        {
            var definition = CoreResources.LogMultiplePrimaryKeyCandidates(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.MultipleNavigationProperties" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="firstPropertyCollection"> The first set of properties. </param>
        /// <param name="secondPropertyCollection"> The second set of properties. </param>
        public static void MultipleNavigationProperties(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> firstPropertyCollection,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> secondPropertyCollection)
        {
            var definition = CoreResources.LogMultipleNavigationProperties(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.MultipleInversePropertiesSameTargetWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="conflictingNavigations"> The list of conflicting navigation properties. </param>
        /// <param name="inverseNavigation"> The inverse navigation property. </param>
        /// <param name="targetType"> The target type. </param>
        public static void MultipleInversePropertiesSameTargetWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEnumerable<Tuple<MemberInfo, Type>> conflictingNavigations,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] Type targetType)
        {
            var definition = CoreResources.LogMultipleInversePropertiesSameTarget(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.NonDefiningInverseNavigationWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="declaringType"> The declaring entity type. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="targetType"> The target type. </param>
        /// <param name="inverseNavigation"> The inverse navigation property. </param>
        /// <param name="definingNavigation"> The defining navigation property. </param>
        public static void NonDefiningInverseNavigationWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEntityType declaringType,
            [NotNull] MemberInfo navigation,
            [NotNull] IEntityType targetType,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] MemberInfo definingNavigation)
        {
            var definition = CoreResources.LogNonDefiningInverseNavigation(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.NonOwnershipInverseNavigationWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="declaringType"> The declaring entity type. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="targetType"> The target type. </param>
        /// <param name="inverseNavigation"> The inverse navigation property. </param>
        /// <param name="ownershipNavigation"> The ownership navigation property. </param>
        public static void NonOwnershipInverseNavigationWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] IEntityType declaringType,
            [NotNull] MemberInfo navigation,
            [NotNull] IEntityType targetType,
            [NotNull] MemberInfo inverseNavigation,
            [NotNull] MemberInfo ownershipNavigation)
        {
            var definition = CoreResources.LogNonOwnershipInverseNavigation(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ForeignKeyAttributesOnBothPropertiesWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="firstNavigation"> The first navigation property. </param>
        /// <param name="secondNavigation"> The second navigation property. </param>
        /// <param name="firstProperty"> The first property. </param>
        /// <param name="secondProperty"> The second property. </param>
        public static void ForeignKeyAttributesOnBothPropertiesWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation,
            [NotNull] MemberInfo firstProperty,
            [NotNull] MemberInfo secondProperty)
        {
            var definition = CoreResources.LogForeignKeyAttributesOnBothProperties(diagnostics);

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
                            new Tuple<MemberInfo, Type>(
                                firstNavigation.GetIdentifyingMemberInfo(), firstNavigation.DeclaringEntityType.ClrType),
                            new Tuple<MemberInfo, Type>(firstProperty, firstNavigation.DeclaringEntityType.ClrType)
                        },
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(
                                secondNavigation.GetIdentifyingMemberInfo(), secondNavigation.DeclaringEntityType.ClrType),
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
        ///     Logs for the <see cref="CoreEventId.ForeignKeyAttributesOnBothNavigationsWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="firstNavigation"> The first navigation property. </param>
        /// <param name="secondNavigation"> The second navigation property. </param>
        public static void ForeignKeyAttributesOnBothNavigationsWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation firstNavigation,
            [NotNull] INavigation secondNavigation)
        {
            var definition = CoreResources.LogForeignKeyAttributesOnBothNavigations(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="property"> The property. </param>
        public static void ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Model> diagnostics,
            [NotNull] INavigation navigation,
            [NotNull] MemberInfo property)
        {
            var definition = CoreResources.LogConflictingForeignKeyAttributesOnNavigationAndProperty(diagnostics);

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
                        new[]
                        {
                            new Tuple<MemberInfo, Type>(navigation.GetIdentifyingMemberInfo(), navigation.DeclaringEntityType.ClrType)
                        },
                        new[] { new Tuple<MemberInfo, Type>(property, property.DeclaringType) }));
            }
        }

        private static string ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning(
            EventDefinitionBase definition, EventData payload)
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
        ///     Logs for the <see cref="CoreEventId.DetectChangesStarting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        public static void DetectChangesStarting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] DbContext context)
        {
            var definition = CoreResources.LogDetectChangesStarting(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.DetectChangesCompleted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        public static void DetectChangesCompleted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] DbContext context)
        {
            var definition = CoreResources.LogDetectChangesCompleted(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.PropertyChangeDetected" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="property"> The property. </param>
        /// <param name="oldValue"> The old value. </param>
        /// <param name="newValue"> The new value. </param>
        public static void PropertyChangeDetected(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreResources.LogPropertyChangeDetected(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.PropertyChangeDetected" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="property"> The property. </param>
        /// <param name="oldValue"> The old value. </param>
        /// <param name="newValue"> The new value. </param>
        public static void PropertyChangeDetectedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreResources.LogPropertyChangeDetectedSensitive(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ForeignKeyChangeDetected" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="property"> The property. </param>
        /// <param name="oldValue"> The old value. </param>
        /// <param name="newValue"> the new value. </param>
        public static void ForeignKeyChangeDetected(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreResources.LogForeignKeyChangeDetected(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ForeignKeyChangeDetected" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="property"> The property. </param>
        /// <param name="oldValue"> The old value. </param>
        /// <param name="newValue"> The new value. </param>
        public static void ForeignKeyChangeDetectedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreResources.LogForeignKeyChangeDetectedSensitive(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.CollectionChangeDetected" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="added"> The added values. </param>
        /// <param name="removed"> The removed values. </param>
        public static void CollectionChangeDetected(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigation navigation,
            [NotNull] ISet<object> added,
            [NotNull] ISet<object> removed)
        {
            var definition = CoreResources.LogCollectionChangeDetected(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.CollectionChangeDetected" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="added"> The added values. </param>
        /// <param name="removed"> The removed values. </param>
        public static void CollectionChangeDetectedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigation navigation,
            [NotNull] ISet<object> added,
            [NotNull] ISet<object> removed)
        {
            var definition = CoreResources.LogCollectionChangeDetectedSensitive(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ReferenceChangeDetected" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="oldValue"> The old value. </param>
        /// <param name="newValue"> The new value. </param>
        public static void ReferenceChangeDetected(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigation navigation,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreResources.LogReferenceChangeDetected(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ReferenceChangeDetected" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="navigation"> The navigation property. </param>
        /// <param name="oldValue"> The old value. </param>
        /// <param name="newValue"> The new value. </param>
        public static void ReferenceChangeDetectedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigation navigation,
            [CanBeNull] object oldValue,
            [CanBeNull] object newValue)
        {
            var definition = CoreResources.LogReferenceChangeDetectedSensitive(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.StartedTracking" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        public static void StartedTracking(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry)
        {
            var definition = CoreResources.LogStartedTracking(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalEntityEntry.StateManager.Context.GetType().ShortDisplayName(),
                    internalEntityEntry.EntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new EntityEntryEventData(
                        definition,
                        StartedTracking,
                        new EntityEntry(internalEntityEntry)));
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
        ///     Logs for the <see cref="CoreEventId.StartedTracking" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        public static void StartedTrackingSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry)
        {
            var definition = CoreResources.LogStartedTrackingSensitive(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalEntityEntry.StateManager.Context.GetType().ShortDisplayName(),
                    internalEntityEntry.EntityType.ShortName(),
                    internalEntityEntry.BuildCurrentValuesString(internalEntityEntry.EntityType.FindPrimaryKey().Properties));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new EntityEntryEventData(
                        definition,
                        StartedTrackingSensitive,
                        new EntityEntry(internalEntityEntry)));
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
        ///     Logs for the <see cref="CoreEventId.StateChanged" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="oldState"> The old value. </param>
        /// <param name="newState"> The new value. </param>
        public static void StateChanged(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            EntityState oldState,
            EntityState newState)
        {
            var definition = CoreResources.LogStateChanged(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalEntityEntry.EntityType.ShortName(),
                    internalEntityEntry.StateManager.Context.GetType().ShortDisplayName(),
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
                        new EntityEntry(internalEntityEntry),
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
        ///     Logs for the <see cref="CoreEventId.StateChanged" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="oldState"> The old state. </param>
        /// <param name="newState"> The new state. </param>
        public static void StateChangedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            EntityState oldState,
            EntityState newState)
        {
            var definition = CoreResources.LogStateChangedSensitive(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalEntityEntry.EntityType.ShortName(),
                    internalEntityEntry.BuildCurrentValuesString(internalEntityEntry.EntityType.FindPrimaryKey().Properties),
                    internalEntityEntry.StateManager.Context.GetType().ShortDisplayName(),
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
                        new EntityEntry(internalEntityEntry),
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
        ///     Logs for the <see cref="CoreEventId.ValueGenerated" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value generated. </param>
        /// <param name="temporary"> Indicates whether or not the value is a temporary or permanent value. </param>
        public static void ValueGenerated(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object value,
            bool temporary)
        {
            var definition = temporary
                ? CoreResources.LogTempValueGenerated(diagnostics)
                : CoreResources.LogValueGenerated(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalEntityEntry.StateManager.Context.GetType().ShortDisplayName(),
                    property.Name,
                    internalEntityEntry.EntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyValueEventData(
                        definition,
                        ValueGenerated,
                        new EntityEntry(internalEntityEntry),
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
        ///     Logs for the <see cref="CoreEventId.ValueGenerated" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalEntityEntry"> The internal entity entry. </param>
        /// <param name="property"> The property. </param>
        /// <param name="value"> The value generated. </param>
        /// <param name="temporary"> Indicates whether or not the value is a temporary or permanent value. </param>
        public static void ValueGeneratedSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> diagnostics,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] IProperty property,
            [CanBeNull] object value,
            bool temporary)
        {
            var definition = temporary
                ? CoreResources.LogTempValueGeneratedSensitive(diagnostics)
                : CoreResources.LogValueGeneratedSensitive(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalEntityEntry.StateManager.Context.GetType().ShortDisplayName(),
                    value,
                    property.Name,
                    internalEntityEntry.EntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new PropertyValueEventData(
                        definition,
                        ValueGeneratedSensitive,
                        new EntityEntry(internalEntityEntry),
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
        ///     Logs for the <see cref="CoreEventId.CascadeDelete" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalChildEntry"> The child internal entity entry. </param>
        /// <param name="internalParentEntry"> The parent internal entity entry. </param>
        /// <param name="state"> The target state. </param>
        public static void CascadeDelete(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] InternalEntityEntry internalChildEntry,
            [NotNull] InternalEntityEntry internalParentEntry,
            EntityState state)
        {
            var definition = CoreResources.LogCascadeDelete(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalChildEntry.EntityType.ShortName(),
                    state,
                    internalParentEntry.EntityType.ShortName());
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CascadeDeleteEventData(
                        definition,
                        CascadeDelete,
                        new EntityEntry(internalChildEntry),
                        new EntityEntry(internalParentEntry),
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
        ///     Logs for the <see cref="CoreEventId.CascadeDelete" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalChildEntry"> The child internal entity entry. </param>
        /// <param name="internalParentEntry"> The parent internal entity entry. </param>
        /// <param name="state"> The target state. </param>
        public static void CascadeDeleteSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] InternalEntityEntry internalChildEntry,
            [NotNull] InternalEntityEntry internalParentEntry,
            EntityState state)
        {
            var definition = CoreResources.LogCascadeDeleteSensitive(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalChildEntry.EntityType.ShortName(),
                    internalChildEntry.BuildCurrentValuesString(internalChildEntry.EntityType.FindPrimaryKey().Properties),
                    state,
                    internalParentEntry.EntityType.ShortName(),
                    internalParentEntry.BuildCurrentValuesString(internalParentEntry.EntityType.FindPrimaryKey().Properties));
            }

            if (diagnostics.DiagnosticSource.IsEnabled(definition.EventId.Name))
            {
                diagnostics.DiagnosticSource.Write(
                    definition.EventId.Name,
                    new CascadeDeleteEventData(
                        definition,
                        CascadeDeleteSensitive,
                        new EntityEntry(internalChildEntry),
                        new EntityEntry(internalParentEntry),
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
        ///     Logs for the <see cref="CoreEventId.CascadeDeleteOrphan" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalChildEntry"> The child internal entity entry. </param>
        /// <param name="parentEntityType"> The parent entity type. </param>
        /// <param name="state"> The target state. </param>
        public static void CascadeDeleteOrphan(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] InternalEntityEntry internalChildEntry,
            [NotNull] IEntityType parentEntityType,
            EntityState state)
        {
            var definition = CoreResources.LogCascadeDeleteOrphan(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalChildEntry.EntityType.ShortName(),
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
                        new EntityEntry(internalChildEntry),
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
        ///     Logs for the <see cref="CoreEventId.CascadeDeleteOrphan" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="internalChildEntry"> The child internal entity entry. </param>
        /// <param name="parentEntityType"> The parent entity type. </param>
        /// <param name="state"> The target state. </param>
        public static void CascadeDeleteOrphanSensitive(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] InternalEntityEntry internalChildEntry,
            [NotNull] IEntityType parentEntityType,
            EntityState state)
        {
            var definition = CoreResources.LogCascadeDeleteOrphanSensitive(diagnostics);

            var warningBehavior = definition.GetLogBehavior(diagnostics);
            if (warningBehavior != WarningBehavior.Ignore)
            {
                definition.Log(
                    diagnostics,
                    warningBehavior,
                    internalChildEntry.EntityType.ShortName(),
                    internalChildEntry.BuildCurrentValuesString(internalChildEntry.EntityType.FindPrimaryKey().Properties),
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
                        new EntityEntry(internalChildEntry),
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
        ///     Logs for the <see cref="CoreEventId.SaveChangesStarting" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        public static void SaveChangesStarting(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context)
        {
            var definition = CoreResources.LogSaveChangesStarting(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.SaveChangesCompleted" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        /// <param name="entitiesSavedCount"> The number of entities saved. </param>
        public static void SaveChangesCompleted(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Update> diagnostics,
            [NotNull] DbContext context,
            int entitiesSavedCount)
        {
            var definition = CoreResources.LogSaveChangesCompleted(diagnostics);

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
        ///     Logs for the <see cref="CoreEventId.ContextDisposed" /> event.
        /// </summary>
        /// <param name="diagnostics"> The diagnostics logger to use. </param>
        /// <param name="context"> The context being used. </param>
        public static void ContextDisposed(
            [NotNull] this IDiagnosticsLogger<DbLoggerCategory.Infrastructure> diagnostics,
            [NotNull] DbContext context)
        {
            var definition = CoreResources.LogContextDisposed(diagnostics);

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
