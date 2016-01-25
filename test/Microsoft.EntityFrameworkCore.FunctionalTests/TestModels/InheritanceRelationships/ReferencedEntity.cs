// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.InheritanceRelationships
{
    public class ReferencedEntity
    {
        public int Id { get; set; }

        public ICollection<PrincipalEntity> Principals { get; set; }
    }
}
