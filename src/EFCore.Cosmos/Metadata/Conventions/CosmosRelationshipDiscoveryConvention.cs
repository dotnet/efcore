// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures relationships between entity types based on the navigation properties
    ///     as long as there is no ambiguity as to which is the corresponding inverse navigation.
    ///     All navigations are assumed to be targeting owned entity types for Cosmos.
    /// </summary>
    public class CosmosRelationshipDiscoveryConvention : RelationshipDiscoveryConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationshipDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public CosmosRelationshipDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Returns a value indicating whether the given entity type should be added as owned if it isn't currently in the model.
        /// </summary>
        /// <param name="targetType"> Target entity type. </param>
        /// <param name="model"> The model. </param>
        /// <returns> <see langword="true"/> if the given entity type should be owned. </returns>
        protected override bool? ShouldBeOwned(Type targetType, IConventionModel model)
            => true;
    }
}
