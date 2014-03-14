// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IProperty : IMetadata
    {
        string Name { get; }
        Type PropertyType { get; }
        IEntityType EntityType { get; }
        bool IsNullable { get; }
        ValueGenerationStrategy ValueGenerationStrategy { get; }
        int Index { get; }
        int ShadowIndex { get; }
        bool HasClrProperty { get; }
    }
}
