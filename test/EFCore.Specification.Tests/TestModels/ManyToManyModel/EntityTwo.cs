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
        public List<EntityOne> OneFullySpecified { get; set; }
        public List<EntityThree> ThreeFullySpecified { get; set; }
        public List<EntityTwo> SelfFullySpecifiedLeft { get; set; }
        public List<EntityTwo> SelfFullySpecifiedRight { get; set; }

        public List<EntityOne> OneSharedType { get; set; }
        public List<EntityCompositeKey> CompositeSharedType { get; set; }
    }
}
