// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A base type for conventions that perform configuration based on an attribute specified on an entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
/// <typeparam name="TAttribute">The attribute type to look for.</typeparam>
public abstract class EntityTypeAttributeConventionBase<TAttribute> : IEntityTypeAddedConvention
    where TAttribute : Attribute
{
    /// <summary>
    ///     Creates a new instance of <see cref="EntityTypeAttributeConventionBase{TAttribute}" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    protected EntityTypeAttributeConventionBase(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Called after an entity type is added to the model.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

        var type = entityTypeBuilder.Metadata.ClrType;
        if (!Attribute.IsDefined(type, typeof(TAttribute), inherit: true))
        {
            return;
        }

        var attributes = type.GetCustomAttributes<TAttribute>(true);

        foreach (var attribute in attributes)
        {
            ProcessEntityTypeAdded(entityTypeBuilder, attribute, context);
            if (((IReadableConventionContext)context).ShouldStopProcessing())
            {
                return;
            }
        }
    }

    /// <summary>
    ///     Called after an entity type is added to the model if it has an attribute.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected abstract void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        TAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context);
}
