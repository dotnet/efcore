// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel
{
    public class ReferencedEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<PrincipalEntity> Principals { get; set; }
    }
}
