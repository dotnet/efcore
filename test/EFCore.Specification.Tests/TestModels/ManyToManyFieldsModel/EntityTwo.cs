// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class EntityTwo
    {
        public int Id;
        public string Name;

        public int? ReferenceInverseId;
        public EntityOne ReferenceInverse;

        public int? CollectionInverseId;
        public EntityOne CollectionInverse;

        public EntityThree Reference;
        public ICollection<EntityThree> Collection;
        public ICollection<EntityOne> OneSkip;
        public ICollection<EntityThree> ThreeSkipFull;
        public ICollection<JoinTwoToThree> JoinThreeFull;
        public ICollection<EntityTwo> SelfSkipSharedLeft;
        public ICollection<EntityTwo> SelfSkipSharedRight;

        [InverseProperty("TwoSkipShared")]
        public ICollection<EntityOne> OneSkipShared;

        public ICollection<EntityCompositeKey> CompositeKeySkipShared;
    }
}
