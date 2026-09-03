// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class EntityBranch : EntityRoot
{
    public long Number;
    public ICollection<EntityOne> OneSkip;
    public ICollection<EntityRoot> RootSkipShared;
}
