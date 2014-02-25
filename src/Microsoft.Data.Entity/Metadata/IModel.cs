// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IModel : IMetadata
    {
        [CanBeNull]
        IEntityType EntityType([NotNull] object instance);

        [CanBeNull]
        IEntityType EntityType([NotNull] Type type);

        IEnumerable<IEntityType> EntityTypes { get; }
        IEqualityComparer<object> EntityEqualityComparer { get; }
    }
}
