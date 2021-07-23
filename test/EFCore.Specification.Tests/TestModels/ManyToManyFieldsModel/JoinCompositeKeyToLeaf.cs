﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
