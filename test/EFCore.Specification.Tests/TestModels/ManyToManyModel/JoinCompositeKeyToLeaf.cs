// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class JoinCompositeKeyToLeaf
    {
        public int CompositeId1 { get; set; }
        public string CompositeId2 { get; set; }
        public DateTime CompositeId3 { get; set; }
        public int LeafId { get; set; }

        public EntityCompositeKey Composite { get; set; }
        public EntityLeaf Leaf { get; set; }
    }
}
