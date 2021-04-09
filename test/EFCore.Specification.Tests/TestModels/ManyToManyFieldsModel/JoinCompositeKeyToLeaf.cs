// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class JoinCompositeKeyToLeaf
    {
        public int CompositeId1;
        public string CompositeId2;
        public DateTime CompositeId3;
        public int LeafId;

        public EntityCompositeKey Composite;
        public EntityLeaf Leaf;
    }
}
