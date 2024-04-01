﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class UnidirectionalJoinOneToTwoExtra
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }

    public virtual ICollection<UnidirectionalJoinOneToTwo> JoinEntities { get; set; }
}
