// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A base type for conventions that perform configuration based on an attribute applied to a property.
/// </summary>
/// <remarks>
///     <para>
///         The deriving class must implement <see cref="IPropertyAddedConvention" /> and
///         <see cref="IPropertyFieldChangedConvention" /> to also handle complex properties.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
/// <typeparam name="TAttribute">The attribute type to look for.</typeparam>
public abstract class PropertyAttributeConventionBase<TAttribute> :
    IPropertyAddedConvention,
    IPropertyFieldChangedConvention
    where TAttribute : Attribute
{
    /// <summary>
    ///     Creates a new instance of <see cref="PropertyAttributeConventionBase{TAttribute}" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    protected PropertyAttributeConventionBase(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
    {
        Check.NotNull(propertyBuilder, nameof(propertyBuilder));

        var memberInfo = propertyBuilder.Metadata.GetIdentifyingMemberInfo();
        if (memberInfo == null)
        {
            return;
        }

        Process(propertyBuilder, memberInfo, (IReadableConventionContext)context);
    }

    /// <inheritdoc />
    public virtual void ProcessPropertyFieldChanged(
        IConventionPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo,
        IConventionContext<FieldInfo> context)
    {
        if (newFieldInfo != null
            && propertyBuilder.Metadata.PropertyInfo == null)
        {
            Process(propertyBuilder, newFieldInfo, (IReadableConventionContext)context);
        }
    }

    private void Process(IConventionPropertyBuilder propertyBuilder, MemberInfo memberInfo, IReadableConventionContext context)
    {
        if (!Attribute.IsDefined(memberInfo, typeof(TAttribute), inherit: true))
        {
            return;
        }

        var attributes = memberInfo.GetCustomAttributes<TAttribute>(inherit: true);

        foreach (var attribute in attributes)
        {
            ProcessPropertyAdded(propertyBuilder, attribute, memberInfo, context);
            if (context.ShouldStopProcessing())
            {
                break;
            }
        }
    }

    /// <summary>
    ///     Called after a complex property is added to an type-like object.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the complex property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
    {
        Check.NotNull(propertyBuilder, nameof(propertyBuilder));

        var memberInfo = propertyBuilder.Metadata.GetIdentifyingMemberInfo();
        if (memberInfo == null)
        {
            return;
        }

        Process(propertyBuilder, memberInfo, (IReadableConventionContext)context);
    }

    /// <summary>
    ///     Called after the backing field for a complex property is changed.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="newFieldInfo">The new field.</param>
    /// <param name="oldFieldInfo">The old field.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessComplexPropertyFieldChanged(
        IConventionComplexPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo,
        IConventionContext<FieldInfo> context)
    {
        if (newFieldInfo != null
            && propertyBuilder.Metadata.PropertyInfo == null)
        {
            Process(propertyBuilder, newFieldInfo, (IReadableConventionContext)context);
        }
    }

    private void Process(IConventionComplexPropertyBuilder propertyBuilder, MemberInfo memberInfo, IReadableConventionContext context)
    {
        if (!Attribute.IsDefined(memberInfo, typeof(TAttribute), inherit: true))
        {
            return;
        }

        var attributes = memberInfo.GetCustomAttributes<TAttribute>(inherit: true);

        foreach (var attribute in attributes)
        {
            ProcessPropertyAdded(propertyBuilder, attribute, memberInfo, context);
            if (context.ShouldStopProcessing())
            {
                break;
            }
        }
    }

    /// <summary>
    ///     Called after a property is added to the entity type with an attribute on the associated CLR property or field.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="clrMember">The member that has the attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected abstract void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        TAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context);

    /// <summary>
    ///     Called after a complex property is added to a type with an attribute on the associated CLR property or field.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="clrMember">The member that has the attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected virtual void ProcessPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        TAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
        => throw new NotSupportedException();
}
