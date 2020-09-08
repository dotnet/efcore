// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityOne
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual EntityTwo Reference { get; set; }
        public virtual ICollection<EntityTwo> Collection { get; set; }
        public virtual ICollection<EntityTwo> TwoSkip { get; set; }
        public virtual ICollection<EntityThree> ThreeSkipPayloadFull { get; set; }
        public virtual ICollection<JoinOneToThreePayloadFull> JoinThreePayloadFull { get; set; }

        [InverseProperty("OneSkipShared")]
        public virtual ICollection<EntityTwo> TwoSkipShared { get; set; }

        public virtual ICollection<EntityThree> ThreeSkipPayloadFullShared { get; set; }
        public virtual ICollection<Dictionary<string, object>> JoinThreePayloadFullShared { get; set; }
        public virtual ICollection<EntityOne> SelfSkipPayloadLeft { get; set; }
        public virtual ICollection<JoinOneSelfPayload> JoinSelfPayloadLeft { get; set; }
        public virtual ICollection<EntityOne> SelfSkipPayloadRight { get; set; }
        public virtual ICollection<JoinOneSelfPayload> JoinSelfPayloadRight { get; set; }
        public virtual ICollection<EntityBranch> BranchSkip { get; set; }
    }
}
