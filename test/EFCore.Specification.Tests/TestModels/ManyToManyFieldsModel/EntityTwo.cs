// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

public class EntityTwo
{
    public int Id;
    public string? Name;

    public int? ReferenceInverseId;
    public EntityOne? ReferenceInverse;

    public int? CollectionInverseId;
    public EntityOne? CollectionInverse;

    public EntityThree Reference = null!;
    public ICollection<EntityThree> Collection = null!;
    public ICollection<EntityOne> OneSkip = null!;
    public ICollection<EntityThree> ThreeSkipFull = null!;
    public ICollection<JoinTwoToThree> JoinThreeFull = null!;
    public ICollection<EntityTwo> SelfSkipSharedLeft = null!;
    public ICollection<EntityTwo> SelfSkipSharedRight = null!;

    [InverseProperty("TwoSkipShared")]
    public ICollection<EntityOne> OneSkipShared = null!;

    public ICollection<EntityCompositeKey> CompositeKeySkipShared = null!;
}
