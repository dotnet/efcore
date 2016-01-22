// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class PropertyIndexes
    {
        public PropertyIndexes(int index, int originalValueIndex, int shadowIndex, int relationshipIndex, int storeGenerationIndex)
        {
            Index = index;
            OriginalValueIndex = originalValueIndex;
            ShadowIndex = shadowIndex;
            RelationshipIndex = relationshipIndex;
            StoreGenerationIndex = storeGenerationIndex;
        }

        public virtual int Index { get; }
        public virtual int OriginalValueIndex { get; }
        public virtual int ShadowIndex { get; }
        public virtual int RelationshipIndex { get; }
        public virtual int StoreGenerationIndex { get; }
    }
}
