// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Migrations
{
    public class MigrationsAnnotationProvider : IMigrationsAnnotationProvider
    {
        public virtual IEnumerable<IAnnotation> For(IEntityType entityType) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IForeignKey foreignKey) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IIndex index) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IKey key) => Enumerable.Empty<IAnnotation>();
        public virtual IEnumerable<IAnnotation> For(IProperty property) => Enumerable.Empty<IAnnotation>();
    }
}
