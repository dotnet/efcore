// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityOne
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public EntityTwo Reference { get; set; }
        public List<EntityTwo> Collection { get; set; }
        public List<EntityTwo> TwoFullySpecified { get; set; }
        public List<EntityThree> ThreeFullySpecifiedWithPayload { get; set; }

        public List<EntityTwo> TwoSharedType { get; set; }
        public List<EntityThree> ThreeSharedType { get; set; }
        public List<EntityOne> SelfSharedTypeLeftWithPayload { get; set; }
        public List<EntityOne> SelfSharedTypeRightWithPayload { get; set; }

        public List<EntityBranch> BranchFullySpecified { get; set; }
    }
}
