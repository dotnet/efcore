// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
///     and it is not designed to be directly constructed in your application code.
/// </summary>
/// <typeparam name="TEntity">The entity type being configured.</typeparam>
public class TemporalTableBuilder<TEntity> : TemporalTableBuilder
    where TEntity : class
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public TemporalTableBuilder(EntityTypeBuilder entityTypeBuilder)
        : base(entityTypeBuilder)
    {
    }

    /// <summary>
    ///     Configures a history table for the entity mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the history table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual TemporalTableBuilder<TEntity> UseHistoryTable(string name)
        => (TemporalTableBuilder<TEntity>)base.UseHistoryTable(name);

    /// <summary>
    ///     Configures a history table for the entity mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the history table.</param>
    /// <param name="schema">The schema of the history table.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual TemporalTableBuilder<TEntity> UseHistoryTable(string name, string? schema)
        => (TemporalTableBuilder<TEntity>)base.UseHistoryTable(name, schema);

    /// <summary>
    ///     Returns an object that can be used to configure a period start property of the entity type mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured as the period start property
    ///     (<c>blog => blog.PeriodStart</c>).
    /// </param>
    /// <returns>An object that can be used to configure the period start property.</returns>
    public virtual TemporalPeriodPropertyBuilder HasPeriodStart(Expression<Func<TEntity, DateTime>> propertyExpression)
        => HasPeriodStart(Check.NotNull(propertyExpression).GetMemberAccess().Name);

    /// <summary>
    ///     Returns an object that can be used to configure a period end property of the entity type mapped to a temporal table.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="propertyExpression">
    ///     A lambda expression representing the property to be configured as the period end property
    ///     (<c>blog => blog.PeriodEnd</c>).
    /// </param>
    /// <returns>An object that can be used to configure the period end property.</returns>
    public virtual TemporalPeriodPropertyBuilder HasPeriodEnd(Expression<Func<TEntity, DateTime>> propertyExpression)
        => HasPeriodEnd(Check.NotNull(propertyExpression).GetMemberAccess().Name);

    /// <summary>
    ///     Configures whether the period columns of the temporal table are defined with the HIDDEN flag,
    ///     which excludes them from <c>SELECT *</c> results.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The default value is <see langword="true" />, matching the behavior of EF Core releases prior to this option
    ///         being introduced. Set to <see langword="false" /> to make the period columns visible in <c>SELECT *</c>.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-temporal">Using SQL Server temporal tables with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="hidden">A value indicating whether the period columns should be hidden.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public new virtual TemporalTableBuilder<TEntity> PeriodColumnsHidden(bool hidden = true)
        => (TemporalTableBuilder<TEntity>)base.PeriodColumnsHidden(hidden);
}
