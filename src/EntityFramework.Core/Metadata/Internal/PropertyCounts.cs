// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class PropertyCounts
    {
        public PropertyCounts(
            int propertyCount, 
            int navigationCount, 
            int originalValueCount, 
            int shadowCount, 
            int relationshipCount,
            int storeGeneratedCount)
        {
            PropertyCount = propertyCount;
            NavigationCount = navigationCount;
            OriginalValueCount = originalValueCount;
            ShadowCount = shadowCount;
            RelationshipCount = relationshipCount;
            StoreGeneratedCount = storeGeneratedCount;
        }

        public virtual int PropertyCount { get; }
        public virtual int NavigationCount { get; }
        public virtual int OriginalValueCount { get; }
        public virtual int ShadowCount { get; }
        public virtual int RelationshipCount { get; }
        public virtual int StoreGeneratedCount { get; }
    }
}
