// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     Provides strongly typed access to scaffolding related annotations on an
    ///     <see cref="IEntityType" /> instance. Instances of this class are typically obtained via the
    ///     <see cref="ScaffoldingMetadataExtensions.Scaffolding(IEntityType)" /> extension method and it is not designed
    ///     to be directly constructed in your application code.
    /// </summary>
    public class ScaffoldingEntityTypeAnnotations : RelationalEntityTypeAnnotations
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ScaffoldingEntityTypeAnnotations" /> class.
        ///     Instances of this class are typically obtained via the
        ///     <see cref="ScaffoldingMetadataExtensions.Scaffolding(IEntityType)" /> extension method and it is not designed
        ///     to be directly constructed in your application code.
        /// </summary>
        /// <param name="entity"> The entity type to access annotation on. </param>
        public ScaffoldingEntityTypeAnnotations([NotNull] IEntityType entity)
            : base(entity)
        {
        }

        /// <summary>
        ///     Gets or set the name of the <see cref="DbSet{TEntity}" /> property for this entity type.
        /// </summary>
        public virtual string DbSetName
        {
            get => (string)Annotations.Metadata[ScaffoldingAnnotationNames.DbSetName]
                   ?? EntityType.Name;

            [param: CanBeNull]
            set => Annotations.SetAnnotation(
                ScaffoldingAnnotationNames.DbSetName,
                Check.NullButNotEmpty(value, nameof(value)));
        }
    }
}
