// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class EntityOne
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }

    public virtual EntityTwo Reference { get; set; }
    public virtual ICollection<EntityTwo> Collection { get; set; }
    public virtual ICollection<EntityTwo> TwoSkip { get; set; }
    public virtual ICollection<EntityThree> ThreeSkipPayloadFull { get; set; }
    public virtual ICollection<JoinOneToThreePayloadFull> JoinThreePayloadFull { get; set; }

    [InverseProperty("OneSkipShared")]
    public virtual ICollection<EntityTwo> TwoSkipShared { get; set; }

    public virtual ICollection<EntityThree> ThreeSkipPayloadFullShared { get; set; }
    public virtual ICollection<Dictionary<string, object>> JoinThreePayloadFullShared { get; set; }
    public virtual ICollection<EntityOne> SelfSkipPayloadLeft { get; set; }
    public virtual ICollection<JoinOneSelfPayload> JoinSelfPayloadLeft { get; set; }
    public virtual ICollection<EntityOne> SelfSkipPayloadRight { get; set; }
    public virtual ICollection<JoinOneSelfPayload> JoinSelfPayloadRight { get; set; }
    public virtual ICollection<EntityBranch> BranchSkip { get; set; }
}
