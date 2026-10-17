// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

public class EntityOne
{
    public int Id;
    public string? Name;

    public EntityTwo Reference = null!;
    public ICollection<EntityTwo> Collection = null!;
    public ICollection<EntityTwo> TwoSkip = null!;
    public ICollection<EntityThree> ThreeSkipPayloadFull = null!;
    public ICollection<JoinOneToThreePayloadFull> JoinThreePayloadFull = null!;
    public ICollection<EntityTwo> TwoSkipShared = null!;
    public ICollection<EntityThree> ThreeSkipPayloadFullShared = null!;
    public ICollection<Dictionary<string, object>> JoinThreePayloadFullShared = null!;
    public ICollection<EntityOne> SelfSkipPayloadLeft = null!;
    public ICollection<JoinOneSelfPayload> JoinSelfPayloadLeft = null!;
    public ICollection<EntityOne> SelfSkipPayloadRight = null!;
    public ICollection<JoinOneSelfPayload> JoinSelfPayloadRight = null!;
    public ICollection<EntityBranch> BranchSkip = null!;
}
