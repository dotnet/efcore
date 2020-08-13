// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures indexes as non-clustered for memory-optimized tables.
    /// </summary>
    public class SqlServerMemoryOptimizedTablesConvention :
        IEntityTypeAnnotationChangedConvention,
        IKeyAddedConvention,
        IIndexAddedConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerMemoryOptimizedTablesConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqlServerMemoryOptimizedTablesConvention(
            [NotNull] ProviderConventionSetBuilderDependencies dependencies,
            [NotNull] RelationalConventionSetBuilderDependencies relationalDependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

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
            if (name == SqlServerAnnotationNames.MemoryOptimized)
            {
                var memoryOptimized = annotation?.Value as bool? == true;
                foreach (var key in entityTypeBuilder.Metadata.GetDeclaredKeys())
                {
                    key.Builder.IsClustered(memoryOptimized ? false : (bool?)null);
                }

                foreach (var index in
                    entityTypeBuilder.Metadata.GetDerivedTypesInclusive().SelectMany(et => et.GetDeclaredIndexes()))
                {
                    index.Builder.IsClustered(memoryOptimized ? false : (bool?)null);
                }
            }
        }

        /// <summary>
        ///     Called after a key is added to the entity type.
        /// </summary>
        /// <param name="keyBuilder"> The builder for the key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context)
        {
            if (keyBuilder.Metadata.DeclaringEntityType.IsMemoryOptimized())
            {
                keyBuilder.IsClustered(false);
            }
        }

        /// <summary>
        ///     Called after an index is added to the entity type.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessIndexAdded(IConventionIndexBuilder indexBuilder, IConventionContext<IConventionIndexBuilder> context)
        {
            if (indexBuilder.Metadata.DeclaringEntityType.GetAllBaseTypesInclusive().Any(et => et.IsMemoryOptimized()))
            {
                indexBuilder.IsClustered(false);
            }
        }
    }
}
