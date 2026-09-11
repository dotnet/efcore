// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

public class UnidirectionalEntityOne
{
    public virtual int Id { get; set; }
    public virtual string? Name { get; set; }

    public virtual UnidirectionalEntityTwo Reference { get; set; } = null!;
    public virtual ICollection<UnidirectionalEntityTwo> Collection { get; set; } = null!;
    public virtual ICollection<UnidirectionalEntityTwo> TwoSkip { get; set; } = null!;
    public virtual ICollection<UnidirectionalJoinOneToThreePayloadFull> JoinThreePayloadFull { get; set; } = null!;

    public virtual ICollection<UnidirectionalEntityTwo> TwoSkipShared { get; set; } = null!;

    public virtual ICollection<UnidirectionalEntityThree> ThreeSkipPayloadFullShared { get; set; } = null!;
    public virtual ICollection<Dictionary<string, object>> JoinThreePayloadFullShared { get; set; } = null!;
    public virtual ICollection<UnidirectionalEntityOne> SelfSkipPayloadLeft { get; set; } = null!;
    public virtual ICollection<UnidirectionalJoinOneSelfPayload> JoinSelfPayloadLeft { get; set; } = null!;
    public virtual ICollection<UnidirectionalJoinOneSelfPayload> JoinSelfPayloadRight { get; set; } = null!;
    public virtual ICollection<UnidirectionalEntityBranch> BranchSkip { get; set; } = null!;
}
