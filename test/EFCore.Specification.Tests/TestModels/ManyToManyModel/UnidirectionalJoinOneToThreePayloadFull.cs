// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class UnidirectionalJoinOneToThreePayloadFull
{
    public virtual int OneId { get; set; }
    public virtual int ThreeId { get; set; }
    public virtual UnidirectionalEntityOne One { get; set; }
    public virtual UnidirectionalEntityThree Three { get; set; }

    public virtual string Payload { get; set; }
}
