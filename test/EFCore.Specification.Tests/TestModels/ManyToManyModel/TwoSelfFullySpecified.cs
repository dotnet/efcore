// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class TwoSelfFullySpecified
    {
        public int LeftId { get; set; }
        public int RightId { get; set; }
        public EntityTwo Left { get; set; }
        public EntityTwo Right { get; set; }
    }
}
