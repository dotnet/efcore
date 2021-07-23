// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedMember.Local
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
            CascadeDelete,
            CascadeDeleteOrphan,
            SaveChangesStarting,
            SaveChangesCompleted,
            OptimisticConcurrencyException,

            // Query events
            QueryIterationFailed = CoreBaseId + 100,
            Obsolete_QueryModelCompiling,
            RowLimitingOperationWithoutOrderByWarning,
            FirstWithoutOrderByAndFilterWarning,
            Obsolete_QueryModelOptimized,
            Obsolete_NavigationIncluded,
            Obsolete_IncludeIgnoredWarning,
            QueryExecutionPlanned,
            PossibleUnintendedCollectionNavigationNullComparisonWarning,
            PossibleUnintendedReferenceComparisonWarning,
            InvalidIncludePathError,
            QueryCompilationStarting,
            NavigationBaseIncluded,
            NavigationBaseIncludeIgnored,
            DistinctAfterOrderByWithoutRowLimitingOperatorWarning,

            // Infrastructure events
            SensitiveDataLoggingEnabledWarning = CoreBaseId + 400,
            ServiceProviderCreated,
            ManyServiceProvidersCreatedWarning,
            ContextInitialized,
            ExecutionStrategyRetrying,
            LazyLoadOnDisposedContextWarning,
            NavigationLazyLoading,
            ContextDisposed,
            DetachedLazyLoadingWarning,
            ServiceProviderDebugInfo,
            RedundantAddServicesCallWarning,
            OldModelVersionWarning,

            // Model and ModelValidation events
            ShadowPropertyCreated = CoreBaseId + 600,
            RedundantIndexRemoved,
            IncompatibleMatchingForeignKeyProperties,
            Obsolete_RequiredAttributeOnDependent,
            Obsolete_RequiredAttributeOnBothNavigations,
            ConflictingShadowForeignKeysWarning,
            MultiplePrimaryKeyCandidates,
            MultipleNavigationProperties,
            MultipleInversePropertiesSameTargetWarning,
            NonDefiningInverseNavigationWarning,
            NonOwnershipInverseNavigationWarning,
            ForeignKeyAttributesOnBothPropertiesWarning,
            ForeignKeyAttributesOnBothNavigationsWarning,
            ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning,
            RedundantForeignKeyWarning,
            Obsolete_NonNullableInverted,
            Obsolete_NonNullableReferenceOnBothNavigations,
            Obsolete_NonNullableReferenceOnDependent,
            Obsolete_RequiredAttributeInverted,
            RequiredAttributeOnCollection,
            CollectionWithoutComparer,
            ConflictingKeylessAndKeyAttributesWarning,
            PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning,
            RequiredAttributeOnSkipNavigation,
            AmbiguousEndRequiredWarning,

            // ChangeTracking events
            DetectChangesStarting = CoreBaseId + 800,
            DetectChangesCompleted,
            PropertyChangeDetected,
            ForeignKeyChangeDetected,
            CollectionChangeDetected,
            ReferenceChangeDetected,
            StartedTracking,
            StateChanged,
            ValueGenerated,
            SkipCollectionChangeDetected,
        }

        private static readonly string _updatePrefix = DbLoggerCategory.Update.Name + ".";

        private static EventId MakeUpdateId(Id id)
            => new((int)id, _updatePrefix + id);

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
        ///     The same entity is being tracked as a different shared entity entity type.
        ///     This event is in the <see cref="DbLoggerCategory.Update" /> category.
        /// </summary>
        public static readonly EventId DuplicateDependentEntityTypeInstanceWarning =
            MakeUpdateId(Id.DuplicateDependentEntityTypeInstanceWarning);

        private static readonly string _queryPrefix = DbLoggerCategory.Query.Name + ".";

        private static EventId MakeQueryId(Id id)
            => new((int)id, _queryPrefix + id);

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
        ///         This event uses the <see cref="NavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
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

        /// <summary>
        ///     <para>
        ///         Invalid include path '{navigationChain}', couldn't find navigation for '{navigationName}'.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="InvalidIncludePathEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId InvalidIncludePathError
            = MakeQueryId(Id.InvalidIncludePathError);

        /// <summary>
        ///     <para>
        ///         Starting query compilation.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="QueryExpressionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId QueryCompilationStarting
            = MakeQueryId(Id.QueryCompilationStarting);

        /// <summary>
        ///     <para>
        ///         A navigation base was included in the query.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NavigationBaseEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId NavigationBaseIncluded
            = MakeQueryId(Id.NavigationBaseIncluded);

        /// <summary>
        ///     <para>
        ///         A navigation base specific in Include in the query was ignored because it will be populated already due to fix-up.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NavigationBaseEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId NavigationBaseIncludeIgnored
            = MakeQueryId(Id.NavigationBaseIncludeIgnored);

        /// <summary>
        ///     <para>
        ///         A query uses a row limiting operation (Skip/Take) without OrderBy which may lead to unpredictable results.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId RowLimitingOperationWithoutOrderByWarning
            = MakeQueryId(Id.RowLimitingOperationWithoutOrderByWarning);

        /// <summary>
        ///     <para>
        ///         A query uses First/FirstOrDefault operation without OrderBy and filter which may lead to unpredictable results.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId FirstWithoutOrderByAndFilterWarning
            = MakeQueryId(Id.FirstWithoutOrderByAndFilterWarning);

        /// <summary>
        ///     <para>
        ///         The query uses the 'Distinct' operator after applying an ordering. If there are any row limiting operation used before `Distinct` and after ordering then ordering will be used for it.
        ///         Ordering(s) will be erased after `Distinct` and results afterwards would be unordered.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
        ///     </para>
        /// </summary>
        public static readonly EventId DistinctAfterOrderByWithoutRowLimitingOperatorWarning
            = MakeQueryId(Id.DistinctAfterOrderByWithoutRowLimitingOperatorWarning);

        private static readonly string _infraPrefix = DbLoggerCategory.Infrastructure.Name + ".";

        private static EventId MakeInfraId(Id id)
            => new((int)id, _infraPrefix + id);

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
        ///         Provides debug information for why a new internal service provider was created.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ServiceProviderDebugInfoEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ServiceProviderDebugInfo = MakeInfraId(Id.ServiceProviderDebugInfo);

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

        /// <summary>
        ///     <para>
        ///         An attempt was made to lazy-load a property from a detached/no-tracking entity.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="LazyLoadingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DetachedLazyLoadingWarning = MakeInfraId(Id.DetachedLazyLoadingWarning);

        /// <summary>
        ///     <para>
        ///         'AddEntityFramework*' was called on the service provider, but 'UseInternalServiceProvider' wasn't.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ServiceProviderEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId RedundantAddServicesCallWarning = MakeInfraId(Id.RedundantAddServicesCallWarning);

        /// <summary>
        ///     <para>
        ///         The model supplied in the context options was created with an older EF Core version.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ContextInitializedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId OldModelVersionWarning = MakeInfraId(Id.OldModelVersionWarning);

        private static readonly string _modelPrefix = DbLoggerCategory.Model.Name + ".";

        private static EventId MakeModelId(Id id)
            => new((int)id, _modelPrefix + id);

        private static readonly string _modelValidationPrefix = DbLoggerCategory.Model.Validation.Name + ".";

        private static EventId MakeModelValidationId(Id id)
            => new((int)id, _modelValidationPrefix + id);

        /// <summary>
        ///     <para>
        ///         A shadow property has been created.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ShadowPropertyCreated = MakeModelValidationId(Id.ShadowPropertyCreated);

        /// <summary>
        ///     <para>
        ///         An index was not created as the properties are already covered.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId RedundantIndexRemoved = MakeModelId(Id.RedundantIndexRemoved);

        /// <summary>
        ///     <para>
        ///         The best match for foreign key properties are incompatible with the principal key.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId IncompatibleMatchingForeignKeyProperties = MakeModelId(Id.IncompatibleMatchingForeignKeyProperties);

        /// <summary>
        ///     <para>
        ///         Foreign key configured as required before the dependent end was determined.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId AmbiguousEndRequiredWarning = MakeModelId(Id.AmbiguousEndRequiredWarning);

        /// <summary>
        ///     <para>
        ///         The entity type with the navigation property that has the <see cref="RequiredAttribute" />
        ///         was configured as the dependent side in the relationship.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        [Obsolete]
        public static readonly EventId RequiredAttributeInverted = MakeModelId(Id.Obsolete_RequiredAttributeInverted);

        /// <summary>
        ///     <para>
        ///         The entity type with the navigation property that has non-nullability
        ///         was configured as the dependent side in the relationship.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        [Obsolete]
        public static readonly EventId NonNullableInverted = MakeModelId(Id.Obsolete_NonNullableInverted);

        /// <summary>
        ///     <para>
        ///         Navigations separated into two relationships as <see cref="RequiredAttribute" /> was specified on both navigations.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        [Obsolete]
        public static readonly EventId RequiredAttributeOnBothNavigations = MakeModelId(Id.Obsolete_RequiredAttributeOnBothNavigations);

        /// <summary>
        ///     <para>
        ///         Navigations separated into two relationships as non-nullability was specified on both navigations.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        [Obsolete]
        public static readonly EventId NonNullableReferenceOnBothNavigations =
            MakeModelId(Id.Obsolete_NonNullableReferenceOnBothNavigations);

        /// <summary>
        ///     <para>
        ///         The <see cref="RequiredAttribute" /> on the navigation property to the dependent entity was ignored.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        [Obsolete]
        public static readonly EventId RequiredAttributeOnDependent = MakeModelId(Id.Obsolete_RequiredAttributeOnDependent);

        /// <summary>
        ///     <para>
        ///         The non-nullability of the navigation property to the dependent entity was ignored.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        [Obsolete]
        public static readonly EventId NonNullableReferenceOnDependent = MakeModelId(Id.Obsolete_NonNullableReferenceOnDependent);

        /// <summary>
        ///     <para>
        ///         The <see cref="RequiredAttribute" /> on the collection navigation property was ignored.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="NavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId RequiredAttributeOnCollection = MakeModelId(Id.RequiredAttributeOnCollection);

        /// <summary>
        ///     <para>
        ///         The <see cref="RequiredAttribute" /> on the skip navigation property was ignored.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="SkipNavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId RequiredAttributeOnSkipNavigation = MakeModelId(Id.RequiredAttributeOnSkipNavigation);

        /// <summary>
        ///     <para>
        ///         The properties that best match the foreign key convention are already used by a different foreign key.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConflictingShadowForeignKeysWarning = MakeModelId(Id.ConflictingShadowForeignKeysWarning);

        /// <summary>
        ///     <para>
        ///         There are multiple properties that could be used as the primary key.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MultiplePrimaryKeyCandidates = MakeModelId(Id.MultiplePrimaryKeyCandidates);

        /// <summary>
        ///     <para>
        ///         There are multiple properties that could be navigations to the same type.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoUnmappedPropertyCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MultipleNavigationProperties = MakeModelId(Id.MultipleNavigationProperties);

        /// <summary>
        ///     <para>
        ///         There are multiple navigations with <see cref="InversePropertyAttribute" /> that point
        ///         to the same inverse navigation.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId MultipleInversePropertiesSameTargetWarning =
            MakeModelId(Id.MultipleInversePropertiesSameTargetWarning);

        /// <summary>
        ///     <para>
        ///         The navigation that <see cref="InversePropertyAttribute" /> points to is not the defining navigation.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoUnmappedPropertyCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId NonDefiningInverseNavigationWarning = MakeModelId(Id.NonDefiningInverseNavigationWarning);

        /// <summary>
        ///     <para>
        ///         The navigation that <see cref="InversePropertyAttribute" /> points to is not the defining navigation.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoUnmappedPropertyCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId NonOwnershipInverseNavigationWarning = MakeModelId(Id.NonOwnershipInverseNavigationWarning);

        /// <summary>
        ///     <para>
        ///         Navigations separated into two relationships as <see cref="ForeignKeyAttribute" /> was specified on properties
        ///         on both sides.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ForeignKeyAttributesOnBothPropertiesWarning =
            MakeModelId(Id.ForeignKeyAttributesOnBothPropertiesWarning);

        /// <summary>
        ///     <para>
        ///         Navigations separated into two relationships as <see cref="ForeignKeyAttribute" /> was specified on navigations
        ///         on both sides.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ForeignKeyAttributesOnBothNavigationsWarning =
            MakeModelId(Id.ForeignKeyAttributesOnBothNavigationsWarning);

        /// <summary>
        ///     <para>
        ///         The <see cref="ForeignKeyAttribute" /> specified on the navigation doesn't match the <see cref="ForeignKeyAttribute" />
        ///         specified on the property.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning =
            MakeModelId(Id.ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning);

        /// <summary>
        ///     <para>
        ///         The configured <see cref="IForeignKey" /> is redundant.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId RedundantForeignKeyWarning = MakeModelValidationId(Id.RedundantForeignKeyWarning);

        /// <summary>
        ///     <para>
        ///         A <see cref="KeylessAttribute" /> attribute on the entity type is conflicting
        ///         with a <see cref="KeyAttribute" /> attribute on at least one of its properties.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ConflictingKeylessAndKeyAttributesWarning =
            MakeModelId(Id.ConflictingKeylessAndKeyAttributesWarning);

        /// <summary>
        ///     <para>
        ///         Required navigation with principal entity having global query filter defined
        ///         and the declaring entity not having a matching filter
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning
            = MakeModelValidationId(Id.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);

        private static readonly string _changeTrackingPrefix = DbLoggerCategory.ChangeTracking.Name + ".";

        private static EventId MakeChangeTrackingId(Id id)
            => new((int)id, _changeTrackingPrefix + id);

        /// <summary>
        ///     <para>
        ///         DetectChanges is starting.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DbContextEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DetectChangesStarting = MakeChangeTrackingId(Id.DetectChangesStarting);

        /// <summary>
        ///     <para>
        ///         DetectChanges has completed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DbContextEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId DetectChangesCompleted = MakeChangeTrackingId(Id.DetectChangesCompleted);

        /// <summary>
        ///     <para>
        ///         DetectChanges has detected a change in a property value.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyChangedEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId PropertyChangeDetected = MakeChangeTrackingId(Id.PropertyChangeDetected);

        /// <summary>
        ///     <para>
        ///         DetectChanges has detected a change in a foreign key property value.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyChangedEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ForeignKeyChangeDetected = MakeChangeTrackingId(Id.ForeignKeyChangeDetected);

        /// <summary>
        ///     <para>
        ///         DetectChanges has detected entities were added and/or removed from a collection
        ///         navigation property.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CollectionChangedEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CollectionChangeDetected = MakeChangeTrackingId(Id.CollectionChangeDetected);

        /// <summary>
        ///     <para>
        ///         DetectChanges has detected entities were added and/or removed from a collection skip navigation property.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="SkipCollectionChangedEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SkipCollectionChangeDetected = MakeChangeTrackingId(Id.SkipCollectionChangeDetected);

        /// <summary>
        ///     <para>
        ///         DetectChanges has detected a change to the entity references by another entity.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="ReferenceChangedEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ReferenceChangeDetected = MakeChangeTrackingId(Id.ReferenceChangeDetected);

        /// <summary>
        ///     <para>
        ///         An entity is being tracked by the <see cref="DbContext" />.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="EntityEntryEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId StartedTracking = MakeChangeTrackingId(Id.StartedTracking);

        /// <summary>
        ///     <para>
        ///         An entity tracked by the <see cref="DbContext" /> is changing from one
        ///         <see cref="EntityState" /> to another.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="StateChangedEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId StateChanged = MakeChangeTrackingId(Id.StateChanged);

        /// <summary>
        ///     <para>
        ///         A property of a tracked entity is getting a generated value.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyValueEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ValueGenerated = MakeChangeTrackingId(Id.ValueGenerated);

        /// <summary>
        ///     <para>
        ///         An entity is being deleted or detached because its parent was deleted.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CascadeDeleteEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CascadeDelete = MakeUpdateId(Id.CascadeDelete);

        /// <summary>
        ///     <para>
        ///         An entity is being deleted or detached because the required relationship to its
        ///         parent was severed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="CascadeDeleteOrphanEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CascadeDeleteOrphan = MakeUpdateId(Id.CascadeDeleteOrphan);

        /// <summary>
        ///     <para>
        ///         <see cref="DbContext.SaveChanges()" /> or one of its overloads started.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DbContextEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SaveChangesStarting = MakeUpdateId(Id.SaveChangesStarting);

        /// <summary>
        ///     <para>
        ///         <see cref="DbContext.SaveChanges()" /> or one of its overloads has completed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="SaveChangesCompletedEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId SaveChangesCompleted = MakeUpdateId(Id.SaveChangesCompleted);

        /// <summary>
        ///     <para>
        ///         An <see cref="OptimisticConcurrencyException" /> was thrown during the call to
        ///         <see cref="DbContext.SaveChanges()" />
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DbContextErrorEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId OptimisticConcurrencyException = MakeUpdateId(Id.OptimisticConcurrencyException);

        /// <summary>
        ///     <para>
        ///         The <see cref="DbContext" /> is being disposed.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="DbContextEventData" /> payload when used with a
        ///         <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId ContextDisposed = MakeInfraId(Id.ContextDisposed);

        /// <summary>
        ///     <para>
        ///         A property has a collection or enumeration type with a value converter but with no value comparer.
        ///     </para>
        ///     <para>
        ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
        ///     </para>
        ///     <para>
        ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
        ///     </para>
        /// </summary>
        public static readonly EventId CollectionWithoutComparer = MakeModelValidationId(Id.CollectionWithoutComparer);
    }
}
