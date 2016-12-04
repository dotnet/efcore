// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class MigrationsAnnotationProvider : IMigrationsAnnotationProvider
    {
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
