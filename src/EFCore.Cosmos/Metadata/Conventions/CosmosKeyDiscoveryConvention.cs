// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that finds primary key property for the entity type based on the names
    ///     and adds the partition key to it if present.
    /// </summary>
    public class CosmosKeyDiscoveryConvention :
        KeyDiscoveryConvention,
        IEntityTypeAnnotationChangedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="KeyDiscoveryConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public CosmosKeyDiscoveryConvention([NotNull] ProviderConventionSetBuilderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Called after an annotation is changed on an entity type.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation annotation,
            IConventionAnnotation oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(context, nameof(context));

            if (name == CosmosAnnotationNames.PartitionKeyName)
            {
                TryConfigurePrimaryKey(entityTypeBuilder);
            }
        }

        /// <inheritdoc />
        protected override void ProcessKeyProperties(IList<IConventionProperty> keyProperties, IConventionEntityType entityType)
        {
            if (keyProperties.Count == 0)
            {
                return;
            }

            var partitionKey = entityType.GetPartitionKeyPropertyName();
            if (partitionKey != null)
            {
                var partitionKeyProperty = entityType.FindProperty(partitionKey);
                if (partitionKeyProperty != null
                    && !keyProperties.Contains(partitionKeyProperty))
                {
                    keyProperties.Add(partitionKeyProperty);
                }
            }
        }
    }
}
