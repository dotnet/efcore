// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

public class EntityThree
{
    public int Id;
    public string? Name;

    public int? ReferenceInverseId;
    public EntityTwo? ReferenceInverse;

    public int? CollectionInverseId;
    public EntityTwo? CollectionInverse;

    public ICollection<EntityOne> OneSkipPayloadFull = null!;
    public ICollection<JoinOneToThreePayloadFull> JoinOnePayloadFull = null!;
    public ICollection<EntityTwo> TwoSkipFull = null!;
    public ICollection<JoinTwoToThree> JoinTwoFull = null!;
    public ICollection<EntityOne> OneSkipPayloadFullShared = null!;
    public ICollection<Dictionary<string, object>> JoinOnePayloadFullShared = null!;
    public ICollection<EntityCompositeKey> CompositeKeySkipFull = null!;
    public ICollection<JoinThreeToCompositeKeyFull> JoinCompositeKeyFull = null!;
    public ICollection<EntityRoot> RootSkipShared = null!;
}
