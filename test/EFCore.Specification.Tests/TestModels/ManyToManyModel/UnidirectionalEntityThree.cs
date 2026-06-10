// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class UnidirectionalEntityThree
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }

    public virtual int? ReferenceInverseId { get; set; }
    public virtual UnidirectionalEntityTwo ReferenceInverse { get; set; }

    public virtual int? CollectionInverseId { get; set; }
    public virtual UnidirectionalEntityTwo CollectionInverse { get; set; }

    public virtual ICollection<UnidirectionalJoinOneToThreePayloadFull> JoinOnePayloadFull { get; set; }
    public virtual ICollection<UnidirectionalEntityTwo> TwoSkipFull { get; set; }
    public virtual ICollection<UnidirectionalJoinTwoToThree> JoinTwoFull { get; set; }
    public virtual ICollection<Dictionary<string, object>> JoinOnePayloadFullShared { get; set; }
    public virtual ICollection<UnidirectionalJoinThreeToCompositeKeyFull> JoinCompositeKeyFull { get; set; }
}
