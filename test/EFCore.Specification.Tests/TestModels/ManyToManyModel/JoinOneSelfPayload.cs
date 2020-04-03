// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class JoinOneSelfPayload
    {
        public int LeftId { get; set; }
        public int RightId { get; set; }
        public DateTime Payload { get; set; }
        public EntityOne Right { get; set; }
        public EntityOne Left { get; set; }
    }
}
