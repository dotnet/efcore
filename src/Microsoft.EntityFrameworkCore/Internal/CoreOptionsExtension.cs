// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     Used to store the options specified via <see cref="DbContextOptionsBuilder" /> that are applicable to all databases.
    /// </summary>
    public class CoreOptionsExtension : IDbContextOptionsExtension, IWarningsAsErrorsOptionsExtension
    {
        private IServiceProvider _internalServiceProvider;
        private IModel _model;
        private ILoggerFactory _loggerFactory;
        private IMemoryCache _memoryCache;
        private bool _isSensitiveDataLoggingEnabled;
        private IReadOnlyCollection<CoreLoggingEventId> _warningsAsErrorsEventIds;

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="CoreOptionsExtension" /> class.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        public CoreOptionsExtension()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CoreOptionsExtension" /> class with the same options as an
        ///     existing instance.
        /// </summary>
        /// <param name="copyFrom"> The <see cref="CoreOptionsExtension" /> to copy options from. </param>
        public CoreOptionsExtension([NotNull] CoreOptionsExtension copyFrom)
        {
            _internalServiceProvider = copyFrom.InternalServiceProvider;
            _model = copyFrom.Model;
            _loggerFactory = copyFrom.LoggerFactory;
            _memoryCache = copyFrom.MemoryCache;
            _isSensitiveDataLoggingEnabled = copyFrom.IsSensitiveDataLoggingEnabled;
            _warningsAsErrorsEventIds = copyFrom.WarningsAsErrorsEventIds;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether application data can be included in exception messages, logging, etc.
        ///     This can include the values assigned to properties of your entity instances, parameter values for commands being
        ///     sent to the database, and other such data. You should only enable this flag if you have the appropriate security
        ///     measures in place based on the sensitivity of this data.
        /// </summary>
        public virtual bool IsSensitiveDataLoggingEnabled
        {
            get { return _isSensitiveDataLoggingEnabled; }
            set { _isSensitiveDataLoggingEnabled = value; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether a warning has been logged that <see cref="IsSensitiveDataLoggingEnabled" />
        ///     is enabled. This is used internally by EF to ensure the warning is only displayed once per context type.
        /// </summary>
        public virtual bool SensitiveDataLoggingWarned { get; set; }

        /// <summary>
        ///     Gets or sets the model to be used for the context. If the model is set, then <see cref="DbContext.OnModelCreating(ModelBuilder)" />
        ///     will not be run.
        /// </summary>
        public virtual IModel Model
        {
            get { return _model; }
            [param: CanBeNull] set { _model = value; }
        }

        public virtual ILoggerFactory LoggerFactory
        {
            get { return _loggerFactory; }
            [param: CanBeNull] set { _loggerFactory = value; }
        }

        public virtual IMemoryCache MemoryCache
        {
            get { return _memoryCache; }
            [param: CanBeNull] set { _memoryCache = value; }
        }

        public virtual IServiceProvider InternalServiceProvider
        {
            get { return _internalServiceProvider; }
            [param: CanBeNull] set { _internalServiceProvider = value; }
        }

        public virtual IReadOnlyCollection<CoreLoggingEventId> WarningsAsErrorsEventIds
        {
            get { return _warningsAsErrorsEventIds; }
            [param: CanBeNull] set { _warningsAsErrorsEventIds = value; }
        }

        /// <summary>
        ///     Adds the services required to make the selected options work. This is used when there is no external <see cref="IServiceProvider" />
        ///     and EF is maintaining its own service provider internally. Since all the core services are already added to the service provider,
        ///     this method does nothing.
        /// </summary>
        /// <param name="builder"> The builder to add services to. </param>
        public virtual void ApplyServices(IServiceCollection builder)
        {
        }

        public virtual bool WarningIsError(Enum warningEventId)
            => _warningsAsErrorsEventIds != null
               && warningEventId is CoreLoggingEventId
               && _warningsAsErrorsEventIds.Contains((CoreLoggingEventId)warningEventId);
    }
}
