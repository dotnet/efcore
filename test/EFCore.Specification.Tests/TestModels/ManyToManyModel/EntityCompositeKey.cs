// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class EntityCompositeKey
{
    public virtual int Key1 { get; set; }
    public virtual string Key2 { get; set; }
    public virtual DateTime Key3 { get; set; }

    public virtual string Name { get; set; }

    public virtual ICollection<EntityTwo> TwoSkipShared { get; set; }
    public virtual ICollection<EntityThree> ThreeSkipFull { get; set; }
    public virtual ICollection<JoinThreeToCompositeKeyFull> JoinThreeFull { get; set; }
    public virtual ICollection<EntityRoot> RootSkipShared { get; set; }
    public virtual ICollection<EntityLeaf> LeafSkipFull { get; set; }
    public virtual ICollection<JoinCompositeKeyToLeaf> JoinLeafFull { get; set; }
}
