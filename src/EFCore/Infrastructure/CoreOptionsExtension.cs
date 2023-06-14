// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

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
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class CoreOptionsExtension : IDbContextOptionsExtension
{
    private IServiceProvider? _internalServiceProvider;
    private IServiceProvider? _applicationServiceProvider;
    private IServiceProvider? _rootApplicationServiceProvider;
    private bool _autoResolveResolveRootProvider;
    private IModel? _model;
    private ILoggerFactory? _loggerFactory;
    private IDbContextLogger? _contextLogger;
    private IMemoryCache? _memoryCache;
    private bool _sensitiveDataLoggingEnabled;
    private bool _detailedErrorsEnabled;
    private bool _threadSafetyChecksEnabled = true;
    private QueryTrackingBehavior _queryTrackingBehavior = QueryTrackingBehavior.TrackAll;
    private Dictionary<(Type, Type?), Type>? _replacedServices;
    private int? _maxPoolSize;
    private TimeSpan _loggingCacheTime = DefaultLoggingCacheTime;
    private bool _serviceProviderCachingEnabled = true;
    private DbContextOptionsExtensionInfo? _info;
    private IEnumerable<IInterceptor>? _interceptors;
    private IEnumerable<ISingletonInterceptor>? _singletonInterceptors;

    private static readonly TimeSpan DefaultLoggingCacheTime = TimeSpan.FromSeconds(1);

    private WarningsConfiguration _warningsConfiguration
        = new WarningsConfiguration()
            .TryWithExplicit(CoreEventId.ManyServiceProvidersCreatedWarning, WarningBehavior.Throw)
            .TryWithExplicit(CoreEventId.LazyLoadOnDisposedContextWarning, WarningBehavior.Throw)
            .TryWithExplicit(CoreEventId.DetachedLazyLoadingWarning, WarningBehavior.Throw)
            .TryWithExplicit(CoreEventId.InvalidIncludePathError, WarningBehavior.Throw)
            .TryWithExplicit(CoreEventId.NavigationBaseIncludeIgnored, WarningBehavior.Throw);

    /// <summary>
    ///     Creates a new set of options with everything set to default values.
    /// </summary>
    public CoreOptionsExtension()
    {
    }

    /// <summary>
    ///     Called by a derived class constructor when implementing the <see cref="Clone" /> method.
    /// </summary>
    /// <param name="copyFrom">The instance that is being cloned.</param>
    protected CoreOptionsExtension(CoreOptionsExtension copyFrom)
    {
        _internalServiceProvider = copyFrom.InternalServiceProvider;
        _applicationServiceProvider = copyFrom.ApplicationServiceProvider;
        _rootApplicationServiceProvider = copyFrom.RootApplicationServiceProvider;
        _autoResolveResolveRootProvider = copyFrom.AutoResolveRootProvider;
        _model = copyFrom.Model;
        _loggerFactory = copyFrom.LoggerFactory;
        _contextLogger = copyFrom.DbContextLogger;
        _memoryCache = copyFrom.MemoryCache;
        _sensitiveDataLoggingEnabled = copyFrom.IsSensitiveDataLoggingEnabled;
        _detailedErrorsEnabled = copyFrom.DetailedErrorsEnabled;
        _threadSafetyChecksEnabled = copyFrom.ThreadSafetyChecksEnabled;
        _warningsConfiguration = copyFrom.WarningsConfiguration;
        _queryTrackingBehavior = copyFrom.QueryTrackingBehavior;
        _maxPoolSize = copyFrom.MaxPoolSize;
        _loggingCacheTime = copyFrom.LoggingCacheTime;
        _serviceProviderCachingEnabled = copyFrom.ServiceProviderCachingEnabled;
        _interceptors = copyFrom.Interceptors?.ToList();
        _singletonInterceptors = copyFrom.SingletonInterceptors?.ToList();

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
    /// <returns>A clone of this instance, which can be modified before being returned as immutable.</returns>
    protected virtual CoreOptionsExtension Clone()
        => new(this);

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="internalServiceProvider">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithInternalServiceProvider(IServiceProvider? internalServiceProvider)
    {
        var clone = Clone();

        clone._internalServiceProvider = internalServiceProvider;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="applicationServiceProvider">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithApplicationServiceProvider(IServiceProvider? applicationServiceProvider)
    {
        var clone = Clone();

        clone._applicationServiceProvider = applicationServiceProvider;
        clone._rootApplicationServiceProvider ??= _autoResolveResolveRootProvider
            ? applicationServiceProvider?.GetService<ServiceProviderAccessor>()?.RootServiceProvider
            : null;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="rootApplicationServiceProvider">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithRootApplicationServiceProvider(IServiceProvider? rootApplicationServiceProvider)
    {
        var clone = Clone();

        clone._rootApplicationServiceProvider = rootApplicationServiceProvider;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="autoResolve">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithRootApplicationServiceProvider(bool autoResolve = true)
    {
        var clone = Clone();

        clone._autoResolveResolveRootProvider = autoResolve;
        clone._rootApplicationServiceProvider ??= autoResolve
            ? _applicationServiceProvider?.GetService<ServiceProviderAccessor>()?.RootServiceProvider
            : null;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="model">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithModel(IModel? model)
    {
        var clone = Clone();

        clone._model = model;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="memoryCache">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithMemoryCache(IMemoryCache? memoryCache)
    {
        var clone = Clone();

        clone._memoryCache = memoryCache;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="loggerFactory">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithLoggerFactory(ILoggerFactory? loggerFactory)
    {
        var clone = Clone();

        clone._loggerFactory = loggerFactory;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="contextLogger">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithDbContextLogger(IDbContextLogger? contextLogger)
    {
        var clone = Clone();

        clone._contextLogger = contextLogger;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="sensitiveDataLoggingEnabled">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
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
    /// <param name="detailedErrorsEnabled">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
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
    /// <param name="checksEnabled">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithThreadSafetyChecksEnabled(bool checksEnabled)
    {
        var clone = Clone();

        clone._threadSafetyChecksEnabled = checksEnabled;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="queryTrackingBehavior">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
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
    /// <param name="serviceType">The service contract.</param>
    /// <param name="newImplementationType">The implementation type to use for the service.</param>
    /// <param name="currentImplementationType">The specific existing implementation type to replace.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithReplacedService(
        Type serviceType,
        Type newImplementationType,
        Type? currentImplementationType = null)
    {
        var clone = Clone();

        clone._replacedServices ??= new Dictionary<(Type, Type?), Type>();

        clone._replacedServices[(serviceType, currentImplementationType)] = newImplementationType;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="maxPoolSize">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
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
    /// <param name="timeSpan">The maximum time period over which to skip logging checks before checking again.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithLoggingCacheTime(TimeSpan timeSpan)
    {
        var clone = Clone();

        clone._loggingCacheTime = timeSpan;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="warningsConfiguration">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithWarningsConfiguration(WarningsConfiguration warningsConfiguration)
    {
        var clone = Clone();

        clone._warningsConfiguration = warningsConfiguration;

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="serviceProviderCachingEnabled">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
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
    /// <param name="interceptors">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithInterceptors(IEnumerable<IInterceptor> interceptors)
    {
        var clone = Clone();

        clone._interceptors = _interceptors == null
            ? interceptors
            : _interceptors.Concat(interceptors);

        return clone;
    }

    /// <summary>
    ///     Creates a new instance with all options the same as for this instance, but with the given option changed.
    ///     It is unusual to call this method directly. Instead use <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    /// <param name="interceptors">The option to change.</param>
    /// <returns>A new instance with the option changed.</returns>
    public virtual CoreOptionsExtension WithSingletonInterceptors(IEnumerable<ISingletonInterceptor> interceptors)
    {
        var clone = Clone();

        clone._singletonInterceptors = _singletonInterceptors == null
            ? interceptors
            : _singletonInterceptors.Concat(interceptors);

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
    ///     The option set from the <see cref="DbContextOptionsBuilder.EnableThreadSafetyChecks" /> method.
    /// </summary>
    public virtual bool ThreadSafetyChecksEnabled
        => _threadSafetyChecksEnabled;

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
    ///     The option set from the <see cref="DbContextOptionsBuilder.UseRootApplicationServiceProvider(IServiceProvider?)" /> method.
    /// </summary>
    public virtual IServiceProvider? RootApplicationServiceProvider
        => _rootApplicationServiceProvider;

    /// <summary>
    ///     The option set from the <see cref="DbContextOptionsBuilder.UseRootApplicationServiceProvider(IServiceProvider?)" /> method.
    /// </summary>
    public virtual bool AutoResolveRootProvider
        => _autoResolveResolveRootProvider;

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
        => _replacedServices;

    /// <summary>
    ///     The option set from the
    ///     <see
    ///         cref="EntityFrameworkServiceCollectionExtensions.AddDbContextPool{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" />
    ///     method.
    /// </summary>
    public virtual int? MaxPoolSize
        => _maxPoolSize;

    /// <summary>
    ///     The option set from the
    ///     <see
    ///         cref="EntityFrameworkServiceCollectionExtensions.AddDbContextPool{TContext}(IServiceCollection,Action{DbContextOptionsBuilder},int)" />
    ///     method.
    /// </summary>
    public virtual TimeSpan LoggingCacheTime
        => _loggingCacheTime;

    /// <summary>
    ///     The options set from the <see cref="DbContextOptionsBuilder.AddInterceptors(IEnumerable{IInterceptor})" /> method
    ///     for scoped interceptors.
    /// </summary>
    public virtual IEnumerable<IInterceptor>? Interceptors
        => _interceptors;

    /// <summary>
    ///     The options set from the <see cref="DbContextOptionsBuilder.AddInterceptors(IEnumerable{IInterceptor})" /> method
    ///     for singleton interceptors.
    /// </summary>
    public virtual IEnumerable<ISingletonInterceptor>? SingletonInterceptors
        => _singletonInterceptors;

    /// <summary>
    ///     Adds the services required to make the selected options work. This is used when there
    ///     is no external <see cref="IServiceProvider" /> and EF is maintaining its own service
    ///     provider internally. This allows database providers (and other extensions) to register their
    ///     required services when EF is creating an service provider.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    public virtual void ApplyServices(IServiceCollection services)
    {
        var memoryCache = GetMemoryCache();
        if (memoryCache != null)
        {
            services.AddSingleton(memoryCache);
        }

        if (_singletonInterceptors != null)
        {
            foreach (var interceptor in _singletonInterceptors)
            {
                services.AddSingleton(interceptor);
            }
        }
    }

    private IMemoryCache? GetMemoryCache()
        => MemoryCache;

    /// <summary>
    ///     Gives the extension a chance to validate that all options in the extension are valid.
    ///     If options are invalid, then an exception will be thrown.
    /// </summary>
    /// <param name="options">The options being validated.</param>
    public virtual void Validate(IDbContextOptions options)
    {
        if (MaxPoolSize is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxPoolSize), CoreStrings.InvalidPoolSize);
        }

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

            if (SingletonInterceptors != null && SingletonInterceptors.Any())
            {
                throw new InvalidOperationException(
                    CoreStrings.InvalidUseService(
                        nameof(DbContextOptionsBuilder.AddInterceptors),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                        nameof(ISingletonInterceptor)));
            }
        }
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        private int? _serviceProviderHash;
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

                    if (!Extension._threadSafetyChecksEnabled)
                    {
                        builder.Append("ThreadSafetyChecksEnabled ");
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
            debugInfo["Core:" + nameof(DbContextOptionsBuilder.UseMemoryCache)] =
                (Extension.GetMemoryCache()?.GetHashCode() ?? 0L).ToString(CultureInfo.InvariantCulture);
            debugInfo["Core:" + nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging)] =
                Extension._sensitiveDataLoggingEnabled.GetHashCode().ToString(CultureInfo.InvariantCulture);
            debugInfo["Core:" + nameof(DbContextOptionsBuilder.EnableDetailedErrors)] =
                Extension._detailedErrorsEnabled.GetHashCode().ToString(CultureInfo.InvariantCulture);
            debugInfo["Core:" + nameof(DbContextOptionsBuilder.EnableThreadSafetyChecks)] =
                (!Extension._threadSafetyChecksEnabled).GetHashCode().ToString(CultureInfo.InvariantCulture);
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

        public override int GetServiceProviderHashCode()
        {
            if (_serviceProviderHash == null)
            {
                var hashCode = new HashCode();
                hashCode.Add(Extension.GetMemoryCache());
                hashCode.Add(Extension._sensitiveDataLoggingEnabled);
                hashCode.Add(Extension._detailedErrorsEnabled);
                hashCode.Add(Extension.RootApplicationServiceProvider);
                hashCode.Add(Extension._threadSafetyChecksEnabled);
                hashCode.Add(Extension._warningsConfiguration.GetServiceProviderHashCode());

                if (Extension._replacedServices != null)
                {
                    foreach (var replacedService in Extension._replacedServices)
                    {
                        hashCode.Add(replacedService.Value);
                    }
                }

                if (Extension._singletonInterceptors != null)
                {
                    foreach (var interceptor in Extension._singletonInterceptors)
                    {
                        hashCode.Add(interceptor);
                    }
                }

                _serviceProviderHash = hashCode.ToHashCode();
            }

            return _serviceProviderHash.Value;
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            => other is ExtensionInfo otherInfo
                && Extension.GetMemoryCache() == otherInfo.Extension.GetMemoryCache()
                && Extension._sensitiveDataLoggingEnabled == otherInfo.Extension._sensitiveDataLoggingEnabled
                && Extension._detailedErrorsEnabled == otherInfo.Extension._detailedErrorsEnabled
                && Extension.RootApplicationServiceProvider == otherInfo.Extension.RootApplicationServiceProvider
                && Extension._threadSafetyChecksEnabled == otherInfo.Extension._threadSafetyChecksEnabled
                && Extension._warningsConfiguration.ShouldUseSameServiceProvider(otherInfo.Extension._warningsConfiguration)
                && (Extension._replacedServices == otherInfo.Extension._replacedServices
                    || (Extension._replacedServices != null
                        && otherInfo.Extension._replacedServices != null
                        && Extension._replacedServices.Count == otherInfo.Extension._replacedServices.Count
                        && Extension._replacedServices.SequenceEqual(otherInfo.Extension._replacedServices)))
                && (Extension._singletonInterceptors == otherInfo.Extension._singletonInterceptors
                    || (Extension._singletonInterceptors != null
                        && otherInfo.Extension._singletonInterceptors != null
                        && Extension._singletonInterceptors.SequenceEqual(otherInfo.Extension._singletonInterceptors)));
    }
}
