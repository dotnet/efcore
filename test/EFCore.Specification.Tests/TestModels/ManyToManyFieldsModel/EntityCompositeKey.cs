// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

public class EntityCompositeKey
{
    public int Key1;
    public string Key2 = null!;
    public DateTime Key3;

    public string? Name;

    public ICollection<EntityTwo> TwoSkipShared = null!;
    public ICollection<EntityThree> ThreeSkipFull = null!;
    public ICollection<JoinThreeToCompositeKeyFull> JoinThreeFull = null!;
    public ICollection<EntityRoot> RootSkipShared = null!;
    public ICollection<EntityLeaf> LeafSkipFull = null!;
    public ICollection<JoinCompositeKeyToLeaf> JoinLeafFull = null!;
}
