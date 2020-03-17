// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    // TODO: remove and use shared type instead
    public class OneToTwoSharedType
    {
        public int OneId { get; set; }
        public int TwoId { get; set; }

        public EntityOne One { get; set; }
        public EntityTwo Two { get; set; }
    }

    public class OneToThreeSharedType
    {
        public int OneId { get; set; }
        public int ThreeId { get; set; }

        public EntityOne One { get; set; }
        public EntityThree Three { get; set; }
    }

    public class OneSelfSharedTypeWithPayload
    {
        public int LeftId { get; set; }
        public int RightId { get; set; }

        public EntityOne Left { get; set; }
        public EntityOne Right { get; set; }

        public DateTime Payload { get; set; }
    }

    public class TwoToCompositeSharedType
    {
        public int TwoId { get; set; }
        public int CompositeId1 { get; set; }
        public string CompositeId2 { get; set; }
        public DateTime CompositeId3 { get; set; }

        public EntityTwo Two { get; set; }
        public EntityCompositeKey Composite { get; set; }
    }

    public class ThreeToRootSharedType
    {
        public int ThreeId { get; set; }
        public int RootId { get; set; }

        public EntityThree Three { get; set; }
        public EntityRoot Root { get; set; }
    }

    public class CompositeToRootSharedType
    {
        public int CompositeId1 { get; set; }
        public string CompositeId2 { get; set; }
        public DateTime CompositeId3 { get; set; }
        public int RootId { get; set; }

        public EntityCompositeKey Composite { get; set; }
        public EntityRoot Root { get; set; }
    }
}
