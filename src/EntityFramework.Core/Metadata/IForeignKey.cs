// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IForeignKey : IMetadata
    {
        IEntityType EntityType { get; }
        IReadOnlyList<IProperty> Properties { get; }
        IReadOnlyList<IProperty> ReferencedProperties { get; }
        IEntityType ReferencedEntityType { get; }
        IKey ReferencedKey { get; }
        bool IsUnique { get; }
        bool IsRequired { get; }
    }
}
