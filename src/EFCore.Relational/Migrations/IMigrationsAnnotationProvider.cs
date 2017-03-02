// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public interface IMigrationsAnnotationProvider
    {
        IEnumerable<IAnnotation> For([NotNull] IModel model);
        IEnumerable<IAnnotation> For([NotNull] IIndex index);
        IEnumerable<IAnnotation> For([NotNull] IProperty property);
        IEnumerable<IAnnotation> For([NotNull] IKey key);
        IEnumerable<IAnnotation> For([NotNull] IForeignKey foreignKey);
        IEnumerable<IAnnotation> For([NotNull] IEntityType entityType);
        IEnumerable<IAnnotation> For([NotNull] ISequence sequence);

        IEnumerable<IAnnotation> ForRemove([NotNull] IModel model);
        IEnumerable<IAnnotation> ForRemove([NotNull] IIndex index);
        IEnumerable<IAnnotation> ForRemove([NotNull] IProperty property);
        IEnumerable<IAnnotation> ForRemove([NotNull] IKey key);
        IEnumerable<IAnnotation> ForRemove([NotNull] IForeignKey foreignKey);
        IEnumerable<IAnnotation> ForRemove([NotNull] IEntityType entityType);
        IEnumerable<IAnnotation> ForRemove([NotNull] ISequence sequence);
    }
}
