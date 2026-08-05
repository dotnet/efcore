// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class EntityTwo
{
    public int Id;
    public string Name;

    public int? ReferenceInverseId;
    public EntityOne ReferenceInverse;

    public int? CollectionInverseId;
    public EntityOne CollectionInverse;

    public EntityThree Reference;
    public ICollection<EntityThree> Collection;
    public ICollection<EntityOne> OneSkip;
    public ICollection<EntityThree> ThreeSkipFull;
    public ICollection<JoinTwoToThree> JoinThreeFull;
    public ICollection<EntityTwo> SelfSkipSharedLeft;
    public ICollection<EntityTwo> SelfSkipSharedRight;

    [InverseProperty("TwoSkipShared")]
    public ICollection<EntityOne> OneSkipShared;

    public ICollection<EntityCompositeKey> CompositeKeySkipShared;
}
