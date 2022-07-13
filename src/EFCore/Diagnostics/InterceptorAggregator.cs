// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     Abstract base class for implementations of the <see cref="IInterceptorAggregator" /> service.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" /> and multiple registrations
///         are allowed. This means that each <see cref="DbContext" /> instance will use its own
///         set of instances of this service.
///         The implementations may depend on other services registered with any lifetime.
///         The implementations do not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="TInterceptor">The interceptor type.</typeparam>
public abstract class InterceptorAggregator<TInterceptor> : IInterceptorAggregator
    where TInterceptor : class, IInterceptor
{
    private TInterceptor? _interceptor;
    private bool _resolved;

    /// <summary>
    ///     The interceptor type.
    /// </summary>
    public virtual Type InterceptorType
        => typeof(TInterceptor);

    /// <summary>
    ///     Resolves a single <see cref="IInterceptor" /> /> from all those registered on
    ///     the <see cref="DbContext" /> or in the internal service provider.
    /// </summary>
    /// <param name="interceptors">The interceptors to combine.</param>
    /// <returns>The combined interceptor.</returns>
    public virtual IInterceptor? AggregateInterceptors(IReadOnlyList<IInterceptor> interceptors)
    {
        if (!_resolved)
        {
            if (interceptors.Count == 1)
            {
                _interceptor = interceptors[0] as TInterceptor;
            }
            else if (interceptors.Count > 1)
            {
                var filtered = interceptors.OfType<TInterceptor>().ToList();

                if (filtered.Count == 1)
                {
                    _interceptor = filtered[0];
                }
                else if (filtered.Count > 1)
                {
                    _interceptor = CreateChain(filtered);
                }
            }

            _resolved = true;
        }

        return _interceptor;
    }

    /// <summary>
    ///     Must be implemented by the inheriting type to create a single interceptor from the given list.
    /// </summary>
    /// <param name="interceptors">The interceptors to combine.</param>
    /// <returns>The combined interceptor.</returns>
    protected abstract TInterceptor CreateChain(IEnumerable<TInterceptor> interceptors);
}
