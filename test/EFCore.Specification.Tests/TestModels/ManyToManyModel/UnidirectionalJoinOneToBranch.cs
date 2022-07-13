// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

public class UnidirectionalJoinOneToBranch
{
    public virtual int UnidirectionalEntityOneId { get; set; }
    public virtual int UnidirectionalEntityBranchId { get; set; }
}
