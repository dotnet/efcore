// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the properties of non-nullable types as required.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class NonNullableReferencePropertyConvention(ProviderConventionSetBuilderDependencies dependencies)
    : NonNullableConventionBase(dependencies),
    IPropertyAddedConvention,
    IPropertyFieldChangedConvention,
    IPropertyElementTypeChangedConvention,
    IComplexPropertyAddedConvention,
    IComplexPropertyFieldChangedConvention
{
    private void Process(IConventionPropertyBuilder propertyBuilder)
    {
        if (propertyBuilder.Metadata.GetIdentifyingMemberInfo() is { } memberInfo
            && TryGetNullabilityInfo(propertyBuilder.ModelBuilder, memberInfo, out var nullabilityInfo))
        {
            if (nullabilityInfo.ReadState == NullabilityState.NotNull)
            {
                propertyBuilder.IsRequired(true);
            }

            // If there's an element type, this is a primitive collection; check and apply the element's nullability as well.
            if (propertyBuilder.Metadata.GetElementType() is { } elementType
                && nullabilityInfo is
                    { ElementType.ReadState: NullabilityState.NotNull } or
                    { GenericTypeArguments: [{ ReadState: NullabilityState.NotNull }] })
            {
                elementType.SetIsNullable(false);
            }
        }
    }

    private void Process(IConventionComplexPropertyBuilder propertyBuilder)
    {
        if (propertyBuilder.Metadata.GetIdentifyingMemberInfo() is { } memberInfo
            && TryGetNullabilityInfo(propertyBuilder.ModelBuilder, memberInfo, out var nullabilityInfo)
            && nullabilityInfo.ReadState == NullabilityState.NotNull)
        {
            propertyBuilder.IsRequired(true);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
        => Process(propertyBuilder);

    /// <inheritdoc />
    public virtual void ProcessPropertyFieldChanged(
        IConventionPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo,
        IConventionContext<FieldInfo> context)
    {
        if (propertyBuilder.Metadata.PropertyInfo == null)
        {
            Process(propertyBuilder);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessPropertyElementTypeChanged(
        IConventionPropertyBuilder propertyBuilder,
        IElementType? newElementType,
        IElementType? oldElementType,
        IConventionContext<IElementType> context)
    {
        if (newElementType != null)
        {
            Process(propertyBuilder);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
        => Process(propertyBuilder);

    /// <inheritdoc />
    public virtual void ProcessComplexPropertyFieldChanged(
        IConventionComplexPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo,
        IConventionContext<FieldInfo> context)
    {
        if (propertyBuilder.Metadata.PropertyInfo == null)
        {
            Process(propertyBuilder);
        }
    }
}
