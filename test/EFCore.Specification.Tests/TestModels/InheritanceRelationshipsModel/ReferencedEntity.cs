﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
