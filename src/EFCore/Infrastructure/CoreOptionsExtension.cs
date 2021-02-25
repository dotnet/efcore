// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Represents options managed by the core of Entity Framework, as opposed to those managed
    ///         by database providers or extensions. These options are set using <see cref="DbContextOptionsBuilder" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are designed to be immutable. To change an option, call one of the 'With...'
    ///         methods to obtain a new instance with the option changed.
    ///     </para>
    /// </summary>
    public class CoreOptionsExtension : IDbContextOptionsExtension
    {
        private IServiceProvider? _internalServiceProvider;
        private IServiceProvider? _applicationServiceProvider;
        private IModel? _model;
        private ILoggerFactory? _loggerFactory;
        private IDbContextLogger? _contextLogger;
        private IMemoryCache? _memoryCache;
        private bool _sensitiveDataLoggingEnabled;
        private bool _detailedErrorsEnabled;
        private bool _concurrencyDetectionEnabled = true;
        private QueryTrackingBehavior _queryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        private IDictionary<(Type, Type?), Type>? _replacedServices;
        private int? _maxPoolSize;
        private bool _serviceProviderCachingEnabled = true;
        private DbContextOptionsExtensionInfo? _info;
        private IEnumerable<IInterceptor>? _interceptors;

        private WarningsConfiguration _warningsConfiguration
            = new WarningsConfiguration()
                .TryWithExplicit(CoreEventId.ManyServiceProvidersCreatedWarning, WarningBehavior.Throw)
                .TryWithExplicit(CoreEventId.LazyLoadOnDisposedContextWarning, WarningBehavior.Throw)
                .TryWithExplicit(CoreEventId.DetachedLazyLoadingWarning, WarningBehavior.Throw)
                .TryWithExplicit(CoreEventId.InvalidIncludePathError, WarningBehavior.Throw);

        /// <summary>
        ///     Creates a new set of options with everything set to default values.
        /// </summary>
        public CoreOptionsExtension()
        {
        }

        /// <summary>
        ///     Called by a derived class constructor when implementing the <see cref="Clone" /> method.
        /// </summary>
        /// <param name="copyFrom"> The instance that is being cloned. </param>
        protected CoreOptionsExtension([NotNull] CoreOptionsExtension copyFrom)
        {
            _internalServiceProvider = copyFrom.InternalServiceProvider;
            _applicationServiceProvider = copyFrom.ApplicationServiceProvider;
            _model = copyFrom.Model;
            _loggerFactory = copyFrom.LoggerFactory;
            _contextLogger = copyFrom.DbContextLogger;
            _memoryCache = copyFrom.MemoryCache;
            _sensitiveDataLoggingEnabled = copyFrom.IsSensitiveDataLoggingEnabled;
            _detailedErrorsEnabled = copyFrom.DetailedErrorsEnabled;
            _concurrencyDetectionEnabled = copyFrom.ConcurrencyDetectionEnabled;
            _warningsConfiguration = copyFrom.WarningsConfiguration;
            _queryTrackingBehavior = copyFrom.QueryTrackingBehavior;
            _maxPoolSize = copyFrom.MaxPoolSize;
            _serviceProviderCachingEnabled = copyFrom.ServiceProviderCachingEnabled;
            _interceptors = copyFrom.Interceptors?.ToList();

            if (copyFrom._replacedServices != null)
            {
                _replacedServices = new Dictionary<(Type, Type?), Type>(copyFrom._replacedServices);
            }
        }

        /// <summary>
        ///     Information/metadata about the extension.
        /// </summary>
        public virtual DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        /// <summary>
        ///     Override this method in a derived class to ensure that any clone created is also of that class.
        /// </summary>
        /// <returns> A clone of this instance, which can be modified before being returned as immutable. </returns>
        protected virtual CoreOptionsExtension Clone()
            => new(this);

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="internalServiceProvider"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithInternalServiceProvider([CanBeNull] IServiceProvider? internalServiceProvider)
        {
            var clone = Clone();

            clone._internalServiceProvider = internalServiceProvider;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="applicationServiceProvider"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithApplicationServiceProvider([CanBeNull] IServiceProvider? applicationServiceProvider)
        {
            var clone = Clone();

            clone._applicationServiceProvider = applicationServiceProvider;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="model"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithModel([CanBeNull] IModel? model)
        {
            var clone = Clone();

            clone._model = model;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="memoryCache"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithMemoryCache([CanBeNull] IMemoryCache? memoryCache)
        {
            var clone = Clone();

            clone._memoryCache = memoryCache;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="loggerFactory"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithLoggerFactory([CanBeNull] ILoggerFactory? loggerFactory)
        {
            var clone = Clone();

            clone._loggerFactory = loggerFactory;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="contextLogger"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithDbContextLogger([CanBeNull] IDbContextLogger? contextLogger)
        {
            var clone = Clone();

            clone._contextLogger = contextLogger;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="sensitiveDataLoggingEnabled"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithSensitiveDataLoggingEnabled(bool sensitiveDataLoggingEnabled)
        {
            var clone = Clone();

            clone._sensitiveDataLoggingEnabled = sensitiveDataLoggingEnabled;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="detailedErrorsEnabled"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithDetailedErrorsEnabled(bool detailedErrorsEnabled)
        {
            var clone = Clone();

            clone._detailedErrorsEnabled = detailedErrorsEnabled;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="concurrencyDetectionEnabled"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithConcurrencyDetectionEnabled(bool concurrencyDetectionEnabled)
        {
            var clone = Clone();

            clone._concurrencyDetectionEnabled = concurrencyDetectionEnabled;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="queryTrackingBehavior"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithQueryTrackingBehavior(QueryTrackingBehavior queryTrackingBehavior)
        {
            var clone = Clone();

            clone._queryTrackingBehavior = queryTrackingBehavior;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="serviceType"> The service contract. </param>
        /// <param name="newImplementationType"> The implementation type to use for the service. </param>
        /// <param name="currentImplementationType"> The specific existing implementation type to replace. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithReplacedService(
            [NotNull] Type serviceType,
            [NotNull] Type newImplementationType,
            [CanBeNull] Type? currentImplementationType = null)
        {
            var clone = Clone();

            if (clone._replacedServices == null)
            {
                clone._replacedServices = new Dictionary<(Type, Type?), Type>();
            }

            clone._replacedServices[(serviceType, currentImplementationType)] = newImplementationType;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="maxPoolSize"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithMaxPoolSize(int? maxPoolSize)
        {
            var clone = Clone();

            clone._maxPoolSize = maxPoolSize;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="warningsConfiguration"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithWarningsConfiguration([NotNull] WarningsConfiguration warningsConfiguration)
        {
            var clone = Clone();

            clone._warningsConfiguration = warningsConfiguration;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="serviceProviderCachingEnabled"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithServiceProviderCachingEnabled(bool serviceProviderCachingEnabled)
        {
            var clone = Clone();

            clone._serviceProviderCachingEnabled = serviceProviderCachingEnabled;

            return clone;
        }

        /// <summary>
        ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
        ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
        /// </summary>
        /// <param name="interceptors"> The option to change. </param>
        /// <returns> A new instance with the option changed. </returns>
        public virtual CoreOptionsExtension WithInterceptors([NotNull] IEnumerable<IInterceptor> interceptors)
        {
            Check.NotNull(interceptors, nameof(interceptors));

            var clone = Clone();

            clone._interceptors = _interceptors == null
                ? interceptors
                : _interceptors.Concat(interceptors);

            return clone;
        }

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.EnableSensitiveDataLogging" /> method.
        /// </summary>
        public virtual bool IsSensitiveDataLoggingEnabled
            => _sensitiveDataLoggingEnabled;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.EnableDetailedErrors" /> method.
        /// </summary>
        public virtual bool DetailedErrorsEnabled
            => _detailedErrorsEnabled;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.DisableConcurrencyDetection" /> method.
        /// </summary>
        public virtual bool ConcurrencyDetectionEnabled
            => _concurrencyDetectionEnabled;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.UseModel" /> method.
        /// </summary>
        public virtual IModel? Model
            => _model;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.UseLoggerFactory" /> method.
        /// </summary>
        public virtual ILoggerFactory? LoggerFactory
            => _loggerFactory;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.LogTo(Action{string},LogLevel,DbContextLoggerOptions?)" /> method.
        /// </summary>
        public virtual IDbContextLogger? DbContextLogger
            => _contextLogger;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.UseMemoryCache" /> method.
        /// </summary>
        public virtual IMemoryCache? MemoryCache
            => _memoryCache;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        /// </summary>
        public virtual IServiceProvider? InternalServiceProvider
            => _internalServiceProvider;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.UseApplicationServiceProvider" /> method.
        /// </summary>
        public virtual IServiceProvider? ApplicationServiceProvider
            => _applicationServiceProvider;

        /// <summary>
        ///     The options set from the <see cref="DbContextOptionsBuilder.ConfigureWarnings" /> method.
        /// </summary>
        public virtual WarningsConfiguration WarningsConfiguration
            => _warningsConfiguration;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.UseQueryTrackingBehavior" /> method.
        /// </summary>
        public virtual QueryTrackingBehavior QueryTrackingBehavior
            => _queryTrackingBehavior;

        /// <summary>
        ///     The option set from the <see cref="DbContextOptionsBuilder.EnableServiceProviderCaching" /> method.
        /// </summary>
        public virtual bool ServiceProviderCachingEnabled
            => _serviceProviderCachingEnabled;

        /// <summary>
        ///     The options set from the <see cref="DbContextOptionsBuilder.ReplaceService{TService,TImplementation}" /> method.
        /// </summary>
        public virtual IReadOnlyDictionary<(Type, Type?), Type>? ReplacedServices
            => (IReadOnlyDictionary<(Type, Type?), Type>?)_replacedServices;

        /// <summary>
        ///     The option set from the
        ///     <see
        ///         cref="EntityFrameworkServiceCollectionExtensions.AddDbContextPool{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" />
        ///     method.
        /// </summary>
        public virtual int? MaxPoolSize
            => _maxPoolSize;

        /// <summary>
        ///     The options set from the <see cref="DbContextOptionsBuilder.AddInterceptors(IEnumerable{IInterceptor})" /> method.
        /// </summary>
        public virtual IEnumerable<IInterceptor>? Interceptors
            => _interceptors;

        /// <summary>
        ///     Adds the services required to make the selected options work. This is used when there
        ///     is no external <see cref="IServiceProvider" /> and EF is maintaining its own service
        ///     provider internally. This allows database providers (and other extensions) to register their
        ///     required services when EF is creating an service provider.
        /// </summary>
        /// <param name="services"> The collection to add services to. </param>
        public virtual void ApplyServices(IServiceCollection services)
        {
            var memoryCache = GetMemoryCache();
            if (memoryCache != null)
            {
                services.AddSingleton(memoryCache);
            }
        }

        private IMemoryCache? GetMemoryCache()
            => MemoryCache;

        /// <summary>
        ///     Gives the extension a chance to validate that all options in the extension are valid.
        ///     If options are invalid, then an exception will be thrown.
        /// </summary>
        /// <param name="options"> The options being validated. </param>
        public virtual void Validate(IDbContextOptions options)
        {
            if (_internalServiceProvider != null)
            {
                if (ReplacedServices != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.InvalidReplaceService(
                            nameof(DbContextOptionsBuilder.ReplaceService),
                            nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
                }

                if (LoggerFactory != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.InvalidUseService(
                            nameof(DbContextOptionsBuilder.UseLoggerFactory),
                            nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                            nameof(ILoggerFactory)));
                }

                if (MemoryCache != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.InvalidUseService(
                            nameof(DbContextOptionsBuilder.UseMemoryCache),
                            nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                            nameof(IMemoryCache)));
                }
            }
        }

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            private long? _serviceProviderHash;
            private string? _logFragment;

            public ExtensionInfo(CoreOptionsExtension extension)
                : base(extension)
            {
            }

            private new CoreOptionsExtension Extension
                => (CoreOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider
                => false;

            public override string LogFragment
            {
                get
                {
                    if (_logFragment == null)
                    {
                        var builder = new StringBuilder();

                        if (Extension._queryTrackingBehavior != QueryTrackingBehavior.TrackAll)
                        {
                            builder.Append(Extension._queryTrackingBehavior).Append(' ');
                        }

                        if (Extension._sensitiveDataLoggingEnabled)
                        {
                            builder.Append("SensitiveDataLoggingEnabled ");
                        }

                        if (Extension._detailedErrorsEnabled)
                        {
                            builder.Append("DetailedErrorsEnabled ");
                        }

                        if (!Extension._concurrencyDetectionEnabled)
                        {
                            builder.Append("ConcurrencyDetectionDisabled ");
                        }

                        if (Extension._maxPoolSize != null)
                        {
                            builder.Append("MaxPoolSize=").Append(Extension._maxPoolSize).Append(' ');
                        }

                        _logFragment = builder.ToString();
                    }

                    return _logFragment;
                }
            }

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                Check.NotNull(debugInfo, nameof(debugInfo));

                debugInfo["Core:" + nameof(DbContextOptionsBuilder.UseMemoryCache)] =
                    (Extension.GetMemoryCache()?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);
                debugInfo["Core:" + nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging)] =
                    Extension._sensitiveDataLoggingEnabled.GetHashCode().ToString(CultureInfo.InvariantCulture);
                debugInfo["Core:" + nameof(DbContextOptionsBuilder.EnableDetailedErrors)] =
                    Extension._detailedErrorsEnabled.GetHashCode().ToString(CultureInfo.InvariantCulture);
                debugInfo["Core:" + nameof(DbContextOptionsBuilder.DisableConcurrencyDetection)] =
                    (!Extension._concurrencyDetectionEnabled).GetHashCode().ToString(CultureInfo.InvariantCulture);
                debugInfo["Core:" + nameof(DbContextOptionsBuilder.ConfigureWarnings)] =
                    Extension._warningsConfiguration.GetServiceProviderHashCode().ToString(CultureInfo.InvariantCulture);

                if (Extension._replacedServices != null)
                {
                    foreach (var replacedService in Extension._replacedServices)
                    {
                        var (serviceType, implementationType) = replacedService.Key;

                        debugInfo["Core:"
                                + nameof(DbContextOptionsBuilder.ReplaceService)
                                + ":"
                                + serviceType.DisplayName()
                                + (implementationType == null ? "" : ", " + implementationType.DisplayName())]
                            = replacedService.Value.GetHashCode().ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            public override long GetServiceProviderHashCode()
            {
                if (_serviceProviderHash == null)
                {
                    var hashCode = Extension.GetMemoryCache()?.GetHashCode() ?? 0L;
                    hashCode = (hashCode * 3) ^ Extension._sensitiveDataLoggingEnabled.GetHashCode();
                    hashCode = (hashCode * 3) ^ Extension._detailedErrorsEnabled.GetHashCode();
                    hashCode = (hashCode * 3) ^ Extension._concurrencyDetectionEnabled.GetHashCode();
                    hashCode = (hashCode * 1073742113) ^ Extension._warningsConfiguration.GetServiceProviderHashCode();

                    if (Extension._replacedServices != null)
                    {
                        hashCode = Extension._replacedServices.Aggregate(hashCode, (t, e) => (t * 397) ^ e.Value.GetHashCode());
                    }

                    _serviceProviderHash = hashCode;
                }

                return _serviceProviderHash.Value;
            }
        }
    }
}
