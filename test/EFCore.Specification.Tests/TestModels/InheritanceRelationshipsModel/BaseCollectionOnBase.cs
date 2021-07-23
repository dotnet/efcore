// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel
{
    public class BaseCollectionOnBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? BaseParentId { get; set; }
        public BaseInheritanceRelationshipEntity BaseParent { get; set; }

        public NestedReferenceBase NestedReference { get; set; }

        public List<NestedCollectionBase> NestedCollection { get; set; }
    }
}
