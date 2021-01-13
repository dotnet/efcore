// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel
{
    public class BaseInheritanceRelationshipEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DerivedInheritanceRelationshipEntity DerivedSefReferenceOnBase { get; set; }
        public BaseReferenceOnBase BaseReferenceOnBase { get; set; }
        public ReferenceOnBase ReferenceOnBase { get; set; }
        public OwnedEntity OwnedReferenceOnBase { get; set; }

        public List<BaseCollectionOnBase> BaseCollectionOnBase { get; set; }
        public List<CollectionOnBase> CollectionOnBase { get; set; }
        public List<OwnedEntity> OwnedCollectionOnBase { get; set; }
    }
}
