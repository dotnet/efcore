// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IModel : IAnnotatable
    {
        [CanBeNull]
        IEntityType FindEntityType([NotNull] Type type);

        [NotNull]
        IEntityType GetEntityType([NotNull] Type type);

        IReadOnlyList<IEntityType> EntityTypes { get; }

        [CanBeNull]
        IEntityType FindEntityType([NotNull] string name);

        [NotNull]
        IEntityType GetEntityType([NotNull] string name);

        [NotNull]
        IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] IEntityType entityType);

        [NotNull]
        IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] IKey key);

        [NotNull]
        IEnumerable<IForeignKey> GetReferencingForeignKeys([NotNull] IProperty property);
    }
}
