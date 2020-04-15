// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    // TODO: remove and use shared type instead
    public class JoinOneToTwoShared
    {
        public int OneId { get; set; }
        public int TwoId { get; set; }
    }

    public class JoinOneToThreePayloadFullShared
    {
        public int OneId { get; set; }
        public int ThreeId { get; set; }
        public string Payload { get; set; }
    }

    public class JoinTwoSelfShared
    {
        public int LeftId { get; set; }
        public int RightId { get; set; }
    }

    public class JoinTwoToCompositeKeyShared
    {
        public int TwoId { get; set; }
        public int CompositeId1 { get; set; }
        public string CompositeId2 { get; set; }
        public DateTime CompositeId3 { get; set; }
    }

    public class JoinThreeToRootShared
    {
        public int ThreeId { get; set; }
        public int RootId { get; set; }
    }

    public class JoinCompositeKeyToRootShared
    {
        public int CompositeId1 { get; set; }
        public string CompositeId2 { get; set; }
        public DateTime CompositeId3 { get; set; }
        public int RootId { get; set; }
    }
}
