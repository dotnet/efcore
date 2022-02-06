// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     <para>
///         A service that resolves a single <see cref="IInterceptor" /> from all those registered on
///         the <see cref="DbContext" /> or in the internal service provider.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
///     </para>
/// </remarks>
public interface IInterceptors
{
    /// <summary>
    ///     Resolves a single <typeparamref name="TInterceptor" /> from all those registered on
    ///     the <see cref="DbContext" /> or in the internal service provider.
    /// </summary>
    /// <typeparam name="TInterceptor">The interceptor type to resolve.</typeparam>
    /// <returns>The resolved interceptor, which may be <see langword="null" /> if none are registered.</returns>
    TInterceptor? Aggregate<TInterceptor>()
        where TInterceptor : class, IInterceptor;
}
