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

        public List<EntityOne> OneFullySpecifiedWithPayload { get; set; }
        public List<EntityTwo> TwoFullySpecified { get; set; }

        public List<EntityOne> OneSharedType { get; set; }
        public List<EntityCompositeKey> CompositeFullySpecified { get; set; }
        public List<EntityRoot> RootSharedType { get; set; }
    }
}
