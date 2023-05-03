// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Proxies.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Allows SQL Server specific configuration to be performed on <see cref="DbContextOptions" />.
/// </summary>
/// <remarks>
///     Instances of this class are returned from a call to <see cref="O:SqlServerDbContextOptionsExtensions.UseSqlServer" />
///     and it is not designed to be directly constructed in your application code.
/// </remarks>
public class LazyLoadingProxiesOptionsBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LazyLoadingProxiesOptionsBuilder" /> class.
    /// </summary>
    /// <param name="optionsBuilder">The core options builder.</param>
    public LazyLoadingProxiesOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        OptionsBuilder = optionsBuilder;
    }

    /// <summary>
    ///     Gets the core options builder.
    /// </summary>
    protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

    /// <summary>
    ///     Configures the proxies to ignore navigations that are not virtual. By default, an exception will be thrown if a non-virtual
    ///     navigation is found.
    /// </summary>
    /// <param name="ignoreNonVirtualNavigations">
    ///     <see langword="true" /> to ignore navigations that are not virtual. The default value is
    ///     <see langword="false" />, meaning an exception will be thrown if a non-virtual navigation is found.
    /// </param>
    public virtual LazyLoadingProxiesOptionsBuilder IgnoreNonVirtualNavigations(bool ignoreNonVirtualNavigations = true)
        => WithOption(e => e.WithIgnoreNonVirtualNavigations(ignoreNonVirtualNavigations));

    /// <summary>
    ///     Sets an option by cloning the extension used to store the settings. This ensures the builder
    ///     does not modify options that are already in use elsewhere.
    /// </summary>
    /// <param name="setAction">An action to set the option.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    protected virtual LazyLoadingProxiesOptionsBuilder WithOption(Func<ProxiesOptionsExtension, ProxiesOptionsExtension> setAction)
    {
        ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(
            setAction(OptionsBuilder.Options.FindExtension<ProxiesOptionsExtension>() ?? new ProxiesOptionsExtension()));

        return this;
    }
}
