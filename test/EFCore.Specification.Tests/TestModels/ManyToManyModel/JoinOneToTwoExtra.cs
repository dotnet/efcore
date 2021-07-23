﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel
{
    public class JoinOneToTwoExtra
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }

        public virtual ICollection<JoinOneToTwo> JoinEntities { get; set; }
    }
}
