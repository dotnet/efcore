// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding;

/// <summary>
///     A service typically implemented by database providers to generate code fragments
///     for reverse engineering.
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
public interface IProviderConfigurationCodeGenerator
{
    /// <summary>
    ///     Generates a method chain used to configure provider-specific options.
    /// </summary>
    /// <returns>The method chain. May be null.</returns>
    MethodCallCodeFragment? GenerateProviderOptions();

    /// <summary>
    ///     Generates a code fragment like <c>.UseSqlServer("Database=Foo")</c> which can be used in
    ///     the <see cref="DbContext.OnConfiguring" /> method of the generated DbContext.
    /// </summary>
    /// <param name="connectionString">The connection string to include in the code fragment.</param>
    /// <param name="providerOptions">The method chain used to configure provider options.</param>
    /// <returns>The code fragment.</returns>
    MethodCallCodeFragment GenerateUseProvider(
        string connectionString,
        MethodCallCodeFragment? providerOptions);

    /// <summary>
    ///     Generates a method chain to configure additional context options.
    /// </summary>
    /// <returns>The method chain. May be null.</returns>
    MethodCallCodeFragment? GenerateContextOptions();

    /// <summary>
    ///     Generates a code fragment like <c>.UseSqlServer("Database=Foo")</c> which can be used in
    ///     the <see cref="DbContext.OnConfiguring" /> method of the generated DbContext.
    /// </summary>
    /// <param name="connectionString">The connection string to include in the code fragment.</param>
    /// <returns>The code fragment.</returns>
    MethodCallCodeFragment GenerateUseProvider(string connectionString)
        => GenerateUseProviderInternal(connectionString);

    // Issue #28537.
    internal sealed MethodCallCodeFragment GenerateUseProviderInternal(string connectionString)
    {
        var useProviderCall = GenerateUseProvider(
            connectionString,
            GenerateProviderOptions());
        var contextOptions = GenerateContextOptions();
        if (contextOptions != null)
        {
            useProviderCall = useProviderCall.Chain(contextOptions);
        }

        return useProviderCall;
    }
}
