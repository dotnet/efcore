// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Infrastructure
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
        public const int CoreBaseId = 100000;

        /// <summary>
        ///     The lower-bound for event IDs used by any Entity Framework or provider code design-time and tooling.
        /// </summary>
        public const int CoreDesignBaseId = 150000;

        /// <summary>
        ///     The lower-bound for event IDs used by any relational database provider.
        /// </summary>
        public const int RelationalBaseId = 200000;

        /// <summary>
        ///     The lower-bound for event IDs used by any relational database provider design-time and tooling.
        /// </summary>
        public const int RelationalDesignBaseId = 250000;

        /// <summary>
        ///     The lower-bound for event IDs used only by database providers.
        /// </summary>
        public const int ProviderBaseId = 300000;

        /// <summary>
        ///     The lower-bound for event IDs used only by database provider design-time and tooling.
        /// </summary>
        public const int ProviderDesignBaseId = 350000;
        
        // Warning: These values must not change between releases.
        // Only add new values to the end of sections, never in the middle.
        // Try to use <Noun><Verb> naming and be consistent with existing names.
        private enum Id
        {
            // Update events
            SaveChangesFailed = CoreBaseId,

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

            // Model validation events
            ModelValidationShadowKeyWarning = CoreBaseId + 300,

            // Infrastucture events
            SensitiveDataLoggingEnabledWarning = CoreBaseId + 400,
            ServiceProviderCreated,
            ManyServiceProvidersCreatedWarning
        }

        private static readonly string _updatePrefix = DbLoggerCategory.Update.Name + ".";
        private static EventId MakeUpdateId(Id id) => new EventId((int)id, _updatePrefix + id);

        /// <summary>
        ///     An error occurred while attempting to save changes to the database.
        ///     This event is in the <see cref="DbLoggerCategory.Update" /> category.
        /// </summary>
        public static readonly EventId SaveChangesFailed = MakeUpdateId(Id.SaveChangesFailed);

        private static readonly string _queryPrefix = DbLoggerCategory.Query.Name + ".";
        private static EventId MakeQueryId(Id id) => new EventId((int)id, _queryPrefix + id);

        /// <summary>
        ///     An error occurred while processing the results of a query.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId QueryIterationFailed = MakeQueryId(Id.QueryIterationFailed);

        /// <summary>
        ///     A query model is being compiled.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId QueryModelCompiling = MakeQueryId(Id.QueryModelCompiling);

        /// <summary>
        ///     A query uses a row limiting operation (Skip/Take) without OrderBy which may lead to unpredictable results.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId RowLimitingOperationWithoutOrderByWarning = MakeQueryId(Id.RowLimitingOperationWithoutOrderByWarning);

        /// <summary>
        ///     A query uses First/FirstOrDefault operation without OrderBy and filter which may lead to unpredictable results.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId FirstWithoutOrderByAndFilterWarning = MakeQueryId(Id.FirstWithoutOrderByAndFilterWarning);

        /// <summary>
        ///     A query model was optimized.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId QueryModelOptimized = MakeQueryId(Id.QueryModelOptimized);

        /// <summary>
        ///     A navigation was included in the query.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId NavigationIncluded = MakeQueryId(Id.NavigationIncluded);

        /// <summary>
        ///     A navigation was ignored while compiling a query.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId IncludeIgnoredWarning = MakeQueryId(Id.IncludeIgnoredWarning);

        /// <summary>
        ///     A query is planned for execution.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId QueryExecutionPlanned = MakeQueryId(Id.QueryExecutionPlanned);

        /// <summary>
        ///     Possible uninteded comparison of collection navigation to null.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId PossibleUnintendedCollectionNavigationNullComparisonWarning
            = MakeQueryId(Id.PossibleUnintendedCollectionNavigationNullComparisonWarning);

        /// <summary>
        ///     Possible uninteded reference comparison.
        ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
        /// </summary>
        public static readonly EventId PossibleUnintendedReferenceComparisonWarning
            = MakeQueryId(Id.PossibleUnintendedReferenceComparisonWarning);

        private static readonly string _validationPrefix = DbLoggerCategory.Model.Validation.Name + ".";
        private static EventId MakeValidationId(Id id) => new EventId((int)id, _validationPrefix + id);

        /// <summary>
        ///     A warning during model validation indicating a key is configured on shadow properties.
        ///     This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        /// </summary>
        public static readonly EventId ModelValidationShadowKeyWarning = MakeValidationId(Id.ModelValidationShadowKeyWarning);

        private static readonly string _infraPrefix = DbLoggerCategory.Infrastructure.Name + ".";
        private static EventId MakeInfraId(Id id) => new EventId((int)id, _infraPrefix + id);

        /// <summary>
        ///     A warning indicating that sensitive data logging is enabled and may be logged.
        ///     This event may be in different categories depending on where sensitive data is being logged.
        /// </summary>
        public static readonly EventId SensitiveDataLoggingEnabledWarning = MakeInfraId(Id.SensitiveDataLoggingEnabledWarning);

        /// <summary>
        ///     A service provider was created for internal use by Entity Framework.
        ///     This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        /// </summary>
        public static readonly EventId ServiceProviderCreated = MakeInfraId(Id.ServiceProviderCreated);

        /// <summary>
        ///     Many service proviers were created in a single app domain.
        ///     This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        /// </summary>
        public static readonly EventId ManyServiceProvidersCreatedWarning = MakeInfraId(Id.ManyServiceProvidersCreatedWarning);
    }
}
