// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class UnidirectionalEntityTwo
{
    public virtual int Id { get; set; }
    public virtual string Name { get; set; }

    public virtual int? ReferenceInverseId { get; set; }
    public virtual UnidirectionalEntityOne ReferenceInverse { get; set; }

    public virtual int? CollectionInverseId { get; set; }
    public virtual UnidirectionalEntityOne CollectionInverse { get; set; }

    public virtual UnidirectionalEntityThree Reference { get; set; }
    public virtual ICollection<UnidirectionalEntityThree> Collection { get; set; }
    public virtual ICollection<UnidirectionalJoinTwoToThree> JoinThreeFull { get; set; }
    public virtual ICollection<UnidirectionalEntityTwo> SelfSkipSharedRight { get; set; }

    public virtual int? ExtraId { get; set; }
    public virtual UnidirectionalJoinOneToTwoExtra Extra { get; set; }
}
