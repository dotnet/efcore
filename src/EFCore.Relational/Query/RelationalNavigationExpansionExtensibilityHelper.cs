// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     Relational-specific service which helps with various aspects of navigation expansion extensibility.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
///     </para>
/// </remarks>
public class RelationalNavigationExpansionExtensibilityHelper : NavigationExpansionExtensibilityHelper, INavigationExpansionExtensibilityHelper
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalNavigationExpansionExtensibilityHelper" /> class.
    /// </summary>
    /// <param name="dependencies">The dependencies to use.</param>
    public RelationalNavigationExpansionExtensibilityHelper(NavigationExpansionExtensibilityHelperDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    bool INavigationExpansionExtensibilityHelper.SupportsNavigationExpansionJoins
        => true;
}
