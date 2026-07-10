// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

#nullable disable

public class JoinOneToTwo
{
    public virtual int OneId { get; set; }
    public virtual int TwoId { get; set; }

    public virtual EntityOne One { get; set; }
    public virtual EntityTwo Two { get; set; }
}
