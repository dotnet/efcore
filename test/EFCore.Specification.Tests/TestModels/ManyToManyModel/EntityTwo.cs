// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityTwo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? ReferenceInverseId { get; set; }
        public EntityOne ReferenceInverse { get; set; }

        public int? CollectionInverseId { get; set; }
        public EntityOne CollectionInverse { get; set; }

        public EntityThree Reference { get; set; }
        public List<EntityThree> Collection { get; set; }
        public List<EntityOne> OneSkip { get; set; }
        public List<EntityThree> ThreeSkipFull { get; set; }
        public List<JoinTwoToThree> JoinThreeFull { get; set; }
        public List<EntityTwo> SelfSkipSharedLeft { get; set; }
        public List<EntityTwo> SelfSkipSharedRight { get; set; }

        public List<EntityOne> OneSkipShared { get; set; }
        public List<EntityCompositeKey> CompositeKeySkipShared { get; set; }
    }
}
