// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class JoinOneSelfPayload
    {
        public virtual int LeftId { get; set; }
        public virtual int RightId { get; set; }
        public virtual DateTime Payload { get; set; }
        public virtual EntityOne Right { get; set; }
        public virtual EntityOne Left { get; set; }
    }
}
