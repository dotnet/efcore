// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     This type is added as a singleton service to the application service provider to provide access to the
///     root service provider.
/// </summary>
public class ServiceProviderAccessor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ServiceProviderAccessor" /> class.
    /// </summary>
    /// <param name="rootServiceProvider">The injected service provider.</param>
    public ServiceProviderAccessor(IServiceProvider rootServiceProvider)
    {
        RootServiceProvider = rootServiceProvider;
    }

    /// <summary>
    ///     The injected service provider.
    /// </summary>
    public virtual IServiceProvider RootServiceProvider { get; }
}
