// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships
{
    public class BaseInheritanceRelationshipEntity
    {
        [NotMapped]
        public int Id { get; set; }

        public string Name { get; set; }

        [NotMapped]
        public DerivedInheritanceRelationshipEntity DerivedSefReferenceOnBase { get; set; }

        [NotMapped]
        public BaseReferenceOnBase BaseReferenceOnBase { get; set; }

        [NotMapped]
        public ReferenceOnBase ReferenceOnBase { get; set; }

        [NotMapped]
        public List<BaseCollectionOnBase> BaseCollectionOnBase { get; set; }

        [NotMapped]
        public List<CollectionOnBase> CollectionOnBase { get; set; }
    }
}
