// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class JoinCompositeKeyToLeaf
    {
        public virtual int CompositeId1 { get; set; }
        public virtual string CompositeId2 { get; set; }
        public virtual DateTime CompositeId3 { get; set; }
        public virtual int LeafId { get; set; }

        public virtual EntityCompositeKey Composite { get; set; }
        public virtual EntityLeaf Leaf { get; set; }
    }
}
