// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityTwo
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual int? ReferenceInverseId { get; set; }
        public virtual EntityOne ReferenceInverse { get; set; }

        public virtual int? CollectionInverseId { get; set; }
        public virtual EntityOne CollectionInverse { get; set; }

        public virtual EntityThree Reference { get; set; }
        public virtual ICollection<EntityThree> Collection { get; set; }
        public virtual ICollection<EntityOne> OneSkip { get; set; }
        public virtual ICollection<EntityThree> ThreeSkipFull { get; set; }
        public virtual ICollection<JoinTwoToThree> JoinThreeFull { get; set; }
        public virtual ICollection<EntityTwo> SelfSkipSharedLeft { get; set; }
        public virtual ICollection<EntityTwo> SelfSkipSharedRight { get; set; }

        [InverseProperty("TwoSkipShared")]
        public virtual ICollection<EntityOne> OneSkipShared { get; set; }

        public virtual ICollection<EntityCompositeKey> CompositeKeySkipShared { get; set; }
    }
}
