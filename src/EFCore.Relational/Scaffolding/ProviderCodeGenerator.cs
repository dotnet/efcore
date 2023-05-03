// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     Generates provider-specific code fragments.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///         <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
///     </para>
/// </remarks>
public abstract class ProviderCodeGenerator : IProviderConfigurationCodeGenerator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProviderCodeGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">The dependencies.</param>
    protected ProviderCodeGenerator(ProviderCodeGeneratorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual ProviderCodeGeneratorDependencies Dependencies { get; }

    /// <summary>
    ///     Generates a method chain used to configure provider-specific options.
    /// </summary>
    /// <returns>The method chain. May be null.</returns>
    public virtual MethodCallCodeFragment? GenerateProviderOptions()
    {
        MethodCallCodeFragment? providerOptions = null;

        foreach (var plugin in Dependencies.Plugins)
        {
            var chainedCall = plugin.GenerateProviderOptions();
            if (chainedCall == null)
            {
                continue;
            }

            providerOptions = providerOptions?.Chain(chainedCall) ?? chainedCall;
        }

        return providerOptions;
    }

    /// <summary>
    ///     Generates a code fragment like <c>.UseSqlServer("Database=Foo")</c> which can be used in
    ///     the <see cref="DbContext.OnConfiguring" /> method of the generated DbContext.
    /// </summary>
    /// <param name="connectionString">The connection string to include in the code fragment.</param>
    /// <param name="providerOptions">The method chain used to configure provider options.</param>
    /// <returns>The code fragment.</returns>
    public abstract MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions);

    /// <summary>
    ///     Generates a method chain to configure additional context options.
    /// </summary>
    /// <returns>The method chain. May be null.</returns>
    public virtual MethodCallCodeFragment? GenerateContextOptions()
    {
        MethodCallCodeFragment? contextOptions = null;

        foreach (var plugin in Dependencies.Plugins)
        {
            var chainedCall = plugin.GenerateContextOptions();
            if (chainedCall == null)
            {
                continue;
            }

            contextOptions = contextOptions?.Chain(chainedCall) ?? chainedCall;
        }

        return contextOptions;
    }

    /// <inheritdoc />
    public virtual MethodCallCodeFragment GenerateUseProvider(string connectionString)
        => ((IProviderConfigurationCodeGenerator)this).GenerateUseProviderInternal(connectionString);
}
