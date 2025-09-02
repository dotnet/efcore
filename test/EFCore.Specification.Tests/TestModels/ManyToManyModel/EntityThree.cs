// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class EntityThree
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }

    public virtual int? ReferenceInverseId { get; set; }
    public virtual EntityTwo ReferenceInverse { get; set; }

    public virtual int? CollectionInverseId { get; set; }
    public virtual EntityTwo CollectionInverse { get; set; }

    public virtual ICollection<EntityOne> OneSkipPayloadFull { get; set; }
    public virtual ICollection<JoinOneToThreePayloadFull> JoinOnePayloadFull { get; set; }
    public virtual ICollection<EntityTwo> TwoSkipFull { get; set; }
    public virtual ICollection<JoinTwoToThree> JoinTwoFull { get; set; }
    public virtual ICollection<EntityOne> OneSkipPayloadFullShared { get; set; }
    public virtual ICollection<Dictionary<string, object>> JoinOnePayloadFullShared { get; set; }
    public virtual ICollection<EntityCompositeKey> CompositeKeySkipFull { get; set; }
    public virtual ICollection<JoinThreeToCompositeKeyFull> JoinCompositeKeyFull { get; set; }

    [InverseProperty("ThreeSkipShared")]
    public virtual ICollection<EntityRoot> RootSkipShared { get; set; }
}
