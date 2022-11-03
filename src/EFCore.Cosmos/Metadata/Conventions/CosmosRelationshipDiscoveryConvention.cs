// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures relationships between entity types based on the navigation properties
///     as long as there is no ambiguity as to which is the corresponding inverse navigation.
///     All navigations are assumed to be targeting owned entity types for Cosmos.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public class CosmosRelationshipDiscoveryConvention : RelationshipDiscoveryConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="RelationshipDiscoveryConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public CosmosRelationshipDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Returns a value indicating whether the given entity type should be added as owned if it isn't currently in the model.
    /// </summary>
    /// <param name="targetType">Target entity type.</param>
    /// <param name="model">The model.</param>
    /// <returns><see langword="true" /> if the given entity type should be owned.</returns>
    protected override bool? ShouldBeOwned(Type targetType, IConventionModel model)
        => ShouldBeOwnedType(targetType, model);

    /// <summary>
    ///     Returns a value indicating whether the given entity type should be added as owned if it isn't currently in the model.
    /// </summary>
    /// <param name="targetType">Target entity type.</param>
    /// <param name="model">The model.</param>
    /// <returns><see langword="true" /> if the given entity type should be owned.</returns>
    public static bool ShouldBeOwnedType(Type targetType, IConventionModel model)
        => !targetType.IsGenericType
            || targetType == typeof(Dictionary<string, object>)
            || targetType.GetInterface(typeof(IEnumerable<>).Name) == null;
}
