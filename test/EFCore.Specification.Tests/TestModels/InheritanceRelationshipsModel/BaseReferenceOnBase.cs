// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel
{
    public class BaseReferenceOnBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? BaseParentId { get; set; }
        public BaseInheritanceRelationshipEntity BaseParent { get; set; }

        public NestedReferenceBase NestedReference { get; set; }

        public List<NestedCollectionBase> NestedCollection { get; set; }
    }
}
