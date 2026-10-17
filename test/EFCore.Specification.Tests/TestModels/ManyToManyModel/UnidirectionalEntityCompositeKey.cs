// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

public class UnidirectionalEntityCompositeKey
{
    public virtual int Key1 { get; set; }
    public virtual string Key2 { get; set; } = null!;
    public virtual DateTime Key3 { get; set; }

    public virtual string? Name { get; set; }

    public virtual ICollection<UnidirectionalEntityTwo> TwoSkipShared { get; set; } = null!;
    public virtual ICollection<UnidirectionalEntityThree> ThreeSkipFull { get; set; } = null!;
    public virtual ICollection<UnidirectionalJoinThreeToCompositeKeyFull> JoinThreeFull { get; set; } = null!;
    public virtual ICollection<UnidirectionalEntityRoot> RootSkipShared { get; set; } = null!;
    public virtual ICollection<UnidirectionalJoinCompositeKeyToLeaf> JoinLeafFull { get; set; } = null!;
}
