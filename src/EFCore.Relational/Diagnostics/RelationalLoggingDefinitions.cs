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
    public class RelationalLoggingDefinitions : LoggingDefinitions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogClientEvalWarning;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerTransactionError;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogBoolWithDefaultWarning;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerOpeningConnection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerOpenedConnection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerClosingConnection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerClosedConnection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerConnectionError;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerBeginningTransaction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerUsingTransaction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerCommittingTransaction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerRollingbackTransaction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerDisposingTransaction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogDisposingDataReader;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogAmbientTransaction;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogPossibleUnintendedUseOfEquals;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogQueryPossibleExceptionWithAggregateOperator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogGeneratingDown;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogGeneratingUp;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogApplyingMigration;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRevertingMigration;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogMigrating;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogNoMigrationsApplied;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogNoMigrationsFound;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogKeyHasDefaultValue;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerExecutingCommand;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerExecutedCommand;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerCommandFailed;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogRelationalLoggerConnectionErrorAsDebug;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogAmbientTransactionEnlisted;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogExplicitTransactionEnlisted;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogBatchSmallerThanMinBatchSize;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogBatchReadyForExecution;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogMigrationAttributeMissingWarning;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        //[EntityFrameworkInternal]
        public EventDefinitionBase LogValueConversionSqlLiteralWarning;
    }
}
