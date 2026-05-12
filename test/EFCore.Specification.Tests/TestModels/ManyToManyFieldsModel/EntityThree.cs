// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class EntityThree
{
    public int Id;
    public string Name;

    public int? ReferenceInverseId;
    public EntityTwo ReferenceInverse;

    public int? CollectionInverseId;
    public EntityTwo CollectionInverse;

    public ICollection<EntityOne> OneSkipPayloadFull;
    public ICollection<JoinOneToThreePayloadFull> JoinOnePayloadFull;
    public ICollection<EntityTwo> TwoSkipFull;
    public ICollection<JoinTwoToThree> JoinTwoFull;
    public ICollection<EntityOne> OneSkipPayloadFullShared;
    public ICollection<Dictionary<string, object>> JoinOnePayloadFullShared;
    public ICollection<EntityCompositeKey> CompositeKeySkipFull;
    public ICollection<JoinThreeToCompositeKeyFull> JoinCompositeKeyFull;
    public ICollection<EntityRoot> RootSkipShared;
}
