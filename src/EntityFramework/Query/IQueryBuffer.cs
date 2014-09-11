// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Query
{
    public interface IQueryBuffer
    {
        object GetEntity([NotNull] IEntityType entityType, [NotNull] IValueReader valueReader);
        object GetPropertyValue([NotNull] object entity, [NotNull] IProperty property);

        object StartTracking([NotNull] object entity);

        void Include(
            [NotNull] object entity,
            [NotNull] INavigation navigation,
            [NotNull] IEnumerable<IValueReader> relatedValueReaders);
    }
}
