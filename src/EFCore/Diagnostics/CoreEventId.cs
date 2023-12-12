// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Event IDs for events that correspond to messages logged to an <see cref="ILogger" />
///     and events sent to a <see cref="DiagnosticSource" />.
/// </summary>
/// <remarks>
///     <para>
///         These IDs are also used with <see cref="WarningsConfigurationBuilder" /> to configure the
///         behavior of warnings.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-diagnostics">Logging, events, and diagnostics</see> for more information and examples.
///     </para>
/// </remarks>
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
        SaveChangesCanceled,

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
        QueryCanceled,
        StringEnumValueInJson,

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
        Obsolete_NonDefiningInverseNavigationWarning,
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
        ShadowForeignKeyPropertyCreated,
        MappedEntityTypeIgnoredWarning,
        MappedNavigationIgnoredWarning,
        MappedPropertyIgnoredWarning,
        MappedComplexPropertyIgnoredWarning,
        TypeLoadingErrorWarning,
        SkippedEntityTypeConfigurationWarning,
        NoEntityTypeConfigurationsWarning,

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
        SkipCollectionChangeDetected
    }

    private static readonly string _updatePrefix = DbLoggerCategory.Update.Name + ".";

    private static EventId MakeUpdateId(Id id)
        => new((int)id, _updatePrefix + id);

    /// <summary>
    ///     An error occurred while attempting to save changes to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId SaveChangesFailed = MakeUpdateId(Id.SaveChangesFailed);

    /// <summary>
    ///     An error occurred while attempting to save changes to the database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId SaveChangesCanceled = MakeUpdateId(Id.SaveChangesCanceled);

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
    ///     An error occurred while processing the results of a query.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextTypeErrorEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId QueryIterationFailed = MakeQueryId(Id.QueryIterationFailed);

    /// <summary>
    ///     A query is planned for execution.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="QueryExpressionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId QueryExecutionPlanned = MakeQueryId(Id.QueryExecutionPlanned);

    /// <summary>
    ///     Possible unintended comparison of collection navigation to null.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="NavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId PossibleUnintendedCollectionNavigationNullComparisonWarning
        = MakeQueryId(Id.PossibleUnintendedCollectionNavigationNullComparisonWarning);

    /// <summary>
    ///     Possible unintended reference comparison.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="BinaryExpressionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId PossibleUnintendedReferenceComparisonWarning
        = MakeQueryId(Id.PossibleUnintendedReferenceComparisonWarning);

    /// <summary>
    ///     Invalid include path '{navigationChain}', couldn't find navigation for '{navigationName}'.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="InvalidIncludePathEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId InvalidIncludePathError
        = MakeQueryId(Id.InvalidIncludePathError);

    /// <summary>
    ///     Starting query compilation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="QueryExpressionEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId QueryCompilationStarting
        = MakeQueryId(Id.QueryCompilationStarting);

    /// <summary>
    ///     A navigation base was included in the query.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="NavigationBaseEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId NavigationBaseIncluded
        = MakeQueryId(Id.NavigationBaseIncluded);

    /// <summary>
    ///     A navigation base specific in Include in the query was ignored because it will be populated already due to fix-up.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Query" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="NavigationBaseEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId NavigationBaseIncludeIgnored
        = MakeQueryId(Id.NavigationBaseIncludeIgnored);

    /// <summary>
    ///     A query uses a row limiting operation (Skip/Take) without OrderBy which may lead to unpredictable results.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
    /// </remarks>
    public static readonly EventId RowLimitingOperationWithoutOrderByWarning
        = MakeQueryId(Id.RowLimitingOperationWithoutOrderByWarning);

    /// <summary>
    ///     A query uses First/FirstOrDefault operation without OrderBy and filter which may lead to unpredictable results.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
    /// </remarks>
    public static readonly EventId FirstWithoutOrderByAndFilterWarning
        = MakeQueryId(Id.FirstWithoutOrderByAndFilterWarning);

    /// <summary>
    ///     The query uses the 'Distinct' operator after applying an ordering. If there are any row limiting operation used before `Distinct`
    ///     and after ordering then ordering will be used for it.
    ///     Ordering(s) will be erased after `Distinct` and results afterwards would be unordered.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
    /// </remarks>
    public static readonly EventId DistinctAfterOrderByWithoutRowLimitingOperatorWarning
        = MakeQueryId(Id.DistinctAfterOrderByWithoutRowLimitingOperatorWarning);

    /// <summary>
    ///     A query was canceled for context type '{contextType}'.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
    /// </remarks>
    public static readonly EventId QueryCanceled
        = MakeQueryId(Id.QueryCanceled);

    /// <summary>
    ///     A string value for an enum was read from JSON. Starting with EF Core 8, a breaking change was made to store enum
    ///     values in JSON as numbers by default. See https://aka.ms/efcore-docs-jsonenums for details.
    /// </summary>
    /// <remarks>
    ///     This event is in the <see cref="DbLoggerCategory.Query" /> category.
    /// </remarks>
    public static readonly EventId StringEnumValueInJson
        = MakeQueryId(Id.StringEnumValueInJson);

    private static readonly string _infraPrefix = DbLoggerCategory.Infrastructure.Name + ".";

    private static EventId MakeInfraId(Id id)
        => new((int)id, _infraPrefix + id);

    /// <summary>
    ///     A warning indicating that sensitive data logging is enabled and may be logged.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event may be in different categories depending on where sensitive data is being logged.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="EventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId SensitiveDataLoggingEnabledWarning = MakeInfraId(Id.SensitiveDataLoggingEnabledWarning);

    /// <summary>
    ///     A service provider was created for internal use by Entity Framework.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ServiceProviderEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ServiceProviderCreated = MakeInfraId(Id.ServiceProviderCreated);

    /// <summary>
    ///     Many service providers were created in a single app domain.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ServiceProvidersEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ManyServiceProvidersCreatedWarning = MakeInfraId(Id.ManyServiceProvidersCreatedWarning);

    /// <summary>
    ///     A <see cref="DbContext" /> was initialized.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ContextInitializedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ContextInitialized = MakeInfraId(Id.ContextInitialized);

    /// <summary>
    ///     Provides debug information for why a new internal service provider was created.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ServiceProviderDebugInfoEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ServiceProviderDebugInfo = MakeInfraId(Id.ServiceProviderDebugInfo);

    /// <summary>
    ///     A transient exception has been encountered during execution and the operation will be retried.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ContextInitializedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ExecutionStrategyRetrying = MakeInfraId(Id.ExecutionStrategyRetrying);

    /// <summary>
    ///     A navigation property is being lazy-loaded.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="LazyLoadingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId NavigationLazyLoading = MakeInfraId(Id.NavigationLazyLoading);

    /// <summary>
    ///     An attempt was made to lazy-load a property after the DbContext had been disposed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="LazyLoadingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId LazyLoadOnDisposedContextWarning = MakeInfraId(Id.LazyLoadOnDisposedContextWarning);

    /// <summary>
    ///     An attempt was made to lazy-load a property from a detached/no-tracking entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="LazyLoadingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId DetachedLazyLoadingWarning = MakeInfraId(Id.DetachedLazyLoadingWarning);

    /// <summary>
    ///     'AddEntityFramework*' was called on the service provider, but 'UseInternalServiceProvider' wasn't.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ServiceProviderEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId RedundantAddServicesCallWarning = MakeInfraId(Id.RedundantAddServicesCallWarning);

    /// <summary>
    ///     The model supplied in the context options was created with an older EF Core version.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ContextInitializedEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId OldModelVersionWarning = MakeInfraId(Id.OldModelVersionWarning);

    private static readonly string _modelPrefix = DbLoggerCategory.Model.Name + ".";

    private static EventId MakeModelId(Id id)
        => new((int)id, _modelPrefix + id);

    private static readonly string _modelValidationPrefix = DbLoggerCategory.Model.Validation.Name + ".";

    private static EventId MakeModelValidationId(Id id)
        => new((int)id, _modelValidationPrefix + id);

    /// <summary>
    ///     A shadow property has been created.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ShadowPropertyCreated = MakeModelValidationId(Id.ShadowPropertyCreated);

    /// <summary>
    ///     A foreign key property was created in shadow state because a conflicting property with the simple name for
    ///     this foreign key exists in the entity type, but is either not mapped, is already used for another relationship,
    ///     or is incompatible with the associated primary key type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="UniquifiedPropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public static readonly EventId ShadowForeignKeyPropertyCreated = MakeModelValidationId(Id.ShadowForeignKeyPropertyCreated);

    /// <summary>
    ///     An entity type  was first mapped explicitly and then ignored.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="EntityTypeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public static readonly EventId MappedEntityTypeIgnoredWarning = MakeModelId(Id.MappedEntityTypeIgnoredWarning);

    /// <summary>
    ///     A navigation was first mapped explicitly and then ignored.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="NavigationBaseEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public static readonly EventId MappedNavigationIgnoredWarning = MakeModelId(Id.MappedNavigationIgnoredWarning);

    /// <summary>
    ///     A property was first mapped explicitly and then ignored.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public static readonly EventId MappedPropertyIgnoredWarning = MakeModelId(Id.MappedPropertyIgnoredWarning);

    /// <summary>
    ///     A property was first mapped explicitly and then ignored.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ComplexPropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public static readonly EventId MappedComplexPropertyIgnoredWarning = MakeModelId(Id.MappedComplexPropertyIgnoredWarning);

    /// <summary>
    ///     An error was ignored while loading types from an assembly.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TypeLoadingEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public static readonly EventId TypeLoadingErrorWarning = MakeModelId(Id.TypeLoadingErrorWarning);

    /// <summary>
    ///     A type that implements <see cref="IEntityTypeConfiguration{TEntity}"/> could not be instantiated.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TypeEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public static readonly EventId SkippedEntityTypeConfigurationWarning = MakeModelId(Id.SkippedEntityTypeConfigurationWarning);

    /// <summary>
    ///     A type that implements <see cref="IEntityTypeConfiguration{TEntity}"/> could not be instantiated.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="AssemblyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and
    ///         examples.
    ///     </para>
    /// </remarks>
    public static readonly EventId NoEntityTypeConfigurationsWarning = MakeModelId(Id.NoEntityTypeConfigurationsWarning);

    /// <summary>
    ///     An index was not created as the properties are already covered.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId RedundantIndexRemoved = MakeModelId(Id.RedundantIndexRemoved);

    /// <summary>
    ///     The best match for foreign key properties are incompatible with the principal key.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId IncompatibleMatchingForeignKeyProperties = MakeModelId(Id.IncompatibleMatchingForeignKeyProperties);

    /// <summary>
    ///     Foreign key configured as required before the dependent end was determined.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId AmbiguousEndRequiredWarning = MakeModelId(Id.AmbiguousEndRequiredWarning);

    /// <summary>
    ///     The <see cref="RequiredAttribute" /> on the collection navigation property was ignored.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="NavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId RequiredAttributeOnCollection = MakeModelId(Id.RequiredAttributeOnCollection);

    /// <summary>
    ///     The <see cref="RequiredAttribute" /> on the skip navigation property was ignored.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="SkipNavigationEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId RequiredAttributeOnSkipNavigation = MakeModelId(Id.RequiredAttributeOnSkipNavigation);

    /// <summary>
    ///     The properties that best match the foreign key convention are already used by a different foreign key.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConflictingShadowForeignKeysWarning = MakeModelId(Id.ConflictingShadowForeignKeysWarning);

    /// <summary>
    ///     There are multiple properties that could be used as the primary key.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MultiplePrimaryKeyCandidates = MakeModelId(Id.MultiplePrimaryKeyCandidates);

    /// <summary>
    ///     There are multiple properties that could be navigations to the same type.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoUnmappedPropertyCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MultipleNavigationProperties = MakeModelId(Id.MultipleNavigationProperties);

    /// <summary>
    ///     There are multiple navigations with <see cref="InversePropertyAttribute" /> that point
    ///     to the same inverse navigation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId MultipleInversePropertiesSameTargetWarning =
        MakeModelId(Id.MultipleInversePropertiesSameTargetWarning);

    /// <summary>
    ///     The navigation that <see cref="InversePropertyAttribute" /> points to is not the defining navigation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoUnmappedPropertyCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    [Obsolete("Log message with this event Id has been removed.")]
    public static readonly EventId NonDefiningInverseNavigationWarning = MakeModelId(Id.Obsolete_NonDefiningInverseNavigationWarning);

    /// <summary>
    ///     The navigation that <see cref="InversePropertyAttribute" /> points to is not the defining navigation.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoUnmappedPropertyCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId NonOwnershipInverseNavigationWarning = MakeModelId(Id.NonOwnershipInverseNavigationWarning);

    /// <summary>
    ///     Navigations separated into two relationships as <see cref="ForeignKeyAttribute" /> was specified on properties
    ///     on both sides.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ForeignKeyAttributesOnBothPropertiesWarning =
        MakeModelId(Id.ForeignKeyAttributesOnBothPropertiesWarning);

    /// <summary>
    ///     Navigations separated into two relationships as <see cref="ForeignKeyAttribute" /> was specified on navigations
    ///     on both sides.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ForeignKeyAttributesOnBothNavigationsWarning =
        MakeModelId(Id.ForeignKeyAttributesOnBothNavigationsWarning);

    /// <summary>
    ///     The <see cref="ForeignKeyAttribute" /> specified on the navigation doesn't match the <see cref="ForeignKeyAttribute" />
    ///     specified on the property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="TwoPropertyBaseCollectionsEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning =
        MakeModelId(Id.ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning);

    /// <summary>
    ///     The configured <see cref="IForeignKey" /> is redundant.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId RedundantForeignKeyWarning = MakeModelValidationId(Id.RedundantForeignKeyWarning);

    /// <summary>
    ///     A <see cref="KeylessAttribute" /> attribute on the entity type is conflicting
    ///     with a <see cref="KeyAttribute" /> attribute on at least one of its properties.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ConflictingKeylessAndKeyAttributesWarning =
        MakeModelId(Id.ConflictingKeylessAndKeyAttributesWarning);

    /// <summary>
    ///     Required navigation with principal entity having global query filter defined
    ///     and the declaring entity not having a matching filter
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ForeignKeyEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning
        = MakeModelValidationId(Id.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning);

    private static readonly string _changeTrackingPrefix = DbLoggerCategory.ChangeTracking.Name + ".";

    private static EventId MakeChangeTrackingId(Id id)
        => new((int)id, _changeTrackingPrefix + id);

    /// <summary>
    ///     DetectChanges is starting.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId DetectChangesStarting = MakeChangeTrackingId(Id.DetectChangesStarting);

    /// <summary>
    ///     DetectChanges has completed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId DetectChangesCompleted = MakeChangeTrackingId(Id.DetectChangesCompleted);

    /// <summary>
    ///     DetectChanges has detected a change in a property value.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyChangedEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId PropertyChangeDetected = MakeChangeTrackingId(Id.PropertyChangeDetected);

    /// <summary>
    ///     DetectChanges has detected a change in a foreign key property value.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyChangedEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ForeignKeyChangeDetected = MakeChangeTrackingId(Id.ForeignKeyChangeDetected);

    /// <summary>
    ///     DetectChanges has detected entities were added and/or removed from a collection
    ///     navigation property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CollectionChangedEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CollectionChangeDetected = MakeChangeTrackingId(Id.CollectionChangeDetected);

    /// <summary>
    ///     DetectChanges has detected entities were added and/or removed from a collection skip navigation property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="SkipCollectionChangedEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId SkipCollectionChangeDetected = MakeChangeTrackingId(Id.SkipCollectionChangeDetected);

    /// <summary>
    ///     DetectChanges has detected a change to the entity references by another entity.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="ReferenceChangedEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ReferenceChangeDetected = MakeChangeTrackingId(Id.ReferenceChangeDetected);

    /// <summary>
    ///     An entity is being tracked by the <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="EntityEntryEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId StartedTracking = MakeChangeTrackingId(Id.StartedTracking);

    /// <summary>
    ///     An entity tracked by the <see cref="DbContext" /> is changing from one
    ///     <see cref="EntityState" /> to another.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="StateChangedEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId StateChanged = MakeChangeTrackingId(Id.StateChanged);

    /// <summary>
    ///     A property of a tracked entity is getting a generated value.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.ChangeTracking" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyValueEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ValueGenerated = MakeChangeTrackingId(Id.ValueGenerated);

    /// <summary>
    ///     An entity is being deleted or detached because its parent was deleted.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CascadeDeleteEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CascadeDelete = MakeUpdateId(Id.CascadeDelete);

    /// <summary>
    ///     An entity is being deleted or detached because the required relationship to its
    ///     parent was severed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="CascadeDeleteOrphanEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CascadeDeleteOrphan = MakeUpdateId(Id.CascadeDeleteOrphan);

    /// <summary>
    ///     <see cref="DbContext.SaveChanges()" /> or one of its overloads started.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId SaveChangesStarting = MakeUpdateId(Id.SaveChangesStarting);

    /// <summary>
    ///     <see cref="DbContext.SaveChanges()" /> or one of its overloads has completed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="SaveChangesCompletedEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId SaveChangesCompleted = MakeUpdateId(Id.SaveChangesCompleted);

    /// <summary>
    ///     An <see cref="OptimisticConcurrencyException" /> was thrown during the call to
    ///     <see cref="DbContext.SaveChanges()" />
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Update" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextErrorEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId OptimisticConcurrencyException = MakeUpdateId(Id.OptimisticConcurrencyException);

    /// <summary>
    ///     The <see cref="DbContext" /> is being disposed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Infrastructure" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="DbContextEventData" /> payload when used with a
    ///         <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId ContextDisposed = MakeInfraId(Id.ContextDisposed);

    /// <summary>
    ///     A property has a collection or enumeration type with a value converter but with no value comparer.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This event is in the <see cref="DbLoggerCategory.Model.Validation" /> category.
    ///     </para>
    ///     <para>
    ///         This event uses the <see cref="PropertyEventData" /> payload when used with a <see cref="DiagnosticSource" />.
    ///     </para>
    /// </remarks>
    public static readonly EventId CollectionWithoutComparer = MakeModelValidationId(Id.CollectionWithoutComparer);
}
