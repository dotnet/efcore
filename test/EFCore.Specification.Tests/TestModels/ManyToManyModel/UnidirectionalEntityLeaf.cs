// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class UnidirectionalEntityLeaf : UnidirectionalEntityBranch
{
    public virtual bool? IsGreen { get; set; }

    public virtual ICollection<UnidirectionalEntityCompositeKey> CompositeKeySkipFull { get; set; }
    public virtual ICollection<UnidirectionalJoinCompositeKeyToLeaf> JoinCompositeKeyFull { get; set; }
}
