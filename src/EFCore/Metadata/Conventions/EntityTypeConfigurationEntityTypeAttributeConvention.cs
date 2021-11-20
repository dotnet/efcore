// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that applies the entity type configuration specified in <see cref="EntityTypeConfigurationAttribute" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class EntityTypeConfigurationEntityTypeAttributeConvention : EntityTypeAttributeConventionBase<EntityTypeConfigurationAttribute>
{
    private static readonly MethodInfo _configureMethod = typeof(EntityTypeConfigurationEntityTypeAttributeConvention)
        .GetRequiredDeclaredMethod(nameof(Configure));

    /// <summary>
    ///     Creates a new instance of <see cref="EntityTypeConfigurationEntityTypeAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public EntityTypeConfigurationEntityTypeAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Called after an entity type is added to the model if it has an attribute.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        EntityTypeConfigurationAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        var entityTypeConfigurationType = attribute.EntityTypeConfigurationType;

        if (!entityTypeConfigurationType.GetInterfaces().Any(
                x =>
                    x.IsGenericType
                    && x.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                    && x.GenericTypeArguments[0] == entityTypeBuilder.Metadata.ClrType))
        {
            throw new InvalidOperationException(
                CoreStrings.InvalidEntityTypeConfigurationAttribute(
                    entityTypeConfigurationType.ShortDisplayName(), entityTypeBuilder.Metadata.ShortName()));
        }

        _configureMethod.MakeGenericMethod(entityTypeBuilder.Metadata.ClrType)
            .Invoke(null, new object[] { entityTypeBuilder.Metadata, entityTypeConfigurationType });
    }

    private static void Configure<TEntity>(IConventionEntityType entityType, Type entityTypeConfigurationType)
        where TEntity : class
    {
        var entityTypeBuilder = new EntityTypeBuilder<TEntity>((IMutableEntityType)entityType);
        var entityTypeConfiguration = (IEntityTypeConfiguration<TEntity>)Activator.CreateInstance(entityTypeConfigurationType)!;
        entityTypeConfiguration.Configure(entityTypeBuilder);
    }
}
