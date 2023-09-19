// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A base type for conventions that perform configuration based on an attribute specified on a structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
/// <typeparam name="TAttribute">The attribute type to look for.</typeparam>
public abstract class TypeAttributeConventionBase<TAttribute> : IEntityTypeAddedConvention
    where TAttribute : Attribute
{
    /// <summary>
    ///     Creates a new instance of <see cref="TypeAttributeConventionBase{TAttribute}" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    protected TypeAttributeConventionBase(ProviderConventionSetBuilderDependencies dependencies)
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
        var type = entityTypeBuilder.Metadata.ClrType;

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
    ///     Called after a complex property is added to a type-like object.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the complex property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
    {
        var complexType = propertyBuilder.Metadata.ComplexType;
        var type = complexType.ClrType;
        var attributes = type.GetCustomAttributes<TAttribute>(true);
        foreach (var attribute in attributes)
        {
            ProcessComplexTypeAdded(complexType.Builder, attribute, context);
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

    /// <summary>
    ///     Called after an complex type is added to the model if it has an attribute.
    /// </summary>
    /// <param name="complexTypeBuilder">The builder for the complex type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected virtual void ProcessComplexTypeAdded(
        IConventionComplexTypeBuilder complexTypeBuilder,
        TAttribute attribute,
        IConventionContext context)
        => throw new NotSupportedException();

    /// <summary>
    ///     Tries to replace the complex type with an entity type.
    /// </summary>
    /// <param name="complexTypeBuilder">The complex type builder.</param>
    /// <param name="shouldBeOwned">A value indicating whether the new entity type should be owned.</param>
    /// <returns>The builder for the new entity type.</returns>
    protected virtual IConventionEntityTypeBuilder? ReplaceWithEntityType(
        IConventionComplexTypeBuilder complexTypeBuilder,
        bool? shouldBeOwned = null)
    {
        var modelBuilder = complexTypeBuilder.ModelBuilder;
        if (!modelBuilder.CanHaveEntity(complexTypeBuilder.Metadata.ClrType, fromDataAnnotation: true))
        {
            return null;
        }

        var complexProperty = complexTypeBuilder.Metadata.ComplexProperty;
        switch (complexProperty.DeclaringType.Builder)
        {
            case IConventionEntityTypeBuilder conventionEntityTypeBuilder:
                if (conventionEntityTypeBuilder.HasNoComplexProperty(complexProperty, fromDataAnnotation: true) == null)
                {
                    return null;
                }

                break;
            case IConventionComplexTypeBuilder conventionComplexTypeBuilder:
                if (conventionComplexTypeBuilder.HasNoComplexProperty(complexProperty, fromDataAnnotation: true) == null)
                {
                    return null;
                }

                break;
        }

        return complexTypeBuilder.ModelBuilder.Entity(complexTypeBuilder.Metadata.ClrType, shouldBeOwned, fromDataAnnotation: true);
    }
}
