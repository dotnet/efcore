// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class EntityBranch : EntityRoot
{
    public virtual long Number { get; set; }
    public virtual ICollection<EntityOne> OneSkip { get; set; }
    public virtual ICollection<EntityRoot> RootSkipShared { get; set; }
}
