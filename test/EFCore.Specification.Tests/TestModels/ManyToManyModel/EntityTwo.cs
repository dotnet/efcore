// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class EntityTwo
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }

    public virtual int? ReferenceInverseId { get; set; }
    public virtual EntityOne ReferenceInverse { get; set; }

    public virtual int? CollectionInverseId { get; set; }
    public virtual EntityOne CollectionInverse { get; set; }

    public virtual EntityThree Reference { get; set; }
    public virtual ICollection<EntityThree> Collection { get; set; }
    public virtual ICollection<EntityOne> OneSkip { get; set; }
    public virtual ICollection<EntityThree> ThreeSkipFull { get; set; }
    public virtual ICollection<JoinTwoToThree> JoinThreeFull { get; set; }
    public virtual ICollection<EntityTwo> SelfSkipSharedLeft { get; set; }
    public virtual ICollection<EntityTwo> SelfSkipSharedRight { get; set; }

    [InverseProperty("TwoSkipShared")]
    public virtual ICollection<EntityOne> OneSkipShared { get; set; }

    public virtual ICollection<EntityCompositeKey> CompositeKeySkipShared { get; set; }

    public virtual int? ExtraId { get; set; }
    public virtual JoinOneToTwoExtra Extra { get; set; }
}
