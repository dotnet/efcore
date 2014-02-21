// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IForeignKey : IMetadata
    {
        IEnumerable<Property> Properties { get; }
        EntityType ReferencedEntityType { get; }
        bool IsUnique { get; }
        bool IsRequired { get; }
        string StorageName { get; }
    }
}
