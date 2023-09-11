// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that adds properties to entity types corresponding to scalar public properties on the CLR type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class PropertyDiscoveryConvention :
    IEntityTypeAddedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IComplexPropertyAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="PropertyDiscoveryConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public PropertyDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => Process(entityTypeBuilder);

    /// <inheritdoc />
    public void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
    {
        var complexType = propertyBuilder.Metadata.ComplexType;
        var model = complexType.Model;
        foreach (var propertyInfo in complexType.GetRuntimeProperties().Values)
        {
            if (!Dependencies.MemberClassifier.IsCandidatePrimitiveProperty(propertyInfo, model, out _))
            {
                continue;
            }

            complexType.Builder.Property(propertyInfo);
        }

        foreach (var fieldInfo in complexType.GetRuntimeFields().Values)
        {
            if (!Dependencies.MemberClassifier.IsCandidatePrimitiveProperty(fieldInfo, model, out _))
            {
                continue;
            }

            complexType.Builder.Property(fieldInfo);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if ((newBaseType == null
                || oldBaseType != null)
            && entityTypeBuilder.Metadata.BaseType == newBaseType)
        {
            Process(entityTypeBuilder);
        }
    }

    private void Process(IConventionEntityTypeBuilder entityTypeBuilder)
    {
        var entityType = entityTypeBuilder.Metadata;
        var model = entityType.Model;
        foreach (var propertyInfo in entityType.GetRuntimeProperties().Values)
        {
            if (!Dependencies.MemberClassifier.IsCandidatePrimitiveProperty(propertyInfo, model, out var mapping)
                || ((Model)model).FindIsComplexConfigurationSource(propertyInfo.GetMemberType().UnwrapNullableType()) != null)
            {
                continue;
            }

            var propertyBuilder = entityTypeBuilder.Property(propertyInfo);
            if (mapping?.ElementTypeMapping != null)
            {
                var elementType = propertyInfo.PropertyType.TryGetElementType(typeof(IEnumerable<>));
                if (elementType != null)
                {
                    propertyBuilder?.SetElementType(elementType);
                }
            }
        }
    }
}
