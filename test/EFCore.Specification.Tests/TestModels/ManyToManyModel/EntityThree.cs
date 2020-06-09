// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class EntityThree
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int? ReferenceInverseId { get; set; }
        public EntityTwo ReferenceInverse { get; set; }

        public int? CollectionInverseId { get; set; }
        public EntityTwo CollectionInverse { get; set; }

        public List<EntityOne> OneSkipPayloadFull { get; set; }
        public List<JoinOneToThreePayloadFull> JoinOnePayloadFull { get; set; }
        public List<EntityTwo> TwoSkipFull { get; set; }
        public List<JoinTwoToThree> JoinTwoFull { get; set; }

        public List<EntityOne> OneSkipPayloadFullShared { get; set; }
        public List<JoinOneToThreePayloadFullShared> JoinOnePayloadFullShared { get; set; }
        public List<EntityCompositeKey> CompositeKeySkipFull { get; set; }
        public List<JoinThreeToCompositeKeyFull> JoinCompositeKeyFull { get; set; }
        public List<EntityRoot> RootSkipShared { get; set; }
    }
}
