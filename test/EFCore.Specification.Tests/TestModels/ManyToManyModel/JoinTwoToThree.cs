// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class JoinTwoToThree
    {
        public virtual int TwoId { get; set; }
        public virtual int ThreeId { get; set; }
        public virtual EntityTwo Two { get; set; }
        public virtual EntityThree Three { get; set; }
    }
}
