// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the inverse navigation property based on the <see cref="InversePropertyAttribute" />
///     specified on the other navigation property.
///     All navigations are assumed to be targeting owned entity types for Cosmos.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public class CosmosInversePropertyAttributeConvention : InversePropertyAttributeConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="InversePropertyAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public CosmosInversePropertyAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Finds or tries to create an entity type target for the given navigation member.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the referencing entity type.</param>
    /// <param name="targetClrType">The CLR type of the target entity type.</param>
    /// <param name="navigationMemberInfo">The navigation member.</param>
    /// <param name="shouldCreate">Whether an entity type should be created if one doesn't currently exist.</param>
    /// <returns>The builder for the target entity type or <see langword="null" /> if it can't be created.</returns>
    protected override IConventionEntityTypeBuilder? TryGetTargetEntityTypeBuilder(
        IConventionEntityTypeBuilder entityTypeBuilder,
        Type targetClrType,
        MemberInfo navigationMemberInfo,
        bool shouldCreate = true)
        => entityTypeBuilder
            .GetTargetEntityTypeBuilder(
                targetClrType,
                navigationMemberInfo,
                shouldCreate,
                CosmosRelationshipDiscoveryConvention.ShouldBeOwnedType(targetClrType, entityTypeBuilder.Metadata.Model),
                fromDataAnnotation: true);
}
