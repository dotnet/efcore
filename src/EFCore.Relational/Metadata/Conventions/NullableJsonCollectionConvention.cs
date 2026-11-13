// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures owned collection navigations mapped to a JSON column as nullable when the
///     navigation is annotated as nullable, so that <see langword="null" /> and an empty collection are different values.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class NullableJsonCollectionConvention(ProviderConventionSetBuilderDependencies dependencies) :
    NonNullableConventionBase(dependencies),
    IModelFinalizingConvention
{
    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (!entityType.IsMappedToJson()
                || entityType.FindOwnership() is not { PrincipalToDependent: { IsCollection: true } navigation }
                || !ConfigurationSource.Convention.Overrides(navigation.GetIsNullableConfigurationSource()))
            {
                continue;
            }

            var memberInfo = navigation.PropertyInfo ?? (MemberInfo?)navigation.FieldInfo;
            if (memberInfo != null
                && TryGetNullabilityInfo(modelBuilder, memberInfo, out var nullabilityInfo)
                && nullabilityInfo.ReadState == NullabilityState.Nullable)
            {
                navigation.SetIsNullable(true);
            }
        }
    }
}
