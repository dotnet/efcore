// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Specifies the configuration type for the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
/// <typeparam name="TConfiguration">The IEntityTypeConfiguration&lt;&gt; type to use.</typeparam>
/// <typeparam name="TEntity">The entity type to be configured.</typeparam>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EntityTypeConfigurationAttribute<TConfiguration, TEntity> : EntityTypeConfigurationAttribute
    where TConfiguration : class, IEntityTypeConfiguration<TEntity>
    where TEntity : class
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EntityTypeConfigurationAttribute" /> class.
    /// </summary>
    public EntityTypeConfigurationAttribute()
        : base(typeof(TConfiguration))
    {
    }
}
