// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class JoinOneToThreePayloadFull
{
    public int OneId;
    public int ThreeId;
    public EntityOne One;
    public EntityThree Three;

    public string Payload;
}
