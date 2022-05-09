// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
/// <typeparam name="TOwnerEntity">The entity type owning the relationship.</typeparam>
/// <typeparam name="TDependentEntity">The dependent entity type of the relationship.</typeparam>
public class OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> : OwnedNavigationTemporalTableBuilder
    where TOwnerEntity : class
    where TDependentEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationTemporalTableBuilder(OwnedNavigationBuilder referenceOwnershipBuilder)
        : base(referenceOwnershipBuilder)
    {
    }

    /// <summary>
    ///     Configures a history table for the entity mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="name">The name of the history table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> UseHistoryTable(string name)
        => (OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>)base.UseHistoryTable(name);

    /// <summary>
    ///     Configures a history table for the entity mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information.
    /// </remarks>
    /// <param name="name">The name of the history table.</param>
    /// <param name="schema">The schema of the history table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity> UseHistoryTable(string name, string? schema)
        => (OwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>)base.UseHistoryTable(name, schema);
}
