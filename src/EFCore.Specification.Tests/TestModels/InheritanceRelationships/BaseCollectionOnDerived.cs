// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships
{
    public class BaseCollectionOnDerived
    {
        [NotMapped]
        public int Id { get; set; }

        public string Name { get; set; }

        public int? ParentId { get; set; }
        public DerivedInheritanceRelationshipEntity BaseParent { get; set; }
    }
}
