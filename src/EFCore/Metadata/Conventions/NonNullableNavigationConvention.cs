// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the non-nullable navigations to principal entity type as required.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class NonNullableNavigationConvention :
    NonNullableConventionBase,
    INavigationAddedConvention,
    IForeignKeyPrincipalEndChangedConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="NonNullableNavigationConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public NonNullableNavigationConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public virtual void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        ProcessNavigation(navigationBuilder);
        context.StopProcessingIfChanged(navigationBuilder.Metadata.Builder);
    }

    /// <inheritdoc />
    public virtual void ProcessForeignKeyPrincipalEndChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
    {
        var fk = relationshipBuilder.Metadata;
        if (fk.DependentToPrincipal != null)
        {
            ProcessNavigation(fk.DependentToPrincipal.Builder);
        }

        if (fk.PrincipalToDependent != null)
        {
            ProcessNavigation(fk.PrincipalToDependent.Builder);
        }

        context.StopProcessingIfChanged(relationshipBuilder.Metadata.Builder);
    }

    private void ProcessNavigation(IConventionNavigationBuilder navigationBuilder)
    {
        var navigation = navigationBuilder.Metadata;
        var foreignKey = navigation.ForeignKey;
        var modelBuilder = navigationBuilder.ModelBuilder;

        if (!IsNonNullable(modelBuilder, navigation)
            || navigation.IsCollection)
        {
            return;
        }

        if (navigation.IsOnDependent)
        {
            if (foreignKey.Properties.All(p =>
                !p.IsNullable
                || (p.IsShadowProperty()
                    && ConfigurationSource.Convention.Overrides(p.GetIsNullableConfigurationSource()))))
            {
                foreignKey.Builder.IsRequired(true);
            }
        }
        else
        {
            foreignKey.Builder.IsRequiredDependent(true);
        }
    }

    private bool IsNonNullable(IConventionModelBuilder modelBuilder, IConventionNavigation navigation)
        => navigation.DeclaringEntityType.GetRuntimeProperties().Find(navigation.Name) is PropertyInfo propertyInfo
            && TryGetNullabilityInfo(modelBuilder, propertyInfo, out var nullabilityInfo)
            && nullabilityInfo.ReadState == NullabilityState.NotNull;
}
