// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class MigrationsAnnotationProvider : IMigrationsAnnotationProvider
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public MigrationsAnnotationProvider([NotNull] MigrationsAnnotationProviderDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        public virtual IEnumerable<IAnnotation> For(IModel model) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IEntityType entityType) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IForeignKey foreignKey) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IIndex index) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IKey key) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IProperty property) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(ISequence sequence) => Enumerable.Empty<IAnnotation>();

        public virtual IEnumerable<IAnnotation> ForRemove(IModel model) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> ForRemove(IEntityType entityType) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> ForRemove(IForeignKey foreignKey) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> ForRemove(IIndex index) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> ForRemove(IKey key) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> ForRemove(IProperty property) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> ForRemove(ISequence sequence) => Enumerable.Empty<IAnnotation>();
    }
}
