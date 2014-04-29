// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata
{
    public interface IPropertyBase : IMetadata
    {
        string Name { get; }
        IEntityType EntityType { get; }
        string StorageName { get; }
    }
}
