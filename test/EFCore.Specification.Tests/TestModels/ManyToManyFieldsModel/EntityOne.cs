// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class EntityOne
    {
        public int Id;
        public string Name;

        public EntityTwo Reference;
        public ICollection<EntityTwo> Collection;
        public ICollection<EntityTwo> TwoSkip;
        public ICollection<EntityThree> ThreeSkipPayloadFull;
        public ICollection<JoinOneToThreePayloadFull> JoinThreePayloadFull;
        public ICollection<EntityTwo> TwoSkipShared;
        public ICollection<EntityThree> ThreeSkipPayloadFullShared;
        public ICollection<Dictionary<string, object>> JoinThreePayloadFullShared;
        public ICollection<EntityOne> SelfSkipPayloadLeft;
        public ICollection<JoinOneSelfPayload> JoinSelfPayloadLeft;
        public ICollection<EntityOne> SelfSkipPayloadRight;
        public ICollection<JoinOneSelfPayload> JoinSelfPayloadRight;
        public ICollection<EntityBranch> BranchSkip;
    }
}
