// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class UnidirectionalEntityOne
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }

    public virtual UnidirectionalEntityTwo Reference { get; set; }
    public virtual ICollection<UnidirectionalEntityTwo> Collection { get; set; }
    public virtual ICollection<UnidirectionalEntityTwo> TwoSkip { get; set; }
    public virtual ICollection<UnidirectionalJoinOneToThreePayloadFull> JoinThreePayloadFull { get; set; }

    public virtual ICollection<UnidirectionalEntityTwo> TwoSkipShared { get; set; }

    public virtual ICollection<UnidirectionalEntityThree> ThreeSkipPayloadFullShared { get; set; }
    public virtual ICollection<Dictionary<string, object>> JoinThreePayloadFullShared { get; set; }
    public virtual ICollection<UnidirectionalEntityOne> SelfSkipPayloadLeft { get; set; }
    public virtual ICollection<UnidirectionalJoinOneSelfPayload> JoinSelfPayloadLeft { get; set; }
    public virtual ICollection<UnidirectionalJoinOneSelfPayload> JoinSelfPayloadRight { get; set; }
    public virtual ICollection<UnidirectionalEntityBranch> BranchSkip { get; set; }
}
