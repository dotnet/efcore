// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

public class EntityCompositeKey
{
    public virtual int Key1 { get; set; }
    public virtual string Key2 { get; set; } = null!;
    public virtual DateTime Key3 { get; set; }

    public virtual string? Name { get; set; }

    public virtual ICollection<EntityTwo> TwoSkipShared { get; set; } = null!;
    public virtual ICollection<EntityThree> ThreeSkipFull { get; set; } = null!;
    public virtual ICollection<JoinThreeToCompositeKeyFull> JoinThreeFull { get; set; } = null!;
    public virtual ICollection<EntityRoot> RootSkipShared { get; set; } = null!;
    public virtual ICollection<EntityLeaf> LeafSkipFull { get; set; } = null!;
    public virtual ICollection<JoinCompositeKeyToLeaf> JoinLeafFull { get; set; } = null!;
}
