// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class Interceptors : IInterceptors
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IInterceptor> _injectedInterceptors;
    private readonly Dictionary<Type, IInterceptorAggregator> _aggregators;
    private CoreOptionsExtension? _coreOptionsExtension;
    private List<IInterceptor>? _interceptors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Interceptors(
        IServiceProvider serviceProvider,
        IEnumerable<IInterceptor> injectedInterceptors,
        IEnumerable<IInterceptorAggregator> interceptorAggregators)
    {
        _serviceProvider = serviceProvider;
        _injectedInterceptors = injectedInterceptors;
        _aggregators = interceptorAggregators.ToDictionary(i => i.InterceptorType);
    }

    private IReadOnlyList<IInterceptor> RegisteredInterceptors
    {
        get
        {
            if (_interceptors == null)
            {
                var interceptors = _injectedInterceptors.ToList();
                var appInterceptors = CoreOptionsExtension?.Interceptors;
                if (appInterceptors != null)
                {
                    interceptors.AddRange(appInterceptors);
                }

                _interceptors = interceptors;
            }

            return _interceptors;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TInterceptor? Aggregate<TInterceptor>()
        where TInterceptor : class, IInterceptor
        => (TInterceptor?)_aggregators[typeof(TInterceptor)].AggregateInterceptors(RegisteredInterceptors);

    /// <summary>
    ///     We resolve this lazily because loggers are created very early in the initialization
    ///     process where <see cref="IDbContextOptions" /> is not yet available from D.I.
    ///     This means those loggers can't do interception, but that's okay because nothing
    ///     else is ready for them to do interception anyway.
    /// </summary>
    private CoreOptionsExtension? CoreOptionsExtension
        => _coreOptionsExtension ??= _serviceProvider
            .GetRequiredService<IDbContextOptions>()
            .Extensions
            .OfType<CoreOptionsExtension>()
            .FirstOrDefault();
}
