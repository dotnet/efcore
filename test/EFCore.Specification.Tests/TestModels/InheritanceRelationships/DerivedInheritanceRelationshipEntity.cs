// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships
{
    public class DerivedInheritanceRelationshipEntity : BaseInheritanceRelationshipEntity
    {
        public int? BaseId { get; set; }

        public BaseReferenceOnDerived BaseReferenceOnDerived { get; set; }
        public DerivedReferenceOnDerived DerivedReferenceOnDerived { get; set; }
        public ReferenceOnDerived ReferenceOnDerived { get; set; }
        public BaseInheritanceRelationshipEntity BaseSelfReferenceOnDerived { get; set; }

        public List<BaseCollectionOnDerived> BaseCollectionOnDerived { get; set; }
        public List<DerivedCollectionOnDerived> DerivedCollectionOnDerived { get; set; }
        public List<CollectionOnDerived> CollectionOnDerived { get; set; }
    }
}
