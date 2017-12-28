// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Event IDs for events that correspond to messages logged to an <see cref="ILogger" />
    ///         and events sent to a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
    ///         behavior of warnings.
    ///     </para>
    /// </summary>
    public static class CoreEventId
    {
        /// <summary>
        ///     The lower-bound for event IDs used by any Entity Framework or provider code.
        /// </summary>
        public const int CoreBaseId = 10000;

        /// <summary>
        ///     The lower-bound for event IDs used by any relational database provider.
        /// </summary>
        public const int RelationalBaseId = 20000;

        /// <summary>
        ///     The lower-bound for event IDs used only by database providers.
        /// </summary>
        public const int ProviderBaseId = 30000;

        /// <summary>
        ///     The lower-bound for event IDs used only by database provider design-time and tooling.
        /// </summary>
        public const int ProviderDesignBaseId = 35000;

        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Update events
            SaveChangesFailed = CoreBaseId,
            DuplicateDependentEntityTypeInstanceWarning,

            // Query events
            QueryIterationFailed = CoreBaseId + 100,
            QueryModelCompiling,
            RowLimitingOperationWithoutOrderByWarning,
            FirstWithoutOrderByAndFilterWarning,
            QueryModelOptimized,
            NavigationIncluded,
            IncludeIgnoredWarning,
            QueryExecutionPlanned,
            PossibleUnintendedCollectionNavigationNullComparisonWarning,
            PossibleUnintendedReferenceComparisonWarning,

            // Infrastructure events
            SensitiveDataLoggingEnabledWarning = CoreBaseId + 400,
            ServiceProviderCreated,
            ManyServiceProvidersCreatedWarning,
            ContextInitialized,
            ExecutionStrategyRetrying,
            LazyLoadOnDisposedContextWarning,
            NavigationLazyLoading
        }

        private static readonly string _updatePrefix = DbLoggerCategory.Update.Name + ".";
        private static EventId MakeUpdateId(Id id) => new EventId((int)id, _updatePrefix + id);

        /// <summary>
        ///     <para>
        ///         An error occurred while attempting to save changes to the database.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DbContextErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SaveChangesFailed = MakeUpdateId(Id.SaveChangesFailed);

        /// <summary>
        ///     The same entity is being tracked as a different dependent entity type.
        ///     This event is in the <see cref="DbLoggerCategory.Update" /> category.
        /// </summary>
        public static readonly EventId DuplicateDependentEntityTypeInstanceWarning = MakeUpdateId(Id.DuplicateDependentEntityTypeInstanceWarning);

        private static readonly string _queryPrefix = DbLoggerCategory.Query.Name + ".";
        private static EventId MakeQueryId(Id id) => new EventId((int)id, _queryPrefix + id);

        /// <summary>
        ///     <para>
        ///         An error occurred while processing the results of a query.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DbContextTypeErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryIterationFailed = MakeQueryId(Id.QueryIterationFailed);

        /// <summary>
        ///     <para>
        ///         A query model is being compiled.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="QueryModelEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryModelCompiling = MakeQueryId(Id.QueryModelCompiling);

        /// <summary>
        ///     <para>
        ///         A query uses a row limiting operation (Skip/Take) without OrderBy which may lead to unpredictable results.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="QueryModelEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId RowLimitingOperationWithoutOrderByWarning = MakeQueryId(Id.RowLimitingOperationWithoutOrderByWarning);

        /// <summary>
        ///     <para>
        ///         A query uses First/FirstOrDefault operation without OrderBy and filter which may lead to unpredictable results.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="QueryModelEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId FirstWithoutOrderByAndFilterWarning = MakeQueryId(Id.FirstWithoutOrderByAndFilterWarning);

        /// <summary>
        ///     <para>
        ///         A query model was optimized.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="QueryModelEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryModelOptimized = MakeQueryId(Id.QueryModelOptimized);

        /// <summary>
        ///     <para>
        ///         A navigation was included in the query.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="IncludeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId NavigationIncluded = MakeQueryId(Id.NavigationIncluded);

        /// <summary>
        ///     <para>
        ///         A navigation was ignored while compiling a query.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="IncludeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId IncludeIgnoredWarning = MakeQueryId(Id.IncludeIgnoredWarning);

        /// <summary>
        ///     <para>
        ///         A query is planned for execution.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="QueryExpressionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryExecutionPlanned = MakeQueryId(Id.QueryExecutionPlanned);

        /// <summary>
        ///     <para>
        ///         Possible unintended comparison of collection navigation to null.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NavigationPathEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId PossibleUnintendedCollectionNavigationNullComparisonWarning
            = MakeQueryId(Id.PossibleUnintendedCollectionNavigationNullComparisonWarning);

        /// <summary>
        ///     <para>
        ///         Possible unintended reference comparison.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="BinaryExpressionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId PossibleUnintendedReferenceComparisonWarning
            = MakeQueryId(Id.PossibleUnintendedReferenceComparisonWarning);

        private static readonly string _infraPrefix = DbLoggerCategory.Infrastructure.Name + ".";
        private static EventId MakeInfraId(Id id) => new EventId((int)id, _infraPrefix + id);

        /// <summary>
        ///     <para>
        ///         A warning indicating that sensitive data logging is enabled and may be logged.
        ///     </para>
        ///     <para>
        ///         This event may be in different categories depending on where sensitive data is being logged.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="EventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SensitiveDataLoggingEnabledWarning = MakeInfraId(Id.SensitiveDataLoggingEnabledWarning);

        /// <summary>
        ///     <para>
        ///         A service provider was created for internal use by Entity Framework.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ServiceProviderEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ServiceProviderCreated = MakeInfraId(Id.ServiceProviderCreated);

        /// <summary>
        ///     <para>
        ///         Many service providers were created in a single app domain.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ServiceProvidersEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ManyServiceProvidersCreatedWarning = MakeInfraId(Id.ManyServiceProvidersCreatedWarning);

        /// <summary>
        ///     <para>
        ///         A <see cref="DbContext" /> was initialized.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ContextInitializedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ContextInitialized = MakeInfraId(Id.ContextInitialized);

        /// <summary>
        ///     <para>
        ///         A transient exception has been encountered during execution and the operation will be retried.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ContextInitializedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ExecutionStrategyRetrying = MakeInfraId(Id.ExecutionStrategyRetrying);

        /// <summary>
        ///     <para>
        ///         A navigation property is being lazy-loaded.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="LazyLoadingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId NavigationLazyLoading = MakeInfraId(Id.NavigationLazyLoading);

        /// <summary>
        ///     <para>
        ///         An attempt was made to lazy-load a property after the DbContext had been disposed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="LazyLoadingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId LazyLoadOnDisposedContextWarning = MakeInfraId(Id.LazyLoadOnDisposedContextWarning);
    }
}
