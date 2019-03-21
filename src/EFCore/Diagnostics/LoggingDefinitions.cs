// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         Contains placeholders for caching of <see cref="EventDefinitionBase" />.
    ///     </para>
    ///     <para>
    ///         This class is public so that it can be inherited by database providers
    ///         to add caching for their events. It should not be used for any other purpose.
    ///     </para>
    /// </summary>
    public class LoggingDefinitions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogServiceProviderCreated;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogManyServiceProvidersCreated;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogServiceProviderDebugInfo;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogContextInitialized;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogExceptionDuringQueryIteration;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogExceptionDuringSaveChanges;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogDetectChangesStarting;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogDetectChangesCompleted;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogPropertyChangeDetected;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogPropertyChangeDetectedSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogForeignKeyChangeDetected;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogForeignKeyChangeDetectedSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogCollectionChangeDetected;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogCollectionChangeDetectedSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogReferenceChangeDetected;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogReferenceChangeDetectedSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogCascadeDelete;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogCascadeDeleteSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogCascadeDeleteOrphan;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogCascadeDeleteOrphanSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogStartedTracking;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogStartedTrackingSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogStateChanged;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogStateChangedSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogValueGenerated;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogValueGeneratedSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogTempValueGenerated;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogTempValueGeneratedSensitive;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogSaveChangesStarting;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogSaveChangesCompleted;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogContextDisposed;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogCompilingQueryModel;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogOptimizedQueryModel;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogIncludingNavigation;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogQueryExecutionPlanned;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogSensitiveDataLoggingEnabled;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogIgnoredInclude;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRowLimitingOperationWithoutOrderBy;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogPossibleUnintendedCollectionNavigationNullComparison;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogPossibleUnintendedReferenceComparison;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogDuplicateDependentEntityTypeInstance;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogShadowPropertyCreated;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogExecutionStrategyRetrying;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogNavigationLazyLoading;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogLazyLoadOnDisposedContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogDetachedLazyLoading;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRedundantIndexRemoved;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogIncompatibleMatchingForeignKeyProperties;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRequiredAttributeOnDependent;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRequiredAttributeOnBothNavigations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogForeignKeyAttributesOnBothNavigations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogForeignKeyAttributesOnBothProperties;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogConflictingForeignKeyAttributesOnNavigationAndProperty;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogMultipleInversePropertiesSameTarget;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogConflictingShadowForeignKeys;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogMultipleNavigationProperties;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogMultiplePrimaryKeyCandidates;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogNonOwnershipInverseNavigation;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogFirstWithoutOrderByAndFilter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogNonDefiningInverseNavigation;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogOptimisticConcurrencyException;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRedundantForeignKey;
    }
}
