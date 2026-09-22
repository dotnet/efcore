// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class UnidirectionalJoinOneSelfPayload
{
    public virtual int LeftId { get; set; }
    public virtual int RightId { get; set; }
    public virtual DateTime Payload { get; set; }
    public virtual UnidirectionalEntityOne Right { get; set; }
    public virtual UnidirectionalEntityOne Left { get; set; }
}
