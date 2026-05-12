// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class EntityLeaf2 : EntityBranch2
{
    public virtual bool? IsBrown { get; set; }
    public virtual ICollection<EntityBranch2> Branch2SkipShared { get; set; }
}
