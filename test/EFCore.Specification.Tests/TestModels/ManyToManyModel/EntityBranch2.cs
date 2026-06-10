// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public abstract class EntityBranch2 : EntityRoot
{
    public virtual long Slumber { get; set; }
    public virtual ICollection<EntityLeaf2> Leaf2SkipShared { get; set; }

    public virtual ICollection<EntityBranch2> SelfSkipSharedLeft { get; set; }
    public virtual ICollection<EntityBranch2> SelfSkipSharedRight { get; set; }
}
