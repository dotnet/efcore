// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    // TODO: remove and use shared type instead
    public class JoinTwoToCompositeKeyShared
    {
        public virtual int TwoId { get; set; }
        public virtual int CompositeId1 { get; set; }
        public virtual string CompositeId2 { get; set; }
        public virtual DateTime CompositeId3 { get; set; }
    }

    public class JoinCompositeKeyToRootShared
    {
        public virtual int CompositeId1 { get; set; }
        public virtual string CompositeId2 { get; set; }
        public virtual DateTime CompositeId3 { get; set; }
        public virtual int RootId { get; set; }
    }
}
