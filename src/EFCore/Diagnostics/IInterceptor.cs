// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     The base interface for all Entity Framework interceptors.
/// </summary>
/// <remarks>
///     <para>
///         Interceptors can be used to view, change, or suppress operations taken by Entity Framework.
///         See the specific implementations of this interface for details. For example, 'IDbCommandInterceptor'.
///     </para>
///     <para>
///         Use <see cref="DbContextOptionsBuilder.AddInterceptors(Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[])" />
///         to register application interceptors.
///     </para>
///     <para>
///         Extensions can also register multiple <see cref="IInterceptor" />s in the internal service provider.
///         If both injected and application interceptors are found, then the injected interceptors are run in the
///         order that they are resolved from the service provider, and then the application interceptors are run
///         in the order that they were added to the context.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
///     </para>
/// </remarks>
public interface IInterceptor;
