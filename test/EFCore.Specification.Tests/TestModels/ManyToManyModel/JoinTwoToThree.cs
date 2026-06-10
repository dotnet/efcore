// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class JoinTwoToThree
{
    public virtual int TwoId { get; set; }
    public virtual int ThreeId { get; set; }
    public virtual EntityTwo Two { get; set; }
    public virtual EntityThree Three { get; set; }
}
