// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel
{
    public class BaseReferenceOnDerived
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? BaseParentId { get; set; }
        public DerivedInheritanceRelationshipEntity BaseParent { get; set; }
    }
}
