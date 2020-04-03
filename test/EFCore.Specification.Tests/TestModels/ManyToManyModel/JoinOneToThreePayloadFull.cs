// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class JoinOneToThreePayloadFull
    {
        public int OneId { get; set; }
        public int ThreeId { get; set; }
        public EntityOne One { get; set; }
        public EntityThree Three { get; set; }

        public string Payload { get; set; }
    }
}
