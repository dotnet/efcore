// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the principal side of the relationship as required if the
///     <see cref="RequiredAttribute" /> is applied on the navigation property to the principal entity type, and configures an owned
///     collection navigation with the <see cref="RequiredAttribute" /> as not nullable, see <see cref="IReadOnlyNavigation.IsNullable" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class RequiredNavigationAttributeConvention :
    NavigationAttributeConventionBase<RequiredAttribute>,
    INavigationAddedConvention,
    ISkipNavigationAddedConvention,
    IForeignKeyPrincipalEndChangedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RequiredNavigationAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public RequiredNavigationAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        RequiredAttribute attribute,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        ProcessNavigation(navigationBuilder);
        context.StopProcessingIfChanged(navigationBuilder.Metadata.Builder);
    }

    /// <inheritdoc />
    public override void ProcessForeignKeyPrincipalEndChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IEnumerable<RequiredAttribute>? dependentToPrincipalAttributes,
        IEnumerable<RequiredAttribute>? principalToDependentAttributes,
        IConventionContext<IConventionForeignKeyBuilder> context)
    {
        var fk = relationshipBuilder.Metadata;
        if (dependentToPrincipalAttributes != null
            && dependentToPrincipalAttributes.Any())
        {
            ProcessNavigation(fk.DependentToPrincipal!.Builder);
        }

        if (principalToDependentAttributes != null
            && principalToDependentAttributes.Any())
        {
            ProcessNavigation(fk.PrincipalToDependent!.Builder);
        }

        context.StopProcessingIfChanged(relationshipBuilder.Metadata.Builder);
    }

    private void ProcessNavigation(IConventionNavigationBuilder navigationBuilder)
    {
        var navigation = navigationBuilder.Metadata;
        var foreignKey = navigation.ForeignKey;
        if (navigation.IsCollection)
        {
            // Whether the collection is owned is only known once the model is built, see ProcessModelFinalizing
            return;
        }

        if (navigation.IsOnDependent)
        {
            foreignKey.Builder.IsRequired(true, fromDataAnnotation: true);
        }
        else
        {
            foreignKey.Builder.IsRequiredDependent(true, fromDataAnnotation: true);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var navigation in entityType.GetDeclaredNavigations())
            {
                if (!navigation.IsCollection
                    || !GetAttributes<RequiredAttribute>(entityType, navigation).Any())
                {
                    continue;
                }

                if (navigation.ForeignKey.IsOwnership)
                {
                    // For an owned collection, it means that the collection can't be null as distinct from empty. This is not
                    // applied if it was configured explicitly.
                    navigation.Builder.IsRequired(true, fromDataAnnotation: true);
                }
                else
                {
                    Dependencies.Logger.RequiredAttributeOnCollection(navigation);
                }
            }
        }
    }

    /// <inheritdoc />
    public override void ProcessSkipNavigationAdded(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        RequiredAttribute attribute,
        IConventionContext<IConventionSkipNavigationBuilder> context)
        => Dependencies.Logger.RequiredAttributeOnSkipNavigation(skipNavigationBuilder.Metadata);
}
