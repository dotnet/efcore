// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IEntityType : IMetadata
    {
        string Name { get; }
        string StorageName { get; }
        Type Type { get; }
        IEnumerable<IProperty> Key { get; }
        IProperty Property([NotNull] string name);
        IEnumerable<IProperty> Properties { get; }
        EntityKey CreateEntityKey([NotNull] object entity);
    }
}
