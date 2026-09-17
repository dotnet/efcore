// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

public class EntityTwo
{
    public virtual int Id { get; set; }
    public virtual string? Name { get; set; }

    public virtual int? ReferenceInverseId { get; set; }
    public virtual EntityOne? ReferenceInverse { get; set; }

    public virtual int? CollectionInverseId { get; set; }
    public virtual EntityOne? CollectionInverse { get; set; }

    public virtual EntityThree Reference { get; set; } = null!;
    public virtual ICollection<EntityThree> Collection { get; set; } = null!;
    public virtual ICollection<EntityOne> OneSkip { get; set; } = null!;
    public virtual ICollection<EntityThree> ThreeSkipFull { get; set; } = null!;
    public virtual ICollection<JoinTwoToThree> JoinThreeFull { get; set; } = null!;
    public virtual ICollection<EntityTwo> SelfSkipSharedLeft { get; set; } = null!;
    public virtual ICollection<EntityTwo> SelfSkipSharedRight { get; set; } = null!;

    [InverseProperty("TwoSkipShared")]
    public virtual ICollection<EntityOne> OneSkipShared { get; set; } = null!;

    public virtual ICollection<EntityCompositeKey> CompositeKeySkipShared { get; set; } = null!;

    public virtual int? ExtraId { get; set; }
    public virtual JoinOneToTwoExtra? Extra { get; set; }
}
