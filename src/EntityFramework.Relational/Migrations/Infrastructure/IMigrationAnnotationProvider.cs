// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public interface IMigrationAnnotationProvider
    {
        IEnumerable<IAnnotation> For([NotNull] IIndex index);
        IEnumerable<IAnnotation> For([NotNull] IProperty property);
        IEnumerable<IAnnotation> For([NotNull] IKey key);
        IEnumerable<IAnnotation> For([NotNull] IForeignKey foreignKey);
        IEnumerable<IAnnotation> For([NotNull] IEntityType entityType);
    }
}
