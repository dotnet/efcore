// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class EntityRoot
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }
    public virtual ICollection<EntityThree> ThreeSkipShared { get; set; }
    public virtual ICollection<EntityCompositeKey> CompositeKeySkipShared { get; set; }
    public virtual ICollection<EntityBranch> BranchSkipShared { get; set; }
}
