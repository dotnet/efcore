// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

public class EntityOne
{
    public virtual int Id { get; set; }
    public virtual string? Name { get; set; }

    public virtual EntityTwo Reference { get; set; } = null!;
    public virtual ICollection<EntityTwo> Collection { get; set; } = null!;
    public virtual ICollection<EntityTwo> TwoSkip { get; set; } = null!;
    public virtual ICollection<EntityThree> ThreeSkipPayloadFull { get; set; } = null!;
    public virtual ICollection<JoinOneToThreePayloadFull> JoinThreePayloadFull { get; set; } = null!;

    [InverseProperty("OneSkipShared")]
    public virtual ICollection<EntityTwo> TwoSkipShared { get; set; } = null!;

    public virtual ICollection<EntityThree> ThreeSkipPayloadFullShared { get; set; } = null!;
    public virtual ICollection<Dictionary<string, object>> JoinThreePayloadFullShared { get; set; } = null!;
    public virtual ICollection<EntityOne> SelfSkipPayloadLeft { get; set; } = null!;
    public virtual ICollection<JoinOneSelfPayload> JoinSelfPayloadLeft { get; set; } = null!;
    public virtual ICollection<EntityOne> SelfSkipPayloadRight { get; set; } = null!;
    public virtual ICollection<JoinOneSelfPayload> JoinSelfPayloadRight { get; set; } = null!;
    public virtual ICollection<EntityBranch> BranchSkip { get; set; } = null!;
}
