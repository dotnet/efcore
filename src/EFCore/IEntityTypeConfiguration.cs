// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Allows configuration for an entity type to be factored into a separate class,
///     rather than in-line in <see cref="DbContext.OnModelCreating(ModelBuilder)" />.
///     Implement this interface, applying configuration for the entity in the
///     <see cref="Configure(EntityTypeBuilder{TEntity})" /> method,
///     and then apply the configuration to the model using
///     <see cref="ModelBuilder.ApplyConfiguration{TEntity}(IEntityTypeConfiguration{TEntity})" />
///     in <see cref="DbContext.OnModelCreating(ModelBuilder)" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships in EF Core</see> for more information and
///     examples.
/// </remarks>
/// <typeparam name="TEntity">The entity type to be configured.</typeparam>
public interface IEntityTypeConfiguration<[DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Configures the entity of type <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    void Configure(EntityTypeBuilder<TEntity> builder);
}
