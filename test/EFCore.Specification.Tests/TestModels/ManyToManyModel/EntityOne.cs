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

        public List<EntityTwo> TwoSkip { get; set; }

        public List<EntityThree> ThreeSkipPayloadFull { get; set; }
        public List<JoinOneToThreePayloadFull> JoinThreePayloadFull { get; set; }

        public List<EntityTwo> TwoSkipShared { get; set; }

        public List<EntityThree> ThreeSkipPayloadFullShared { get; set; }
        public List<JoinOneToThreePayloadFullShared> JoinThreePayloadFullShared { get; set; }

        public List<EntityOne> SelfSkipPayloadLeft { get; set; }
        public List<JoinOneSelfPayload> JoinSelfPayloadLeft { get; set; }
        public List<EntityOne> SelfSkipPayloadRight { get; set; }
        public List<JoinOneSelfPayload> JoinSelfPayloadRight { get; set; }

        public List<EntityBranch> BranchSkip { get; set; }
    }
}
