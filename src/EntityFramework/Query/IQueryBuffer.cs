// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Query
{
    public interface IQueryBuffer
    {
        object GetEntity([NotNull] IEntityType entityType, [NotNull] IValueReader valueReader);
        object GetPropertyValue([NotNull] object entity, [NotNull] IProperty property);

        void StartTracking([NotNull] object entity);

        void Include(
            [NotNull] object entity,
            [NotNull] INavigation navigation,
            [NotNull] Func<EntityKey, Func<IValueReader, EntityKey>, IEnumerable<IValueReader>> relatedValueReaders);

        Task IncludeAsync(
            [NotNull] object entity,
            [NotNull] INavigation navigation,
            [NotNull] Func<EntityKey, Func<IValueReader, EntityKey>, IAsyncEnumerable<IValueReader>> relatedValueReaders,
            CancellationToken cancellationToken);
    }
}
