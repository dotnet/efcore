// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     <para>
    ///         The base interface for all Entity Framework interceptors.
    ///     </para>
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
    /// </summary>
    public interface IInterceptor
    {
    }
}
