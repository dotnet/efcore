// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class MigrationAnnotationProvider : IMigrationAnnotationProvider
    {
        private static readonly IReadOnlyList<IAnnotation> _empty = new IAnnotation[0];

        public virtual IEnumerable<IAnnotation> For(IEntityType entityType) => _empty;
        public virtual IEnumerable<IAnnotation> For(IForeignKey foreignKey) => _empty;
        public virtual IEnumerable<IAnnotation> For(IIndex index) => _empty;
        public virtual IEnumerable<IAnnotation> For(IKey key) => _empty;
        public virtual IEnumerable<IAnnotation> For(IProperty property) => _empty;
    }
}
