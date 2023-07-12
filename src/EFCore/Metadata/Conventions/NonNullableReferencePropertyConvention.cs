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
public class NonNullableReferencePropertyConvention : NonNullableConventionBase,
    IPropertyAddedConvention,
    IPropertyFieldChangedConvention,
    IComplexPropertyAddedConvention,
    IComplexPropertyFieldChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="NonNullableReferencePropertyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public NonNullableReferencePropertyConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    private void Process(IConventionPropertyBuilder propertyBuilder)
    {
        if (propertyBuilder.Metadata.GetIdentifyingMemberInfo() is MemberInfo memberInfo
            && IsNonNullableReferenceType(propertyBuilder.ModelBuilder, memberInfo))
        {
            propertyBuilder.IsRequired(true);
        }
    }

    private void Process(IConventionComplexPropertyBuilder propertyBuilder)
    {
        if (propertyBuilder.Metadata.GetIdentifyingMemberInfo() is MemberInfo memberInfo
            && IsNonNullableReferenceType(propertyBuilder.ModelBuilder, memberInfo))
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
    public void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
        => Process(propertyBuilder);

    /// <inheritdoc />
    public void ProcessComplexPropertyFieldChanged(
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
