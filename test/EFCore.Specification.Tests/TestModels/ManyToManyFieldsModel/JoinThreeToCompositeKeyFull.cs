// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

public class JoinThreeToCompositeKeyFull
{
    public Guid Id;
    public int ThreeId;
    public int CompositeId1;
    public string CompositeId2 = null!;
    public DateTime CompositeId3;

    public EntityThree Three = null!;
    public EntityCompositeKey Composite = null!;
}
