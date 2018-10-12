// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal
{
    public class CosmosEntityTypeBuilderAnnotations : CosmosEntityTypeAnnotations
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CosmosEntityTypeBuilderAnnotations(
            [NotNull] InternalEntityTypeBuilder internalBuilder,
            ConfigurationSource configurationSource)
            : base(new CosmosAnnotationsBuilder(internalBuilder, configurationSource))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected new virtual CosmosAnnotationsBuilder Annotations => (CosmosAnnotationsBuilder)base.Annotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalEntityTypeBuilder EntityTypeBuilder => (InternalEntityTypeBuilder)Annotations.MetadataBuilder;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override CosmosModelAnnotations GetAnnotations(IModel model)
            => new CosmosModelBuilderAnnotations(
                ((Model)model).Builder,
                Annotations.ConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override CosmosEntityTypeAnnotations GetAnnotations(IEntityType entityType)
            => new CosmosEntityTypeBuilderAnnotations(
                ((EntityType)entityType).Builder,
                Annotations.ConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ToContainer([CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            return SetContainerName(name);
        }
    }
}
