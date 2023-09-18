// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the delete behavior based on the <see cref="DeleteBehaviorAttribute" /> applied on the property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class DeleteBehaviorAttributeConvention : PropertyAttributeConventionBase<DeleteBehaviorAttribute>,
    INavigationAddedConvention,
    IForeignKeyPrincipalEndChangedConvention,
    IComplexPropertyAddedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteBehaviorAttributeConvention" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public DeleteBehaviorAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public virtual void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        var navAttribute = navigationBuilder.Metadata.PropertyInfo?.GetCustomAttribute<DeleteBehaviorAttribute>();
        if (navAttribute == null)
        {
            return;
        }

        var foreignKey = navigationBuilder.Metadata.ForeignKey;
        if (!navigationBuilder.Metadata.IsOnDependent && foreignKey.IsUnique)
        {
            return;
        }

        foreignKey.Builder.OnDelete(navAttribute.Behavior, fromDataAnnotation: true);
    }

    /// <inheritdoc />
    public virtual void ProcessForeignKeyPrincipalEndChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
    {
        if (!relationshipBuilder.Metadata.IsUnique)
        {
            return;
        }

        var navigation = relationshipBuilder.Metadata.DependentToPrincipal;
        var navAttribute = navigation?.PropertyInfo?.GetCustomAttribute<DeleteBehaviorAttribute>();
        if (navAttribute == null)
        {
            return;
        }

        relationshipBuilder.OnDelete(navAttribute.Behavior, fromDataAnnotation: true);
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var navigation in entityType.GetDeclaredNavigations())
            {
                if (navigation.IsOnDependent)
                {
                    return;
                }

                var navAttribute = navigation.PropertyInfo?.GetCustomAttribute<DeleteBehaviorAttribute>();
                if (navAttribute != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.DeleteBehaviorAttributeOnPrincipalProperty(
                            navigation.DeclaringEntityType.DisplayName(), navigation.Name));
                }
            }
        }
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        DeleteBehaviorAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        var property = propertyBuilder.Metadata;
        throw new InvalidOperationException(
            CoreStrings.DeleteBehaviorAttributeNotOnNavigationProperty(
                property.DeclaringType.DisplayName(), property.Name));
    }

    /// <inheritdoc />
    protected override void ProcessPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        DeleteBehaviorAttribute attribute,
        MemberInfo clrMember,
        IConventionContext context)
    {
        var property = propertyBuilder.Metadata;
        throw new InvalidOperationException(
            CoreStrings.DeleteBehaviorAttributeNotOnNavigationProperty(
                property.DeclaringType.DisplayName(), property.Name));
    }
}
