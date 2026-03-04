// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class UnidirectionalEntityCompositeKey
{
    public virtual int Key1 { get; set; }
    public virtual string Key2 { get; set; }
    public virtual DateTime Key3 { get; set; }

    public virtual string Name { get; set; }

    public virtual ICollection<UnidirectionalEntityTwo> TwoSkipShared { get; set; }
    public virtual ICollection<UnidirectionalEntityThree> ThreeSkipFull { get; set; }
    public virtual ICollection<UnidirectionalJoinThreeToCompositeKeyFull> JoinThreeFull { get; set; }
    public virtual ICollection<UnidirectionalEntityRoot> RootSkipShared { get; set; }
    public virtual ICollection<UnidirectionalJoinCompositeKeyToLeaf> JoinLeafFull { get; set; }
}
