// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityOne
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual EntityTwo Reference { get; set; }
        public virtual ICollection<EntityTwo> Collection { get; } = new ObservableCollection<EntityTwo>(); // #21684

        public virtual ICollection<EntityTwo> TwoSkip { get; } = new ObservableCollection<EntityTwo>(); // #21684

        public virtual ICollection<EntityThree> ThreeSkipPayloadFull { get; } = new ObservableCollection<EntityThree>(); // #21684

        public virtual ICollection<JoinOneToThreePayloadFull> JoinThreePayloadFull { get; }
            = new ObservableCollection<JoinOneToThreePayloadFull>(); // #21684

        [InverseProperty("OneSkipShared")]
        public virtual ICollection<EntityTwo> TwoSkipShared { get; } = new ObservableCollection<EntityTwo>(); // #21684

        public virtual ICollection<EntityThree> ThreeSkipPayloadFullShared { get; } = new ObservableCollection<EntityThree>(); // #21684

        public virtual ICollection<Dictionary<string, object>> JoinThreePayloadFullShared { get; }
             = new ObservableCollection<Dictionary<string, object>>(); // #21684

        public virtual ICollection<EntityOne> SelfSkipPayloadLeft { get; } = new ObservableCollection<EntityOne>(); // #21684

        public virtual ICollection<JoinOneSelfPayload> JoinSelfPayloadLeft { get; }
            = new ObservableCollection<JoinOneSelfPayload>(); // #21684

        public virtual ICollection<EntityOne> SelfSkipPayloadRight { get; } = new ObservableCollection<EntityOne>(); // #21684

        public virtual ICollection<JoinOneSelfPayload> JoinSelfPayloadRight { get; }
            = new ObservableCollection<JoinOneSelfPayload>(); // #21684

        public virtual ICollection<EntityBranch> BranchSkip { get; } = new ObservableCollection<EntityBranch>(); // #21684
    }
}
