// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

public class EntityLeaf : EntityBranch
{
    public bool? IsGreen;

    public ICollection<EntityCompositeKey> CompositeKeySkipFull = null!;
    public ICollection<JoinCompositeKeyToLeaf> JoinCompositeKeyFull = null!;
}
