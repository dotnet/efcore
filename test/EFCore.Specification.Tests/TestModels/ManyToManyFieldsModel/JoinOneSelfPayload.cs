// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class JoinOneSelfPayload
    {
        public int LeftId;
        public int RightId;
        public DateTime Payload;
        public EntityOne Right;
        public EntityOne Left;
    }
}
