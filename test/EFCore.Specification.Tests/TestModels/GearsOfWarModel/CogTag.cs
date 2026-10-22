// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class CogTag
{
    // auto generated key (identity for now)
    public Guid Id { get; set; }

    public string Note { get; set; }

    public string GearNickName { get; set; }
    public int? GearSquadId { get; set; }
    public virtual Gear Gear { get; set; }
    public DateTime IssueDate { get; set; }
}
