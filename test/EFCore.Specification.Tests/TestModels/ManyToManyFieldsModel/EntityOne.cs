// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class EntityOne
{
    public int Id;
    public string Name;

    public EntityTwo Reference;
    public ICollection<EntityTwo> Collection;
    public ICollection<EntityTwo> TwoSkip;
    public ICollection<EntityThree> ThreeSkipPayloadFull;
    public ICollection<JoinOneToThreePayloadFull> JoinThreePayloadFull;
    public ICollection<EntityTwo> TwoSkipShared;
    public ICollection<EntityThree> ThreeSkipPayloadFullShared;
    public ICollection<Dictionary<string, object>> JoinThreePayloadFullShared;
    public ICollection<EntityOne> SelfSkipPayloadLeft;
    public ICollection<JoinOneSelfPayload> JoinSelfPayloadLeft;
    public ICollection<EntityOne> SelfSkipPayloadRight;
    public ICollection<JoinOneSelfPayload> JoinSelfPayloadRight;
    public ICollection<EntityBranch> BranchSkip;
}
