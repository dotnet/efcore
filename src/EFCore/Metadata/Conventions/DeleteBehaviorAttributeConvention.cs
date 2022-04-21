// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the delete behavior based on the <see cref="DeleteBehaviorAttribute" /> applied on the property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class DeleteBehaviorAttributeConvention : PropertyAttributeConventionBase<DeleteBehaviorAttribute>, INavigationAddedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="DeleteBehaviorAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public DeleteBehaviorAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies) { }

    /// <summary>
    ///     Called after a property is added to the entity type with an attribute on the associated CLR property or field.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="clrMember">The member that has the attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        DeleteBehaviorAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        throw new InvalidOperationException(CoreStrings.DeleteBehaviorAttributeNotOnNavigationProperty); 
    }

    /// <summary>
    ///     Called after a navigation is added to the entity type.
    /// </summary>
    /// <param name="navigationBuilder">The builder for the navigation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public void ProcessNavigationAdded(IConventionNavigationBuilder navigationBuilder, IConventionContext<IConventionNavigationBuilder> context)
    {
        var navAttribute = navigationBuilder.Metadata.PropertyInfo?.GetCustomAttribute<DeleteBehaviorAttribute>();
        if (navAttribute == null)
        {
            return;
        }

        if (!navigationBuilder.Metadata.IsOnDependent)
        {
            throw new InvalidOperationException(CoreStrings.DeleteBehaviorAttributeOnPrincipalProperty);
        }

        var foreignKey = navigationBuilder.Metadata.ForeignKey;
        if (foreignKey.GetDeleteBehaviorConfigurationSource() != ConfigurationSource.Explicit)
        {
            foreignKey.SetDeleteBehavior(navAttribute.Behavior);
        }
    }
}
