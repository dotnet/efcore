// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ManyToManyFieldsModel;

#nullable disable

public class JoinOneSelfPayload
{
    public int LeftId;
    public int RightId;
    public DateTime Payload;
    public EntityOne Right;
    public EntityOne Left;
}
