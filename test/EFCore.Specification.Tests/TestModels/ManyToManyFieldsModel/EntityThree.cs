// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel
{
    public class EntityThree
    {
        public int Id;
        public string Name;

        public int? ReferenceInverseId;
        public EntityTwo ReferenceInverse;

        public int? CollectionInverseId;
        public EntityTwo CollectionInverse;

        public ICollection<EntityOne> OneSkipPayloadFull;
        public ICollection<JoinOneToThreePayloadFull> JoinOnePayloadFull;
        public ICollection<EntityTwo> TwoSkipFull;
        public ICollection<JoinTwoToThree> JoinTwoFull;
        public ICollection<EntityOne> OneSkipPayloadFullShared;
        public ICollection<Dictionary<string, object>> JoinOnePayloadFullShared;
        public ICollection<EntityCompositeKey> CompositeKeySkipFull;
        public ICollection<JoinThreeToCompositeKeyFull> JoinCompositeKeyFull;
        public ICollection<EntityRoot> RootSkipShared;
    }
}
