// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CoreOptionsExtension : IDbContextOptionsExtension
    {
        private IServiceProvider _internalServiceProvider;
        private IModel _model;
        private ILoggerFactory _loggerFactory;
        private IMemoryCache _memoryCache;
        private bool _isSensitiveDataLoggingEnabled;
        private WarningsConfiguration _warningsConfiguration;
        private QueryTrackingBehavior _queryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        private IDictionary<Type, Type> _replacedServices;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CoreOptionsExtension()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CoreOptionsExtension([NotNull] CoreOptionsExtension copyFrom)
        {
            _internalServiceProvider = copyFrom.InternalServiceProvider;
            _model = copyFrom.Model;
            _loggerFactory = copyFrom.LoggerFactory;
            _memoryCache = copyFrom.MemoryCache;
            _isSensitiveDataLoggingEnabled = copyFrom.IsSensitiveDataLoggingEnabled;
            _warningsConfiguration = copyFrom.WarningsConfiguration;
            _queryTrackingBehavior = copyFrom.QueryTrackingBehavior;

            if (copyFrom._replacedServices != null)
            {
                _replacedServices = new Dictionary<Type, Type>(copyFrom._replacedServices);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsSensitiveDataLoggingEnabled
        {
            get { return _isSensitiveDataLoggingEnabled; }
            set { _isSensitiveDataLoggingEnabled = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool SensitiveDataLoggingWarned { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IModel Model
        {
            get { return _model; }
            [param: CanBeNull] set { _model = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ILoggerFactory LoggerFactory
        {
            get { return _loggerFactory; }
            [param: CanBeNull] set { _loggerFactory = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IMemoryCache MemoryCache
        {
            get { return _memoryCache; }
            [param: CanBeNull] set { _memoryCache = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceProvider InternalServiceProvider
        {
            get { return _internalServiceProvider; }
            [param: CanBeNull] set { _internalServiceProvider = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual WarningsConfiguration WarningsConfiguration
        {
            get { return _warningsConfiguration; }
            [param: CanBeNull] set { _warningsConfiguration = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual QueryTrackingBehavior QueryTrackingBehavior
        {
            get { return _queryTrackingBehavior; }
            set { _queryTrackingBehavior = value; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void ReplaceService([NotNull] Type serviceType, [NotNull] Type implementationType)
            => (_replacedServices ?? (_replacedServices = new Dictionary<Type, Type>()))[serviceType] = implementationType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyDictionary<Type, Type> ReplacedServices => (IReadOnlyDictionary<Type, Type>)_replacedServices;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void ApplyServices(IServiceCollection builder)
        {
        }
    }
}
